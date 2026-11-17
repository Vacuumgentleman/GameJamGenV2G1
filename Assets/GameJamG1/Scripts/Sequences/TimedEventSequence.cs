using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameJamG1.Sequences
{
    public class TimedEventSequence : MonoBehaviour
    {
        [Serializable]
        public class Step
        {
            [SerializeField] private string label;
            [Tooltip("Seconds to wait after the previous step.")]
            [SerializeField, Min(0f)] private float delay = 1f;
            [SerializeField] private UnityEvent onReached = new();

            public string Label => label;
            public float Delay => delay;
            public UnityEvent OnReached => onReached;
        }

        [SerializeField] private List<Step> steps = new();
        [SerializeField] private bool playOnEnable;
        [Tooltip("When disabled, the sequence can only run once.")]
        [SerializeField] private bool canReplay = true;
        [SerializeField] private UnityEvent onFinished = new();

        private Coroutine routine;
        private bool hasPlayed;

        public IReadOnlyList<Step> Steps => steps;

        private bool playing;

        public bool IsPlaying => playing;

        public event Action Finished;

        private void OnEnable()
        {
            if (playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            routine = null;
            playing = false;
        }

        public void Play()
        {
            if (IsPlaying || (hasPlayed && !canReplay))
            {
                return;
            }

            hasPlayed = true;
            playing = true;
            routine = StartCoroutine(Run());
        }

        public void Stop()
        {
            if (routine == null)
            {
                return;
            }

            StopCoroutine(routine);
            routine = null;
            playing = false;
        }

        private IEnumerator Run()
        {
            foreach (var step in steps)
            {
                yield return new WaitForSeconds(step.Delay);
                step.OnReached.Invoke();
            }

            routine = null;
            playing = false;
            onFinished.Invoke();
            Finished?.Invoke();
        }
    }
}
