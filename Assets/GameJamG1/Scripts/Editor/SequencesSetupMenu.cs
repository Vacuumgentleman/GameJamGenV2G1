using GameJamG1.Enemy;
using GameJamG1.Sequences;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameJamG1.EditorTools
{
    public static class SequencesSetupMenu
    {
        private const string Root = "Assets/GameJamG1";

        internal static ScreenFader EnsureScreenFader()
        {
            var existing = Object.FindFirstObjectByType<ScreenFader>();
            if (existing != null)
            {
                return existing;
            }

            var canvasGo = new GameObject("FaderCanvas", typeof(Canvas), typeof(CanvasGroup));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var fade = new GameObject("Fade", typeof(RectTransform), typeof(Image));
            fade.transform.SetParent(canvasGo.transform, false);
            var rect = (RectTransform)fade.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = fade.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;

            var fader = canvasGo.AddComponent<ScreenFader>();
            EditorAssetUtility.SetObject(fader, "group", canvasGo.GetComponent<CanvasGroup>());
            canvasGo.GetComponent<CanvasGroup>().alpha = 0f;

            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Screen Fader");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return fader;
        }

        internal static NoiseChannel EnsureNoiseChannel()
        {
            EditorAssetUtility.EnsureFolder(Root + "/ScriptableObjects", "Noise");

            var path = Root + "/ScriptableObjects/Noise/Noise_Main.asset";
            var isNew = AssetDatabase.LoadAssetAtPath<NoiseChannel>(path) == null;
            var channel = EditorAssetUtility.LoadOrCreate<NoiseChannel>(path);

            if (isNew)
            {
                EditorAssetUtility.SetBool(channel, "logEmissions", true);
                EditorUtility.SetDirty(channel);
                AssetDatabase.SaveAssets();
            }

            return channel;
        }
    }
}
