#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BioLab.Editor
{
    public static class BioLabBuildApp
    {
        [MenuItem("BioLab/Build macOS Standalone App")]
        public static void BuildStandaloneOSX()
        {
            BioLabSetup.BuildMainScene();

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/BioCleanroomMain.unity" };
            buildPlayerOptions.locationPathName = "Build/macOS/BioCleanroomLab.app";
            buildPlayerOptions.target = BuildTarget.StandaloneOSX;
            buildPlayerOptions.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BioLab] Build succeeded: {summary.totalSize} bytes");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("[BioLab] Build failed");
            }
        }
    }
}
#endif
