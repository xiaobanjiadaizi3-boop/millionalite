#if UNITY_EDITOR
using System.IO;
using Millionalite.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Millionalite.EditorTools
{
    /// <summary>
    /// Assets/Scenes/Main.unity を作り直すためのメニュー。
    /// 手で書いたシーンが壊れている／別のシーンから始めたいときの逃げ道。
    /// </summary>
    public static class SceneSetup
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("millionalite/シーンを作り直す")]
        public static void RebuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrap = new GameObject("Bootstrap");
            bootstrap.AddComponent<GameBootstrap>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"{ScenePath} を作り直しました。Play を押せば起動します。");
        }

        [MenuItem("millionalite/このシーンをビルド対象に追加")]
        public static void AddSceneToBuild()
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (list.Exists(s => s.path == ScenePath))
            {
                Debug.Log("すでにビルド対象に入っています。");
                return;
            }

            list.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log($"{ScenePath} をビルド対象に追加しました。");
        }
    }
}
#endif
