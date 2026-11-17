using System.Collections;
using GameJamG1.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace GameJamG1.Objects
{
    public abstract class AnimatedInteractable : InteractableBase
    {
        [Header("Animation")]
        [SerializeField, Min(0.05f)] private float openDuration = 0.8f;
        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;

        [Header("Prompts")]
        [SerializeField] private string openPrompt = "Abrir";
        [SerializeField] private string closePrompt = "Cerrar";

        [Header("Events")]
        [SerializeField] private UnityEvent onOpened;
        [SerializeField] private UnityEvent onClosed;

        private Coroutine routine;
        private float progress;
        private float activeDuration = 0.8f;

        public bool IsOpen { get; private set; }
        public bool IsMoving => routine != null;

        public override string GetPrompt()
        {
            return IsOpen ? closePrompt : openPrompt;
        }

        public void SetOpen(bool open)
        {
            SetOpen(open, openDuration, true);
        }

        protected void SetOpen(bool open, float duration, bool playSound)
        {
            if (open == IsOpen || !isActiveAndEnabled)
            {
                return;
            }

            activeDuration = Mathf.Max(0.05f, duration);

            IsOpen = open;

            var clip = open ? openSound : closeSound;
            if (playSound && clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }

            if (open)
            {
                OnOpenStarted();
            }

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(Animate(open ? 1f : 0f));
        }

        protected abstract void Evaluate(float value);

        protected virtual void OnOpenStarted()
        {
        }

        protected override bool OnInteract(PlayerInteractor interactor)
        {
            if (IsMoving)
            {
                return false;
            }

            SetOpen(!IsOpen);
            return true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            routine = null;
        }

        private IEnumerator Animate(float target)
        {
            var start = progress;
            var elapsed = 0f;

            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime / activeDuration;
                progress = Mathf.Lerp(start, target, curve.Evaluate(Mathf.Clamp01(elapsed)));
                Evaluate(progress);
                yield return null;
            }

            progress = target;
            Evaluate(progress);
            routine = null;

            if (target > 0.5f)
            {
                onOpened.Invoke();
            }
            else
            {
                onClosed.Invoke();
            }
        }
    }
}
