using System.Collections;
using GameJamG1.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameJamG1.Sequences
{
    // Pantalla de final: funde a negro rapido y muestra, sobre el negro, el texto de lo que paso.
    // Se crea sola en tiempo de ejecucion, por encima de todo el resto de la UI.
    public class EndingScreen : MonoBehaviour
    {
        private const int SortingOrder = 500;
        private const float TextFade = 1.6f;
        private const float MinLineSeconds = 2.5f;
        private const float SecondsPerCharacter = 0.025f;
        private const float FinalHold = 2f;
        private const float LeadInSilence = 1.2f;
        private const float CharInterval = 0.06f;
        private const float CharFade = 0.6f;
        private const float CommaPause = 0.35f;
        private const float StopPause = 0.8f;

        private CanvasGroup background;
        private CanvasGroup textGroup;
        private TMP_Text label;

        public static IEnumerator Show(DialogueSequence text, float fadeDuration)
        {
            var screen = Create();
            yield return screen.Run(text, fadeDuration);
        }

        private static EndingScreen Create()
        {
            var root = new GameObject("EndingScreen", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var screen = root.AddComponent<EndingScreen>();

            var black = new GameObject("Black", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            black.transform.SetParent(root.transform, false);
            Stretch(black.GetComponent<RectTransform>(), 0f);
            black.GetComponent<Image>().color = Color.black;
            screen.background = black.GetComponent<CanvasGroup>();
            screen.background.alpha = 0f;

            var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(CanvasGroup));
            textObject.transform.SetParent(root.transform, false);
            Stretch(textObject.GetComponent<RectTransform>(), 260f);
            var tmp = textObject.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 40f;
            tmp.color = new Color(0.9f, 0.88f, 0.85f);
            tmp.textWrappingMode = TextWrappingModes.Normal;
            if (TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
            }

            screen.label = tmp;
            screen.textGroup = textObject.GetComponent<CanvasGroup>();
            screen.textGroup.alpha = 0f;

            return screen;
        }

        private static void Stretch(RectTransform rect, float margin)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(margin, margin * 0.5f);
            rect.offsetMax = new Vector2(-margin, -margin * 0.5f);
        }

        private IEnumerator Run(DialogueSequence text, float fadeDuration)
        {
            yield return Fade(background, 1f, fadeDuration);
            yield return new WaitForSecondsRealtime(LeadInSilence);

            if (text != null)
            {
                foreach (var line in text.Lines)
                {
                    textGroup.alpha = 1f;
                    yield return Reveal(line.Text);

                    var seconds = line.DurationOverride > 0f
                        ? line.DurationOverride
                        : Mathf.Max(MinLineSeconds, line.Text.Length * SecondsPerCharacter);
                    yield return Hold(seconds);

                    yield return Fade(textGroup, 0f, TextFade);
                }
            }

            yield return new WaitForSecondsRealtime(FinalHold);
        }

        // Cada letra aparece fundiendose, una tras otra, con pausas en la puntuacion.
        private IEnumerator Reveal(string text)
        {
            label.text = text;
            label.ForceMeshUpdate();
            var info = label.textInfo;
            var count = info.characterCount;
            var starts = new float[count];
            var time = 0f;

            for (var i = 0; i < count; i++)
            {
                starts[i] = time;
                var c = info.characterInfo[i].character;
                time += CharInterval;
                if (c == ',' || c == ';' || c == ':')
                {
                    time += CommaPause;
                }
                else if (c == '.' || c == '!' || c == '?' || c == '…')
                {
                    time += StopPause;
                }
            }

            var total = time + CharFade;
            var elapsed = 0f;
            var skipFrom = Time.unscaledTime + 0.4f;
            ApplyAlphas(starts, 0f);

            while (elapsed < total)
            {
                if (Time.unscaledTime > skipFrom && SkipPressed())
                {
                    break;
                }

                elapsed += Time.unscaledDeltaTime;
                ApplyAlphas(starts, elapsed);
                yield return null;
            }

            ApplyAlphas(starts, float.MaxValue);
            yield return null;
        }

        private void ApplyAlphas(float[] starts, float elapsed)
        {
            var info = label.textInfo;

            for (var i = 0; i < starts.Length && i < info.characterCount; i++)
            {
                var ch = info.characterInfo[i];
                if (!ch.isVisible)
                {
                    continue;
                }

                var alpha = (byte)(Mathf.Clamp01((elapsed - starts[i]) / CharFade) * 255f);
                var colors = info.meshInfo[ch.materialReferenceIndex].colors32;
                for (var v = 0; v < 4; v++)
                {
                    colors[ch.vertexIndex + v].a = alpha;
                }
            }

            label.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        // Espera el tiempo de lectura; un clic o una tecla lo adelanta.
        private static IEnumerator Hold(float seconds)
        {
            var end = Time.unscaledTime + seconds;
            var skipFrom = Time.unscaledTime + 0.5f;

            while (Time.unscaledTime < end)
            {
                if (Time.unscaledTime > skipFrom && SkipPressed())
                {
                    yield break;
                }

                yield return null;
            }
        }

        private static bool SkipPressed()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            return (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame))
                || (mouse != null && mouse.leftButton.wasPressedThisFrame);
        }

        private static IEnumerator Fade(CanvasGroup group, float target, float duration)
        {
            var start = group.alpha;
            var elapsed = 0f;

            while (elapsed < 1f)
            {
                elapsed += Time.unscaledDeltaTime / Mathf.Max(duration, 0.01f);
                group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed));
                yield return null;
            }

            group.alpha = target;
        }
    }
}
