using System.Collections;
using GameJamG1.Dialogue;
using GameJamG1.Interaction;
using GameJamG1.Player;
using UnityEngine;
using UnityEngine.Events;

namespace GameJamG1.Sequences
{
    public class PeepholeSequence : InteractableBase
    {
        [Header("Peephole")]
        [Tooltip("Camera placed on the other side of the door. Disabled until used.")]
        [SerializeField] private Camera peepholeCamera;
        [Tooltip("Steps that run while looking through the peephole.")]
        [SerializeField] private TimedEventSequence sequence;
        [SerializeField, Min(0f)] private float fadeDuration = 0.4f;
        [SerializeField, Min(0f)] private float holdAfterSequence = 0.3f;

        [Header("End")]
        [SerializeField] private DialogueSequence endDialogue;
        [SerializeField] private UnityEvent onStarted = new();
        [SerializeField] private UnityEvent onFinished = new();

        private Camera playerCamera;
        private PlayerControlLock playerLock;
        private bool running;

        public bool IsRunning => running;

        public override bool CanInteract()
        {
            return base.CanInteract() && !running;
        }

        protected override void Awake()
        {
            base.Awake();

            if (peepholeCamera != null)
            {
                peepholeCamera.enabled = false;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (running)
            {
                Restore();
            }
        }

        protected override bool OnInteract(PlayerInteractor interactor)
        {
            playerCamera = interactor.GetComponentInChildren<Camera>();
            playerLock = interactor.GetComponentInParent<PlayerControlLock>();

            if (peepholeCamera == null || playerCamera == null)
            {
                return false;
            }

            StartCoroutine(Run());
            return true;
        }

        private IEnumerator Run()
        {
            running = true;

            if (playerLock != null)
            {
                playerLock.Lock(this);
            }

            onStarted.Invoke();

            yield return Fade(1f);
            playerCamera.enabled = false;
            peepholeCamera.enabled = true;
            yield return Fade(0f);

            if (sequence != null)
            {
                sequence.Play();
                yield return new WaitWhile(() => sequence.IsPlaying);
            }

            yield return new WaitForSeconds(holdAfterSequence);

            yield return Fade(1f);
            Restore();
            yield return Fade(0f);

            onFinished.Invoke();

            if (endDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.Play(endDialogue, false);
            }
        }

        private IEnumerator Fade(float target)
        {
            var fader = ScreenFader.Instance;
            return fader != null ? fader.FadeTo(target, fadeDuration) : Wait();
        }

        private static IEnumerator Wait()
        {
            yield break;
        }

        private void Restore()
        {
            running = false;
            peepholeCamera.enabled = false;
            playerCamera.enabled = true;

            if (playerLock != null)
            {
                playerLock.Unlock(this);
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (peepholeCamera == null)
            {
                Debug.LogWarning($"{name}: PeepholeSequence has no peephole camera assigned.", this);
            }
        }
    }
}
