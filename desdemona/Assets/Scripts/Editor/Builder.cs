using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor
{
    public class Builder
    {
        public static void Build()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                targetGroup = BuildTargetGroup.WebGL,
                target = BuildTarget.WebGL,
                locationPathName = Path.Combine(Application.dataPath, "Builds"),
            };

            var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (buildReport.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded.");
            }
            else
            {
                Debug.LogError("Build failed: " + buildReport.summary);
            }
        }
        private static string[] GetScenePaths()
        {
            var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            var scenePaths = new string[sceneCount];

            for (var i = 0; i < sceneCount; i++)
            {
                scenePaths[i] = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            }

            return scenePaths;
        }
    }
}