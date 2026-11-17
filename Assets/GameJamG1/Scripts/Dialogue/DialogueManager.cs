using System;
using System.Collections;
using System.Collections.Generic;
using GameJamG1.Player;
using UnityEngine;

namespace GameJamG1.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueUI ui;
        [Tooltip("Optional. Found automatically in the scene when empty.")]
        [SerializeField] private PlayerControlLock playerLock;

        [Header("Defaults")]
        [Tooltip("Monologue used by locked interactables that have no dialogue of their own.")]
        [SerializeField] private DialogueSequence defaultLockedDialogue;

        private readonly Queue<DialogueSequence> queue = new();
        private DialogueSequence current;
        private Coroutine routine;

        public static DialogueManager Instance { get; private set; }

        public DialogueSequence DefaultLockedDialogue => defaultLockedDialogue;
        public bool IsPlaying => current != null;

        public event Action<DialogueSequence> OnStarted;
        public event Action<DialogueLine> OnLineShown;
        public event Action<DialogueSequence> OnFinished;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            StopCurrent();
            queue.Clear();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Play(DialogueSequence sequence)
        {
            Play(sequence, true);
        }

        public void Stop()
        {
            queue.Clear();
            StopCurrent();
        }

        public void Enqueue(DialogueSequence sequence)
        {
            Play(sequence, false);
        }

        public void Play(DialogueSequence sequence, bool interrupt)
        {
            if (!CanPlay(sequence))
            {
                return;
            }

            if (interrupt)
            {
                StopCurrent();
                queue.Clear();
            }

            queue.Enqueue(sequence);

            if (current == null)
            {
                StartNext();
            }
        }

        private bool CanPlay(DialogueSequence sequence)
        {
            return sequence != null
                && sequence.Lines.Count > 0
                && ui != null
                && sequence != current
                && !queue.Contains(sequence);
        }

        private void StartNext()
        {
            if (queue.Count == 0)
            {
                return;
            }

            current = queue.Dequeue();
            routine = StartCoroutine(RunSequence(current));
        }

        private IEnumerator RunSequence(DialogueSequence sequence)
        {
            SetPlayerLock(sequence, true);
            OnStarted?.Invoke(sequence);

            yield return ui.ShowPanel();

            for (var i = 0; i < sequence.Lines.Count; i++)
            {
                OnLineShown?.Invoke(sequence.Lines[i]);
                yield return ui.PlayLine(sequence.Lines[i]);
            }

            yield return ui.HidePanel();

            Finish(sequence);
        }

        private void Finish(DialogueSequence sequence)
        {
            SetPlayerLock(sequence, false);
            current = null;
            routine = null;
            OnFinished?.Invoke(sequence);
            StartNext();
        }

        private void StopCurrent()
        {
            if (current == null)
            {
                return;
            }

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            var stopped = current;
            SetPlayerLock(stopped, false);
            current = null;
            routine = null;

            if (ui != null)
            {
                ui.HideImmediate();
            }

            OnFinished?.Invoke(stopped);
        }

        private void SetPlayerLock(DialogueSequence sequence, bool locked)
        {
            if (!sequence.LockPlayer)
            {
                return;
            }

            if (playerLock == null)
            {
                playerLock = FindFirstObjectByType<PlayerControlLock>();
            }

            if (playerLock == null)
            {
                return;
            }

            if (locked)
            {
                playerLock.Lock(sequence);
            }
            else
            {
                playerLock.Unlock(sequence);
            }
        }

        private void OnValidate()
        {
            if (ui == null)
            {
                Debug.LogWarning($"{name}: DialogueManager has no DialogueUI assigned.", this);
            }

            if (defaultLockedDialogue == null)
            {
                Debug.LogWarning($"{name}: no default locked dialogue assigned.", this);
            }
        }
    }
}
