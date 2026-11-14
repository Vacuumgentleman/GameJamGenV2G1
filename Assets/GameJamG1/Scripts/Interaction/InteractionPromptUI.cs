using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameJamG1.Interaction
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Optional. Found automatically in the scene when empty.")]
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private TMP_Text promptLabel;
        [SerializeField] private Graphic crosshair;

        [Header("Prompt")]
        [SerializeField] private string interactKeyLabel = "E";
        [Tooltip("{0} is the key label, {1} is the interactable prompt.")]
        [SerializeField] private string promptFormat = "[{0}] {1}";

        [Header("Crosshair")]
        [SerializeField] private Color idleColor = new(1f, 1f, 1f, 0.5f);
        [SerializeField] private Color targetColor = Color.white;
        [SerializeField, Min(0.1f)] private float idleScale = 1f;
        [SerializeField, Min(0.1f)] private float targetScale = 1.8f;

        private string lastPrompt;

        private void Start()
        {
            if (interactor == null)
            {
                interactor = FindFirstObjectByType<PlayerInteractor>();
            }

            if (interactor != null)
            {
                interactor.TargetChanged += HandleTargetChanged;
            }

            Refresh(null);
        }

        private void OnDestroy()
        {
            if (interactor != null)
            {
                interactor.TargetChanged -= HandleTargetChanged;
            }
        }

        private void Update()
        {
            if (interactor != null && interactor.Current != null)
            {
                Refresh(interactor.Current);
            }
        }

        private void HandleTargetChanged(IInteractable target)
        {
            Refresh(target);
        }

        private void Refresh(IInteractable target)
        {
            var prompt = target == null ? string.Empty : string.Format(promptFormat, interactKeyLabel, target.GetPrompt());

            if (prompt != lastPrompt)
            {
                lastPrompt = prompt;
                promptLabel.text = prompt;
            }

            if (crosshair != null)
            {
                var hasTarget = target != null;
                crosshair.color = hasTarget ? targetColor : idleColor;
                crosshair.rectTransform.localScale = Vector3.one * (hasTarget ? targetScale : idleScale);
            }
        }

        private void OnValidate()
        {
            if (promptLabel == null)
            {
                Debug.LogWarning($"{name}: InteractionPromptUI has no prompt label assigned.", this);
            }
        }
    }
}
