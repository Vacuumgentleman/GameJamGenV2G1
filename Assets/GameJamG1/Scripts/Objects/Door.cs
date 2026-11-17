using GameJamG1.Dialogue;
using GameJamG1.Interaction;
using UnityEngine;

namespace GameJamG1.Objects
{
    public enum DoorState
    {
        Closed,
        Open,
        Locked,
        Barricaded,
        Broken
    }

    public class Door : AnimatedInteractable
    {
        [Header("Door")]
        [Tooltip("Hinge transform that rotates. Empty uses this object.")]
        [SerializeField] private Transform pivot;
        [SerializeField] private float openAngle = 90f;

        [Header("Blocking")]
        [Tooltip("Monologue played when the door is barricaded.")]
        [SerializeField] private DialogueSequence barricadedDialogue;
        [SerializeField] private AudioClip lockedSound;

        [Header("Break")]
        [Tooltip("Seconds the door takes to fly open when the monster breaks it.")]
        [SerializeField, Min(0.05f)] private float breakDuration = 0.15f;
        [Tooltip("Angle of a broken door. Wider than a normal open door.")]
        [SerializeField] private float breakAngle = 110f;
        [SerializeField] private AudioClip breakSound;
        [SerializeField, Range(0f, 1f)] private float breakVolume = 1f;

        private Quaternion closedRotation;
        private bool barricaded;
        private float currentAngle;

        public bool IsBroken { get; private set; }

        public override bool IsLocked => !IsBroken && (barricaded || base.IsLocked);

        public DoorState State
        {
            get
            {
                if (IsBroken)
                {
                    return DoorState.Broken;
                }

                if (barricaded)
                {
                    return DoorState.Barricaded;
                }

                if (base.IsLocked)
                {
                    return DoorState.Locked;
                }

                return IsOpen ? DoorState.Open : DoorState.Closed;
            }
        }

        public override bool CanInteract()
        {
            return !IsBroken && base.CanInteract();
        }

        public void SetBarricaded(bool value)
        {
            barricaded = value;
        }

        // El monstruo la revienta: se abre de golpe hacia el lado contrario a el y queda inservible.
        public void Break(Vector3 breakerPosition)
        {
            if (IsBroken || barricaded || !isActiveAndEnabled)
            {
                return;
            }

            IsBroken = true;
            currentAngle = AwayAngle(breakerPosition);
            SetHighlight(false);

            if (breakSound != null)
            {
                AudioSource.PlayClipAtPoint(breakSound, transform.position, breakVolume);
            }

            if (IsOpen)
            {
                // Ya estaba abierta: solo la empuja hasta el angulo de rotura.
                Evaluate(1f);
                return;
            }

            SetOpen(true, breakDuration, breakSound == null);
        }

        // Deshace la rotura: la puerta vuelve entera y cerrada (al reaparecer tras morir).
        public void Repair()
        {
            if (!IsBroken)
            {
                return;
            }

            IsBroken = false;
            currentAngle = openAngle;
            SnapClosed();
        }

        public override void InteractLocked(PlayerInteractor interactor)
        {
            if (lockedSound != null)
            {
                AudioSource.PlayClipAtPoint(lockedSound, transform.position);
            }

            base.InteractLocked(interactor);
        }

        protected override void Awake()
        {
            base.Awake();

            if (pivot == null)
            {
                pivot = transform;
            }

            closedRotation = pivot.localRotation;
            currentAngle = openAngle;
        }

        protected override void Evaluate(float value)
        {
            pivot.localRotation = closedRotation * Quaternion.Euler(0f, currentAngle * value, 0f);
        }

        protected override DialogueSequence ResolveLockedDialogue()
        {
            return barricaded && barricadedDialogue != null ? barricadedDialogue : base.ResolveLockedDialogue();
        }

        // Elige el sentido de giro que aleja la hoja de la puerta del monstruo.
        private float AwayAngle(Vector3 breakerPosition)
        {
            var leaf = GetComponentInChildren<Renderer>();
            var center = leaf != null ? leaf.bounds.center : pivot.position;
            var offset = center - pivot.position;
            var parentRotation = pivot.parent != null ? pivot.parent.rotation : Quaternion.identity;
            var axis = parentRotation * closedRotation * Vector3.up;

            var positive = pivot.position + Quaternion.AngleAxis(breakAngle, axis) * offset;
            var negative = pivot.position + Quaternion.AngleAxis(-breakAngle, axis) * offset;

            return (positive - breakerPosition).sqrMagnitude >= (negative - breakerPosition).sqrMagnitude ? breakAngle : -breakAngle;
        }
    }
}
