using System;
using GameJamG1.Player;
using UnityEngine;

namespace GameJamG1.Interaction
{
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Transform the ray starts from, normally the player camera.")]
        [SerializeField] private Transform rayOrigin;

        [Header("Raycast")]
        [SerializeField, Min(0.1f)] private float maxDistance = 3f;
        [SerializeField] private LayerMask layerMask = ~(1 << 2);

        private PlayerInputReader input;
        private IInteractable current;

        public IInteractable Current => current;

        public event Action<IInteractable> TargetChanged;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            var candidate = FindTarget();

            if (!ReferenceEquals(candidate, current))
            {
                SetTarget(candidate);
            }

            if (current != null && input.InteractPressed)
            {
                Activate();
            }
        }

        private void OnDisable()
        {
            SetTarget(null);
        }

        private IInteractable FindTarget()
        {
            if (rayOrigin == null)
            {
                return null;
            }

            var hitSomething = Physics.Raycast(
                rayOrigin.position,
                rayOrigin.forward,
                out var hit,
                maxDistance,
                layerMask,
                QueryTriggerInteraction.Ignore);

            if (!hitSomething)
            {
                return null;
            }

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable == null)
            {
                return null;
            }

            return interactable.IsLocked || interactable.CanInteract() ? interactable : null;
        }

        private void SetTarget(IInteractable target)
        {
            current?.SetHighlight(false);
            current = target;
            current?.SetHighlight(true);
            TargetChanged?.Invoke(current);
        }

        private void Activate()
        {
            if (current.IsLocked)
            {
                current.InteractLocked(this);
            }
            else if (current.CanInteract())
            {
                current.Interact(this);
            }
        }

        private void OnValidate()
        {
            if (rayOrigin == null)
            {
                Debug.LogWarning($"{name}: PlayerInteractor has no ray origin assigned.", this);
            }
        }
    }
}
