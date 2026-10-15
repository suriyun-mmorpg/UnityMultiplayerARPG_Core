using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class DataPatchRuntime
    {
        private static UniTaskCompletionSource<Exception> initializationCompletion;
        private static bool initializationStarted;
        private static int runtimeVersion;

        /// <summary>Raised on Unity's main thread after IsLoading becomes true, before downloading/applying data.</summary>
        public static event Action PatchingStarted;

        /// <summary>Raised on Unity's main thread after IsLoading becomes false. Arguments are success and error (empty on success).</summary>
        /// <remarks>Chunk application is complete on success; release identity is finalized separately by Commit.</remarks>
        public static event Action<bool, string> PatchingFinished;

        public static DataPatchEngine Engine { get; private set; }

        public static bool IsLoading { get; private set; }

        public static bool IsReady { get; private set; }

        public static string LastError { get; private set; } = "";
        public static string ActiveReleaseId => Engine?.ActiveReleaseId ?? "";
        public static string ActiveHash { get; private set; } = "";
        public static double LastDownloadMilliseconds { get; private set; }
        public static double LastCleanupMilliseconds { get; private set; }
        public static DataPatchProfile Profile { get; private set; }

        public static void StartInitialization(DataPatchProfile profile, GameDatabase loadedDatabase)
        {
            UniTask initialization = Initialize(profile, loadedDatabase);
            var completion = new UniTaskCompletionSource<Exception>();
            initializationCompletion = completion;
            initializationStarted = true;
            CompleteInitialization(initialization, completion, runtimeVersion).Forget();
        }

        private static async UniTaskVoid CompleteInitialization(UniTask initialization, UniTaskCompletionSource<Exception> completion, int version)
        {
            Exception failure = null;
            try
            {
                await initialization;
            }
            catch (Exception error)
            {
                failure = error;
                if (version == runtimeVersion && LastError != error.Message)
                {
                    LastError = error.Message;
                    Debug.LogError("Game-data patch startup failed: " + error.Message);
                }
            }
            // CompletionSource supports concurrent waiters; failures are data until a gate awaits them.
            completion.TrySetResult(failure);
        }

        public static async UniTask WaitForInitialization(bool requireReady = true)
        {
            int version = runtimeVersion;
            Exception failure = initializationStarted ? await initializationCompletion.Task : null;
            if (version != runtimeVersion)
                throw new OperationCanceledException("Patch runtime was reset.");
            if (failure != null && (requireReady || Engine == null))
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
            // A server-selected exact release can recover a failed latest lookup on the client.
            if (requireReady && !IsReady)
                throw new InvalidOperationException(string.IsNullOrEmpty(LastError) ? "Patch initialization is not ready." : LastError);
        }

        public static async UniTask Initialize(DataPatchProfile database, GameDatabase loadedDatabase)
        {
            Reset();
            if (database == null || !database.dataPatchSettings.enabled)
            {
                IsReady = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(database.patchDatabaseId))
                throw new InvalidOperationException("Assign the patch profile database ID before enabling patches.");
            if (database.database != loadedDatabase)
                throw new InvalidOperationException("Patch profile references a different game database.");
            Profile = database;
            Engine = new DataPatchEngine(database, refresh: false);
            DataPatchEngine initializingEngine = Engine;
            var startup = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                // Register scanned references before menus use GameInstance ID lookups; no JSON snapshot.
                Engine.RefreshRegistry();
                await Prepare(string.Empty, refreshRegistry: false);
                if (Engine != initializingEngine)
                    throw new OperationCanceledException("Patch runtime was reset.");
                Commit();
                IsReady = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[Data Patch] Startup finished in {startup.Elapsed.TotalMilliseconds:F1} ms: download {LastDownloadMilliseconds:F1} ms, preparation {Engine.LastPreparationMilliseconds:F1} ms, cleanup {LastCleanupMilliseconds:F1} ms, activation {Engine.LastCommitMilliseconds:F1} ms.");
#endif
            }
            catch (Exception ex)
            {
                if (Engine == initializingEngine)
                {
                    LastError = ex.Message;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError($"[Data Patch] Startup failed after {startup.Elapsed.TotalMilliseconds:F1} ms; download {LastDownloadMilliseconds:F1} ms. {ex.Message}");
#endif
                }
                throw;
            }
        }

        public static async UniTask Prepare(string id, bool baseline = false, bool refreshRegistry = true, string expectedHash = null)
        {
            if (IsLoading)
                throw new InvalidOperationException("A patch load is already in progress.");
            if (Engine == null)
                throw new InvalidOperationException("Enable data patching on the GameInstance patch profile first.");
            IsLoading = true;
            LastError = "";
            DataPatchEngine engine = Engine;
            DataPatchProfile profile = Profile;
            bool success = false;
            try
            {
                NotifyPatchingStarted();
                if (Engine != engine)
                    throw new OperationCanceledException("Patch runtime was reset.");
                var download = System.Diagnostics.Stopwatch.StartNew();
                DataPatchRelease release;
                try
                {
                    release = baseline ? null : await DataPatchHttp.Load(profile, id);
                }
                finally
                {
                    if (Engine == engine)
                        LastDownloadMilliseconds = download.Elapsed.TotalMilliseconds;
                }
                if (Engine != engine)
                    throw new OperationCanceledException("Patch runtime was reset.");
                if (expectedHash != null && (release?.payloadHash ?? "") != expectedHash)
                    throw new InvalidOperationException("Server/broadcast patch hash does not match the downloaded release.");
                await engine.PrepareAsync(release, refreshRegistry: refreshRegistry, applyImmediately: true);
                try
                {
                    // Chunks are already live; save before confirming the completed release identity.
                    await DataPatchHttp.CachePreparedAsync(profile, release);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Game-data patch prepared, but its cache could not be saved: " + ex.Message);
                }
                if (Engine != engine)
                    throw new OperationCanceledException("Patch runtime was reset.");
                LastError = "";
                success = true;
            }
            catch (Exception ex)
            {
                if (Engine == engine)
                    LastError = ex.Message;
                throw;
            }
            finally
            {
                if (Engine == engine)
                {
                    try
                    {
                        await CleanupTemporaryMemory(engine);
                    }
                    catch (Exception ex)
                    {
                        // Cleanup must not turn an applied patch into a failure or hide its original error.
                        Debug.LogWarning("Game-data patch memory cleanup failed: " + ex.Message);
                    }
                    if (Engine == engine)
                    {
                        IsLoading = false;
                        NotifyPatchingFinished(success, LastError);
                    }
                }
            }
        }

        private static async UniTask CleanupTemporaryMemory(DataPatchEngine engine)
        {
            LastCleanupMilliseconds = 0;
            if (engine.LastParsedChunkCount == 0 && engine.PreparedChangeCount == 0)
                return;
            if (!UnityEngine.Scripting.GarbageCollector.isIncremental ||
                UnityEngine.Scripting.GarbageCollector.GCMode != UnityEngine.Scripting.GarbageCollector.Mode.Enabled)
                return;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            // Let the completed parsing/preparation state machines return their pooled references first.
            await YieldCleanupFrame();
            for (int slice = 0; slice < 60 && Engine == engine && timer.Elapsed.TotalMilliseconds < 1000; ++slice)
            {
                if (UnityEngine.Scripting.GarbageCollector.GCMode != UnityEngine.Scripting.GarbageCollector.Mode.Enabled ||
                    !UnityEngine.Scripting.GarbageCollector.CollectIncremental(2000000))
                    break;
                await YieldCleanupFrame();
            }
            if (Engine == engine)
            {
                LastCleanupMilliseconds = timer.Elapsed.TotalMilliseconds;
                engine.LogMemory("after temporary cleanup");
            }
        }

        private static async UniTask YieldCleanupFrame()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                await UniTask.Delay(1, DelayType.Realtime);
                return;
            }
#endif
            await UniTask.NextFrame();
        }

        private static void NotifyPatchingStarted()
        {
            Action handlers = PatchingStarted;
            if (handlers == null)
                return;
            foreach (Action handler in handlers.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static void NotifyPatchingFinished(bool success, string error)
        {
            Action<bool, string> handlers = PatchingFinished;
            if (handlers == null)
                return;
            foreach (Action<bool, string> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(success, error);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public static void Commit()
        {
            DataPatchRelease release = Engine.PreparedRelease;
            Engine.Commit();
            ActiveHash = release?.payloadHash ?? "";
            IsReady = true;
            LastError = "";
            Debug.Log($"Game-data patch activated: {ActiveReleaseId}");
        }

        public static void Reset()
        {
            ++runtimeVersion;
            initializationStarted = false;
            initializationCompletion = null;
            bool wasLoading = IsLoading;
            try
            {
                Engine?.Dispose();
            }
            finally
            {
                Engine = null;
                Profile = null;
                IsReady = false;
                IsLoading = false;
                LastError = "";
                ActiveHash = "";
                LastDownloadMilliseconds = 0;
                LastCleanupMilliseconds = 0;
                if (wasLoading)
                    NotifyPatchingFinished(false, "Patch runtime was reset.");
            }
        }
    }
}
