using System.Collections;
using System.Collections.Generic;
using GameJamG1.Dialogue;
using GameJamG1.Enemy;
using GameJamG1.Gameplay;
using GameJamG1.Hallucination;
using UnityEngine;
using UnityEngine.Events;

namespace GameJamG1.Story
{
    public class StoryReaction : MonoBehaviour
    {
        public enum Activation
        {
            FlagsSet,
            DialogueStarted,
            DialogueFinished,
            ZoneStay,
            OnStart
        }

        [Header("Activation")]
        [Tooltip("FlagsSet: fires when every required flag is active. Dialogue*: fires when the dialogue below starts or ends. ZoneStay: fires while the player is inside this object's trigger collider. OnStart: fires when the scene starts.")]
        [SerializeField] private Activation activation = Activation.FlagsSet;
        [Tooltip("Dialogue watched by DialogueStarted and DialogueFinished.")]
        [SerializeField] private DialogueSequence dialogue;
        [Tooltip("Every flag must be active for the reaction to fire.")]
        [SerializeField] private List<StoryFlag> requiredFlags = new();
        [SerializeField] private string playerTag = "Player";
        [SerializeField, Min(0f)] private float delay;
        [SerializeField] private bool fireOnce = true;

        [Header("Result")]
        [SerializeField] private List<StoryFlag> flagsToSet = new();
        [Tooltip("-1 leaves the hallucination level untouched.")]
        [SerializeField] private int hallucinationLevel = -1;
        [SerializeField] private DialogueSequence dialogueToPlay;
        [SerializeField] private List<GameObject> activate = new();
        [SerializeField] private List<GameObject> deactivate = new();
        [SerializeField] private List<AudioSource> audioToPlay = new();
        [SerializeField] private List<AudioSource> audioToStop = new();
        [SerializeField] private List<EnemyController> enemiesToActivate = new();
        [SerializeField] private List<EnemyController> enemiesToDeactivate = new();
        [SerializeField] private BreathingStep breathingStep;
        [SerializeField] private bool saveCheckpoint;
        [Tooltip("Stops enemies and disables death. Use when the final choice is made.")]
        [SerializeField] private bool beginEnding;
        [Tooltip("Fades out and returns to the menu.")]
        [SerializeField] private bool endGame;
        [Tooltip("Ending: fast fade to black, this text is shown on the black screen, then the menu.")]
        [SerializeField] private DialogueSequence endingText;

        [Header("Events")]
        [SerializeField] private UnityEvent onFired = new();

        private StoryManager story;
        private DialogueManager dialogues;
        private bool fired;

        private void Start()
        {
            story = StoryManager.Instance;
            dialogues = DialogueManager.Instance;

            if (activation == Activation.OnStart)
            {
                TryFire();
            }

            if (activation == Activation.FlagsSet && story != null)
            {
                story.OnFlagChanged += HandleFlagChanged;

                if (requiredFlags.Count > 0)
                {
                    TryFire();
                }
            }

            if (dialogues != null)
            {
                if (activation == Activation.DialogueStarted)
                {
                    dialogues.OnStarted += HandleDialogue;
                }
                else if (activation == Activation.DialogueFinished)
                {
                    dialogues.OnFinished += HandleDialogue;
                }
            }
        }

        private void OnDestroy()
        {
            if (story != null)
            {
                story.OnFlagChanged -= HandleFlagChanged;
            }

            if (dialogues != null)
            {
                dialogues.OnStarted -= HandleDialogue;
                dialogues.OnFinished -= HandleDialogue;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (activation == Activation.ZoneStay && other.CompareTag(playerTag))
            {
                TryFire();
            }
        }

        // Permite que una reaccion de un solo uso vuelva a dispararse (reintento tras morir).
        public void Rearm()
        {
            StopAllCoroutines();
            fired = false;
        }

        public void Fire()
        {
            if (fireOnce && fired)
            {
                return;
            }

            fired = true;

            if (delay > 0f && isActiveAndEnabled)
            {
                StartCoroutine(FireDelayed());
            }
            else
            {
                Apply();
            }
        }

        private void HandleFlagChanged(StoryFlag changed)
        {
            if (requiredFlags.Contains(changed))
            {
                TryFire();
            }
        }

        private void HandleDialogue(DialogueSequence sequence)
        {
            if (sequence == dialogue)
            {
                TryFire();
            }
        }

        private void TryFire()
        {
            if ((fireOnce && fired) || !ConditionsMet())
            {
                return;
            }

            Fire();
        }

        private bool ConditionsMet()
        {
            if (requiredFlags.Count == 0)
            {
                return true;
            }

            var manager = StoryManager.Instance;
            return manager != null && manager.HasAll(requiredFlags);
        }

        private IEnumerator FireDelayed()
        {
            yield return new WaitForSeconds(delay);
            Apply();
        }

        private void Apply()
        {
            if (hallucinationLevel >= 0 && HallucinationManager.Instance != null)
            {
                HallucinationManager.Instance.SetLevel(hallucinationLevel);
            }

            SetObjects(activate, true);
            SetObjects(deactivate, false);

            foreach (var source in audioToStop)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }

            foreach (var source in audioToPlay)
            {
                if (source != null)
                {
                    source.Play();
                }
            }

            foreach (var enemy in enemiesToDeactivate)
            {
                if (enemy != null)
                {
                    enemy.SetActive(false);
                }
            }

            foreach (var enemy in enemiesToActivate)
            {
                if (enemy != null)
                {
                    enemy.SetActive(true);
                }
            }

            var flow = GameFlowController.Instance;
            if (flow != null)
            {
                if (saveCheckpoint)
                {
                    flow.SaveCheckpoint();
                }

                if (beginEnding)
                {
                    flow.BeginEnding();
                }

                if (endingText != null)
                {
                    flow.PlayEnding(endingText);
                }
                else if (endGame)
                {
                    flow.EndGame();
                }
            }

            if (dialogueToPlay != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.Play(dialogueToPlay, false);
            }

            if (breathingStep != null)
            {
                breathingStep.Begin();
            }

            if (StoryManager.Instance != null)
            {
                StoryManager.Instance.SetMany(flagsToSet);
            }

            onFired.Invoke();
        }

        private static void SetObjects(List<GameObject> objects, bool active)
        {
            foreach (var target in objects)
            {
                if (target != null)
                {
                    target.SetActive(active);
                }
            }
        }
    }
}
