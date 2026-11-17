using UnityEngine;

namespace GameJamG1.Interaction
{
    public class InteractableProxy : MonoBehaviour, IInteractable
    {
        [Tooltip("Interactable that receives everything done to this object.")]
        [SerializeField] private InteractableBase target;

        private OutlineHighlight highlight;

        public bool IsLocked => target != null && target.IsLocked;

        private void Awake()
        {
            TryGetComponent(out highlight);
        }

        public string GetPrompt()
        {
            return target != null ? target.GetPrompt() : string.Empty;
        }

        public bool CanInteract()
        {
            return isActiveAndEnabled && target != null && target.CanInteract();
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (target != null)
            {
                target.Interact(interactor);
            }
        }

        public void InteractLocked(PlayerInteractor interactor)
        {
            if (target != null)
            {
                target.InteractLocked(interactor);
            }
        }

        public void SetHighlight(bool highlighted)
        {
            if (highlight != null)
            {
                highlight.SetHighlight(highlighted);
            }

            if (target != null)
            {
                target.SetHighlight(highlighted);
            }
        }

        private void OnValidate()
        {
            if (target == null)
            {
                Debug.LogWarning($"{name}: InteractableProxy has no target assigned.", this);
            }
        }
    }
}
