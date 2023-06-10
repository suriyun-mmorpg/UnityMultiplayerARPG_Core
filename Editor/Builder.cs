using System.Collections.Generic;
using UnityEditor;

namespace MultiplayerARPG
{
    public static class Builder
    {
        public static void BuildWindows64Server()
        {
            string outputPath = string.Empty;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].Equals("-outputPath") && i + 1 < args.Length)
                    outputPath = args[i + 1];
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                UnityEngine.Debug.LogError("No output path");
                return;
            }

            List<string> scenes = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                {
                    scenes.Add(EditorBuildSettings.scenes[i].path);
                    UnityEngine.Debug.Log($"Add {EditorBuildSettings.scenes[i].path} to scenes in build list.");
                }
            }
#if UNITY_2021_1_OR_NEWER
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                locationPathName = outputPath,
                scenes = scenes.ToArray(),
            };
#else
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
                locationPathName = outputPath,
                scenes = scenes.ToArray(),
            };
#endif
            BuildPipeline.BuildPlayer(options);
        }

        public static void BuildLinux64Server()
        {
            string outputPath = string.Empty;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].Equals("-outputPath") && i + 1 < args.Length)
                    outputPath = args[i + 1];
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                UnityEngine.Debug.LogError("No output path");
                return;
            }

            List<string> scenes = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                {
                    scenes.Add(EditorBuildSettings.scenes[i].path);
                    UnityEngine.Debug.Log($"Add {EditorBuildSettings.scenes[i].path} to scenes in build list.");
                }
            }
#if UNITY_2021_1_OR_NEWER
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                locationPathName = outputPath,
                scenes = scenes.ToArray(),
            };
#else
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None,
                locationPathName = outputPath,
                scenes = scenes.ToArray(),
            };
#endif
            BuildPipeline.BuildPlayer(options);
        }
    }
}
