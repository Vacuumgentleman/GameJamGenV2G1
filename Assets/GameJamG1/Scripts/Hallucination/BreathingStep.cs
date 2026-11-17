using System.Collections;
using System.Collections.Generic;
using GameJamG1.Story;
using UnityEngine;
using UnityEngine.Events;

namespace GameJamG1.Hallucination
{
    public class BreathingStep : MonoBehaviour
    {
        [Tooltip("Optional. Found automatically in the scene when empty.")]
        [SerializeField] private BreathingMinigame minigame;
        [Tooltip("Flags activated when the player completes the breathing.")]
        [SerializeField] private List<StoryFlag> flagsOnSuccess = new();
        [Tooltip("Start the minigame again after a failure until the player succeeds.")]
        [SerializeField] private bool retryOnFail = true;
        [SerializeField, Min(0f)] private float retryDelay = 1.5f;
        [SerializeField] private UnityEvent onSuccess = new();

        private bool running;
        private bool completed;
        private bool hasResult;
        private bool lastResult;

        public bool Completed => completed;

        public void Begin()
        {
            if (completed || running)
            {
                return;
            }

            if (minigame == null)
            {
                minigame = FindFirstObjectByType<BreathingMinigame>();
            }

            if (minigame == null || !isActiveAndEnabled)
            {
                Complete();
                return;
            }

            StartCoroutine(Run());
        }

        private void OnDisable()
        {
            if (running && minigame != null)
            {
                minigame.Finished -= HandleFinished;
            }

            running = false;
        }

        private IEnumerator Run()
        {
            running = true;

            while (!completed)
            {
                yield return new WaitUntil(() => minigame.CanBegin);

                hasResult = false;
                minigame.Finished += HandleFinished;
                minigame.Begin();

                yield return new WaitUntil(() => hasResult);
                minigame.Finished -= HandleFinished;

                if (lastResult)
                {
                    Complete();
                }
                else if (!retryOnFail)
                {
                    break;
                }
                else
                {
                    yield return new WaitForSeconds(retryDelay);
                }
            }

            running = false;
        }

        private void HandleFinished(bool success)
        {
            lastResult = success;
            hasResult = true;
        }

        private void Complete()
        {
            completed = true;

            if (StoryManager.Instance != null)
            {
                StoryManager.Instance.SetMany(flagsOnSuccess);
            }

            onSuccess.Invoke();
        }
    }
}
