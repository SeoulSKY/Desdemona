using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor
{
    public class Builder
    {
        // This method performs the actual build.
        private static void Build(BuildTarget buildTarget, string outputFilePath)
        {
            string[] scenes = GetScenePaths();
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                targetGroup = BuildTargetGroup.WebGL,
                target = buildTarget,
                locationPathName = outputFilePath
            };

            BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (buildReport.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded.");
            }
            else
            {
                Debug.LogError("Build failed: " + buildReport.summary.ToString());
            }
        }

        // Helper method to get scene paths.
        private static string[] GetScenePaths()
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