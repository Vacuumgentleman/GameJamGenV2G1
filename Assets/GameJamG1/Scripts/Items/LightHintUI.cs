using TMPro;
using UnityEngine;

namespace GameJamG1.Items
{
    // Recordatorio en pantalla de la luz que se lleva en la mano y de que hace la tecla.
    public class LightHintUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Optional. Found automatically in the scene when empty.")]
        [SerializeField] private ItemHolder holder;
        [Tooltip("Optional. When empty, a copy of the match counter label is created below it.")]
        [SerializeField] private TMP_Text label;

        [Header("Text")]
        [SerializeField] private string keyLabel = "F";
        [SerializeField] private string flashlightOff = "[{0}] Encender linterna";
        [SerializeField] private string flashlightOn = "[{0}] Apagar linterna";
        [SerializeField] private string candleOff = "[{0}] Encender vela (gasta 1 cerilla)";
        [SerializeField] private string candleOn = "[{0}] Apagar vela  ·  correr la apaga";

        private string last;

        private void Start()
        {
            if (holder == null)
            {
                holder = FindFirstObjectByType<ItemHolder>();
            }

            if (label == null)
            {
                label = CreateLabel();
            }

            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (label == null)
            {
                return;
            }

            var light = holder != null ? holder.CurrentLightSource : null;
            var text = string.Empty;

            if (light is Flashlight)
            {
                text = string.Format(light.IsLit ? flashlightOn : flashlightOff, keyLabel);
            }
            else if (light is Candle)
            {
                text = string.Format(light.IsLit ? candleOn : candleOff, keyLabel);
            }

            if (text != last)
            {
                last = text;
                label.text = text;
                label.enabled = text.Length > 0;
            }
        }

        private static TMP_Text CreateLabel()
        {
            var counter = FindFirstObjectByType<MatchCounterUI>();
            var source = counter != null ? counter.GetComponentInChildren<TMP_Text>(true) : null;
            if (source == null)
            {
                return null;
            }

            var copy = Instantiate(source.gameObject, source.transform.parent);
            copy.name = "LightHint";

            foreach (var behaviour in copy.GetComponents<MonoBehaviour>())
            {
                if (behaviour is not TMP_Text)
                {
                    Destroy(behaviour);
                }
            }

            var rect = (RectTransform)copy.transform;
            var sourceRect = (RectTransform)source.transform;
            rect.anchoredPosition = sourceRect.anchoredPosition - new Vector2(0f, Mathf.Max(sourceRect.rect.height, 40f) * 1.1f);

            var text = copy.GetComponent<TMP_Text>();
            text.fontSize = source.fontSize * 0.8f;
            return text;
        }
    }
}
