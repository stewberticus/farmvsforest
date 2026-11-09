using System.Linq;
using FarmVsForest.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FarmVsForest.Editor
{
    /// <summary>Builds a bare scene with a MatchRunner and the debug board so you can press Play straight away.</summary>
    public static class PrototypeSceneCreator
    {
        private const string ScenePath = "Assets/_Project/Scenes/Prototype.unity";

        [MenuItem("Farm vs Forest/Create Prototype Scene")]
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

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(s => s.path != ScenePath))
            {
                scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }

            Selection.activeGameObject = go;
            Debug.Log("Farm vs Forest: created " + ScenePath + ". Press Play to try a hot-seat match.");
        }
    }
}
