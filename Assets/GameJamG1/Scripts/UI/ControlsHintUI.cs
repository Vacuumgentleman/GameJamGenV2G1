using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameJamG1.UI
{
    // Lista de controles: se muestra unos segundos al empezar y siempre que el juego esta en pausa.
    // Va abajo a la izquierda, fuera de los botones centrados del menu de pausa, y no recibe clics.
    public class ControlsHintUI : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float showAtStartSeconds = 10f;
        [SerializeField, Min(0.01f)] private float fadeSeconds = 1f;
        [SerializeField, TextArea(6, 12)] private string controls =
            "<b>CONTROLES</b>\n" +
            "<b>W A S D</b>   Moverse\n" +
            "<b>Ratón</b>   Mirar\n" +
            "<b>Shift</b>   Correr\n" +
            "<b>C</b>   Agacharse\n" +
            "<b>E</b>   Interactuar / recoger\n" +
            "<b>F</b>   Encender / apagar luz\n" +
            "<b>Espacio</b>   Respirar (cuando toque)\n" +
            "<b>P</b>   Pausa";

        private CanvasGroup group;
        private PauseMenu pauseMenu;
        private float remaining;
        private GameObject canvasRoot;

        private void OnDestroy()
        {
            if (canvasRoot != null)
            {
                Destroy(canvasRoot);
            }
        }

        private void Start()
        {
            pauseMenu = FindFirstObjectByType<PauseMenu>();
            Build();
            remaining = showAtStartSeconds;
            group.alpha = 1f;
        }

        private void Update()
        {
            // Descuenta tiempo de juego real, con tope por frame: un tiron de carga al empezar no se come el aviso.
            var paused = pauseMenu != null && pauseMenu.isPaused;
            if (!paused)
            {
                remaining -= Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            }

            var visible = remaining > 0f || paused;
            group.alpha = Mathf.MoveTowards(group.alpha, visible ? 1f : 0f, Time.unscaledDeltaTime / fadeSeconds);
        }

        private void Build()
        {
            var canvasObject = new GameObject("ControlsHintCanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasRoot = canvasObject;
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            panel.transform.SetParent(canvasObject.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(40f, 40f);
            rect.sizeDelta = new Vector2(540f, 380f);
            var image = panel.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.55f);
            image.raycastTarget = false;
            group = panel.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(panel.transform, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 18f);
            textRect.offsetMax = new Vector2(-24f, -18f);
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = controls;
            text.fontSize = 26f;
            text.lineSpacing = 8f;
            text.color = new Color(0.9f, 0.88f, 0.85f);
            text.raycastTarget = false;
            if (TMP_Settings.defaultFontAsset != null)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }
        }
    }
}
