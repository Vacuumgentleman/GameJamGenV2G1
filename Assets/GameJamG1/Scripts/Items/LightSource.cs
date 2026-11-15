using System;
using GameJamG1.Dialogue;
using UnityEngine;

namespace GameJamG1.Items
{
    public abstract class LightSource : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Light lightComponent;

        [Header("Feedback")]
        [SerializeField, Range(0f, 1f)] private float flickerAmount = 0.1f;
        [SerializeField, Min(0.1f)] private float flickerSpeed = 12f;
        [SerializeField] private AudioClip onSound;
        [SerializeField] private AudioClip offSound;

        [Header("Initial State")]
        [SerializeField] private bool startsLit;

        private float baseIntensity;

        public bool IsLit { get; private set; }

        public event Action<bool> LitChanged;

        protected float Remaining { get; set; }

        protected bool StartsLit => startsLit;

        protected abstract float InitialRemaining { get; }

        protected virtual float FlickerMultiplier => 1f;

        public void Toggle()
        {
            if (IsLit)
            {
                TurnOff();
            }
            else
            {
                TurnOn();
            }
        }

        public void TurnOn()
        {
            if (!IsLit && CanTurnOn())
            {
                SetLit(true, true);
            }
        }

        public void TurnOff()
        {
            if (IsLit)
            {
                SetLit(false, true);
            }
        }

        public virtual void Extinguish()
        {
            TurnOff();
        }

        public LightRuntimeState Capture()
        {
            return new LightRuntimeState
            {
                Initialized = true,
                Lit = IsLit,
                Remaining = Remaining
            };
        }

        public void Restore(LightRuntimeState state)
        {
            if (state == null || !state.Initialized)
            {
                return;
            }

            Remaining = state.Remaining;
            SetLit(state.Lit, false);
        }

        protected abstract bool CanTurnOn();

        protected virtual void Tick(float deltaTime)
        {
        }

        protected void Deplete()
        {
            Remaining = 0f;
            SetLit(false, true);
        }

        protected static void PlayDialogue(DialogueSequence sequence)
        {
            if (sequence != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.Play(sequence, false);
            }
        }

        protected virtual void Awake()
        {
            baseIntensity = lightComponent != null ? lightComponent.intensity : 1f;
            Remaining = InitialRemaining;
            SetLit(startsLit, false);
        }

        private void Update()
        {
            if (!IsLit)
            {
                return;
            }

            Tick(Time.deltaTime);

            if (IsLit && lightComponent != null)
            {
                var noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
                lightComponent.intensity = baseIntensity * (1f - flickerAmount * FlickerMultiplier * noise);
            }
        }

        private void SetLit(bool lit, bool playSound)
        {
            var changed = IsLit != lit;
            IsLit = lit;

            if (lightComponent != null)
            {
                lightComponent.enabled = lit;
                lightComponent.intensity = baseIntensity;
            }

            if (changed && playSound)
            {
                var clip = lit ? onSound : offSound;
                if (clip != null)
                {
                    AudioSource.PlayClipAtPoint(clip, transform.position);
                }
            }

            if (changed)
            {
                LitChanged?.Invoke(lit);
            }
        }

        private void OnValidate()
        {
            if (lightComponent == null)
            {
                Debug.LogWarning($"{name}: LightSource has no Light assigned.", this);
            }
        }
    }
}
