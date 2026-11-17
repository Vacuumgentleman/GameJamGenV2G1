using System.Collections.Generic;
using GameJamG1.Dialogue;
using GameJamG1.Enemy;
using GameJamG1.Hallucination;
using GameJamG1.Interaction;
using GameJamG1.Story;
using UnityEngine;

namespace GameJamG1.Gameplay
{
    // Si el monstruo atrapa al jugador durante una persecucion, rebobina esa escena:
    // el jugador reaparece en el punto indicado, el monstruo vuelve a dormir y el objeto
    // que la inicio (el frasco) puede usarse otra vez. Se repite hasta escapar.
    public class ChaseRetry : MonoBehaviour
    {
        private static readonly List<ChaseRetry> Registered = new();

        [Header("When")]
        [Tooltip("The retry applies while every one of these flags is active (chase started).")]
        [SerializeField] private List<StoryFlag> activeWhileAll = new();
        [Tooltip("The retry stops applying once any of these flags is active (player escaped).")]
        [SerializeField] private List<StoryFlag> inactiveWhenAny = new();

        [Header("Respawn")]
        [SerializeField] private Transform respawnPoint;

        [Header("Rewind")]
        [SerializeField] private List<StoryFlag> flagsToClear = new();
        [SerializeField] private List<InteractableBase> interactablesToReset = new();
        [SerializeField] private List<StoryReaction> reactionsToRearm = new();
        [Tooltip("Enemies put back to sleep. They wake up again when the scene repeats.")]
        [SerializeField] private List<EnemyController> enemiesToSleep = new();
        [SerializeField] private List<GameObject> deactivate = new();
        [Tooltip("-1 leaves the hallucination level untouched.")]
        [SerializeField] private int hallucinationLevel = -1;
        [SerializeField] private DialogueSequence retryDialogue;

        public Transform RespawnPoint => respawnPoint;

        public IReadOnlyList<EnemyController> EnemiesToSleep => enemiesToSleep;

        public static ChaseRetry FindActive()
        {
            foreach (var retry in Registered)
            {
                if (retry != null && retry.isActiveAndEnabled && retry.Applies())
                {
                    return retry;
                }
            }

            return null;
        }

        private void OnEnable()
        {
            Registered.Add(this);
        }

        private void OnDisable()
        {
            Registered.Remove(this);
        }

        public bool Applies()
        {
            var story = StoryManager.Instance;
            if (story == null)
            {
                return false;
            }

            return story.HasAll(activeWhileAll) && !story.HasAny(inactiveWhenAny);
        }

        // Se llama con la pantalla en negro, antes de devolver el control al jugador.
        public void Rewind()
        {
            foreach (var enemy in enemiesToSleep)
            {
                if (enemy != null)
                {
                    enemy.SetActive(false);
                }
            }

            foreach (var target in deactivate)
            {
                if (target != null)
                {
                    target.SetActive(false);
                }
            }

            foreach (var reaction in reactionsToRearm)
            {
                if (reaction != null)
                {
                    reaction.Rearm();
                }
            }

            var story = StoryManager.Instance;
            if (story != null)
            {
                foreach (var flag in flagsToClear)
                {
                    story.Clear(flag);
                }
            }

            foreach (var interactable in interactablesToReset)
            {
                if (interactable != null)
                {
                    interactable.ResetInteraction();
                }
            }

            if (hallucinationLevel >= 0 && HallucinationManager.Instance != null)
            {
                HallucinationManager.Instance.SetLevel(hallucinationLevel);
            }
        }

        public void PlayRetryDialogue()
        {
            if (retryDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.Play(retryDialogue, false);
            }
        }

        private void OnValidate()
        {
            if (respawnPoint == null)
            {
                Debug.LogWarning($"{name}: ChaseRetry has no respawn point.", this);
            }
        }
    }
}
