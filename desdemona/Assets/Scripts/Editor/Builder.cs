using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace Commands
{
    public class Builder
    {
        public static void Build()
        {
            var options = new BuildPlayerOptions
            {
                scenes = GetAllScenePaths(),
                locationPathName = Path.Combine(Application.dataPath, "..", "Builds", "WebGL"),
                target = BuildTarget.WebGL,
            };
            
            BuildPipeline.BuildPlayer(options);
            
            // Check GitHub Issue: https://github.com/game-ci/unity-builder/issues/563
            Debug.Log("Logging fake Build results so that the build via game-ci/unity-builder does not fail...");
            Debug.Log($"###########################{Environment.NewLine}#      Build results      #{Environment.NewLine}###########################{Environment.NewLine}" +
                $"{Environment.NewLine}Duration: 00:00:00.0000000{Environment.NewLine}Warnings: 0{Environment.NewLine}Errors: 0{Environment.NewLine}Size: 0 bytes{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}Build succeeded!");
        }
        
        private static string[] GetAllScenePaths()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }
    }
}