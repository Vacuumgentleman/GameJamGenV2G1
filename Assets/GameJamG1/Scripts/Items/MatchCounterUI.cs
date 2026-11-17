using GameJamG1.Interaction;
using TMPro;
using UnityEngine;

namespace GameJamG1.Items
{
    // Contador de cerillas discreto: solo aparece al empezar, cuando cambia el numero,
    // con la vela en la mano o al apuntar a unas cerillas. El resto del tiempo se desvanece.
    public class MatchCounterUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Optional. Found automatically in the scene when empty.")]
        [SerializeField] private MatchBox matchBox;
        [SerializeField] private TMP_Text label;

        [Header("Format")]
        [Tooltip("{0} is the number of matches.")]
        [SerializeField] private string format = "Cerillas: {0}";

        [Header("Visibility")]
        [SerializeField, Min(0f)] private float showAtStartSeconds = 8f;
        [Tooltip("Seconds the counter stays after picking up or spending matches.")]
        [SerializeField, Min(0f)] private float showAfterChangeSeconds = 3f;
        [SerializeField, Min(0.01f)] private float fadeSeconds = 0.5f;

        private ItemHolder holder;
        private PlayerInteractor interactor;
        private float remaining;
        private bool initialized;

        private void Start()
        {
            if (matchBox == null)
            {
                matchBox = FindFirstObjectByType<MatchBox>();
            }

            holder = FindFirstObjectByType<ItemHolder>();
            interactor = FindFirstObjectByType<PlayerInteractor>();
            remaining = showAtStartSeconds;

            if (matchBox == null)
            {
                return;
            }

            matchBox.CountChanged += Refresh;
            Refresh(matchBox.Count);
            initialized = true;
        }

        private void OnDestroy()
        {
            if (matchBox != null)
            {
                matchBox.CountChanged -= Refresh;
            }
        }

        private void Update()
        {
            // Tope por frame: un tiron de carga al empezar no se come el tiempo de aviso.
            remaining -= Mathf.Min(Time.unscaledDeltaTime, 0.1f);

            var visible = remaining > 0f
                || (holder != null && holder.CurrentLightSource is Candle)
                || (interactor != null && interactor.Current is MatchPickup);

            label.alpha = Mathf.MoveTowards(label.alpha, visible ? 1f : 0f, Time.unscaledDeltaTime / fadeSeconds);
        }

        private void Refresh(int count)
        {
            label.text = string.Format(format, count);

            if (initialized)
            {
                remaining = Mathf.Max(remaining, showAfterChangeSeconds);
            }
        }

        private void OnValidate()
        {
            if (label == null)
            {
                Debug.LogWarning($"{name}: MatchCounterUI has no label assigned.", this);
            }
        }
    }
}
