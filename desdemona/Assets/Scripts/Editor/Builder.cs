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
        public static string[] GetScenePaths()
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            string[] scenePaths = new string[sceneCount];

            for (int i = 0; i < sceneCount; i++)
            {
                scenePaths[i] = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            }

            return scenePaths;
        }
    }
}