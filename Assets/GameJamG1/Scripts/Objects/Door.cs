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
        Barricaded
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

        private Quaternion closedRotation;
        private bool barricaded;

        public override bool IsLocked => barricaded || base.IsLocked;

        public DoorState State
        {
            get
            {
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

        public void SetBarricaded(bool value)
        {
            barricaded = value;
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
        }

        protected override void Evaluate(float value)
        {
            pivot.localRotation = closedRotation * Quaternion.Euler(0f, openAngle * value, 0f);
        }

        protected override DialogueSequence ResolveLockedDialogue()
        {
            return barricaded && barricadedDialogue != null ? barricadedDialogue : base.ResolveLockedDialogue();
        }
    }
}
