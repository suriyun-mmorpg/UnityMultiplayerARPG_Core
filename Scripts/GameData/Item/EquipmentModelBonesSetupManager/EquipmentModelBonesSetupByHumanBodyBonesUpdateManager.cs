using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace MultiplayerARPG
{
    [DefaultExecutionOrder(int.MaxValue)]
    public class EquipmentModelBonesSetupByHumanBodyBonesUpdateManager : MonoBehaviour
    {
        private static EquipmentModelBonesSetupByHumanBodyBonesUpdateManager _instance;
        public static EquipmentModelBonesSetupByHumanBodyBonesUpdateManager Instance => _instance != null ? _instance : (_instance = CreateInstance());

        private static EquipmentModelBonesSetupByHumanBodyBonesUpdateManager CreateInstance()
        {
            var gameObject = new GameObject(nameof(EquipmentModelBonesSetupByHumanBodyBonesUpdateManager))
            {
                hideFlags = HideFlags.DontSave,
            };
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
            else
#endif
            {
                DontDestroyOnLoad(gameObject);
            }
            return gameObject.AddComponent<EquipmentModelBonesSetupByHumanBodyBonesUpdateManager>();
        }

        private readonly Dictionary<int, List<Transform>> _allSrc = new Dictionary<int, List<Transform>>();
        private readonly Dictionary<int, NativeList<TransformData>> _allNSrc = new Dictionary<int, NativeList<TransformData>>();
        private readonly Dictionary<int, DstData> _allDst = new Dictionary<int, DstData>();
        private readonly HashSet<int> _destroyedSrcIds = new HashSet<int>();
        private readonly HashSet<int> _destroyedDstIds = new HashSet<int>();
        private JobHandle _jobHandle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetInstance()
        {
            _instance = null;
        }

        private void OnDestroy()
        {
            _jobHandle.Complete();

            _allSrc.Clear();
            _allDst.Clear();
        }

        public void Register(AnimatorHandle srcHandle, List<Transform> src, AnimatorHandle dstHandle, List<Transform> dst)
        {
            if (srcHandle == null)
                return;
            if (dstHandle == null)
                return;
            srcHandle.OnDestroyed -= OnSrcAnimatorHandleDestroyed;
            srcHandle.OnDestroyed += OnSrcAnimatorHandleDestroyed;
            dstHandle.OnDestroyed -= OnDstAnimatorHandleDestroyed;
            dstHandle.OnDestroyed += OnDstAnimatorHandleDestroyed;
            _allSrc[srcHandle.Id] = src;
            if (!_allNSrc.ContainsKey(srcHandle.Id))
            {
                NativeList<TransformData> srcNList = new NativeList<TransformData>(src.Count, Allocator.Persistent);
                _allNSrc[srcHandle.Id] = srcNList;
            }
            if (!_allDst.ContainsKey(dstHandle.Id))
            {
                TransformAccessArray dstTransforms = new TransformAccessArray(dst.Count);
                foreach (Transform dstTransform in dst)
                {
                    dstTransforms.Add(dstTransform);
                }
                _allDst[dstHandle.Id] = new DstData()
                {
                    dstArray = dstTransforms,
                    srcId = srcHandle.Id,
                };
            }
        }

        private void OnSrcAnimatorHandleDestroyed(AnimatorHandle handle)
        {
            _destroyedSrcIds.Add(handle.Id);
        }

        private void OnDstAnimatorHandleDestroyed(AnimatorHandle handle)
        {
            _destroyedDstIds.Add(handle.Id);
        }

        private void LateUpdate()
        {
            // Ensure previous job is done
            _jobHandle.Complete();

            // Remove destroyed sources
            foreach (int id in _destroyedSrcIds)
            {
                if (_allNSrc.TryGetValue(id, out NativeList<TransformData> srcNList))
                {
                    if (srcNList.IsCreated)
                        srcNList.Dispose();
                    _allNSrc.Remove(id);
                }
                ;
                _allSrc.Remove(id);
            }
            _destroyedSrcIds.Clear();

            // Remove destroyed destinations
            foreach (int id in _destroyedDstIds)
            {
                if (_allDst.TryGetValue(id, out DstData dstData))
                {
                    if (dstData.dstArray.isCreated)
                        dstData.dstArray.Dispose();
                    _allDst.Remove(id);
                }
            }
            _destroyedDstIds.Clear();

            foreach (KeyValuePair<int, List<Transform>> srcKvp in _allSrc)
            {
                int srcId = srcKvp.Key;
                if (!_allNSrc.TryGetValue(srcId, out NativeList<TransformData> srcNList))
                    continue;
                srcNList.Clear();
                List<Transform> srcTransforms = srcKvp.Value;
                for (int i = 0; i < srcTransforms.Count; ++i)
                {
                    Transform srcTransform = srcTransforms[i];
                    if (srcTransform != null)
                    {
                        srcNList.Add(new TransformData()
                        {
                            isNull = false,
                            position = srcTransform.position,
                            rotation = srcTransform.rotation,
                            localScale = srcTransform.localScale,
                        });
                    }
                    else
                    {
                        srcNList.Add(new TransformData()
                        {
                            isNull = true,
                            position = float3.zero,
                            rotation = quaternion.identity,
                            localScale = float3.zero,
                        });
                    }
                }
            }

            foreach (KeyValuePair<int, DstData> dstKvp in _allDst)
            {
                if (!_allNSrc.TryGetValue(dstKvp.Value.srcId, out NativeList<TransformData> srcNList))
                    continue;
                // Schedule ONE big job
                CopyTransformsJob job = new CopyTransformsJob
                {
                    sourceTransforms = srcNList,
                };
                _jobHandle = job.Schedule(dstKvp.Value.dstArray, _jobHandle);
            }
        }

        [BurstCompile]
        private struct CopyTransformsJob : IJobParallelForTransform
        {
            [ReadOnly]
            public NativeList<TransformData> sourceTransforms;

            public void Execute(int index, TransformAccess destination)
            {
                if (!destination.isValid)
                    return;

                TransformData source = sourceTransforms[index];
                if (source.isNull)
                    return;

                destination.position = source.position;
                destination.rotation = source.rotation;
                destination.localScale = source.localScale;
            }
        }

        [BurstCompile]
        private struct TransformData
        {
            public bool isNull;
            public float3 position;
            public quaternion rotation;
            public float3 localScale;
        }

        [BurstCompile]
        private struct DstData
        {
            public int srcId;
            public TransformAccessArray dstArray;
        }
    }
}