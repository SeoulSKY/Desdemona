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