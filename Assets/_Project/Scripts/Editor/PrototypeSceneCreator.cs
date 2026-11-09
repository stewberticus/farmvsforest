using System.Linq;
using FarmVsForest.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FarmVsForest.Editor
{
    /// <summary>Builds bare scenes with a MatchRunner so you can press Play straight away.</summary>
    public static class PrototypeSceneCreator
    {
        private const string ScenePath = "Assets/_Project/Scenes/Prototype.unity";
        private const string DemoScenePath = "Assets/_Project/Scenes/Demo.unity";

        /// <summary>The v1 no-art demo: team select, board, hand, goals and a winner screen.</summary>
        [MenuItem("Farm vs Forest/Create Demo Scene", priority = 0)]
        public static void CreateDemo()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var go = new GameObject("Farm vs Forest Demo");
            go.AddComponent<MatchRunner>().startOnPlay = false;
            go.AddComponent<DemoGame>();
            Save(scene, DemoScenePath);
            Selection.activeGameObject = go;
            Debug.Log("Farm vs Forest: created " + DemoScenePath + ". Press Play, pick teams and start.");
        }

        /// <summary>The bare debug board (DebugMatchGUI), handy for testing rules.</summary>
        [MenuItem("Farm vs Forest/Create Prototype Scene (debug board)", priority = 1)]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.1f, 0.12f, 0.1f);
            }

            var go = new GameObject("Match");
            go.AddComponent<MatchRunner>();
            go.AddComponent<DebugMatchGUI>();

            Save(scene, ScenePath);
            Selection.activeGameObject = go;
            Debug.Log("Farm vs Forest: created " + ScenePath + ". Press Play to try a hot-seat match.");
        }

        private static void Save(UnityEngine.SceneManagement.Scene scene, string path)
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            EditorSceneManager.SaveScene(scene, path);

            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(s => s.path != path))
            {
                scenes.Insert(0, new EditorBuildSettingsScene(path, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
