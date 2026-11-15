using System;
using System.Collections;
using System.Collections.Generic;
using GameJamG1.Enemy;
using GameJamG1.Player;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GameJamG1.Hallucination
{
    public class BreathingMinigame : MonoBehaviour
    {
        private const float MinScale = 0.15f;

        [Header("UI")]
        [SerializeField] private CanvasGroup panel;
        [SerializeField] private RectTransform circle;
        [SerializeField] private Image circleImage;
        [SerializeField] private RectTransform zoneMinRing;
        [SerializeField] private RectTransform zoneMaxRing;
        [SerializeField] private Image progressFill;
        [SerializeField] private TMP_Text hintLabel;
        [SerializeField] private string hint = "Manten ESPACIO para inhalar. Suelta para exhalar.";
        [SerializeField, Min(0f)] private float fadeDuration = 0.25f;
        [SerializeField] private Color outOfZoneColor = new(1f, 1f, 1f, 0.5f);
        [SerializeField] private Color inZoneColor = new(0.5f, 1f, 0.6f, 0.8f);

        [Header("Rules")]
        [Tooltip("Circle size gained per second while the key is held (0 to 1).")]
        [SerializeField, Min(0.01f)] private float inhaleSpeed = 0.35f;
        [Tooltip("Circle size lost per second while the key is released.")]
        [SerializeField, Min(0.01f)] private float exhaleSpeed = 0.3f;
        [SerializeField, Range(0f, 1f)] private float startSize = 0.15f;
        [Tooltip("Center of the target zone (0 to 1).")]
        [SerializeField, Range(0.1f, 0.9f)] private float zoneCenter = 0.55f;
        [SerializeField, Range(0.02f, 0.3f)] private float zoneHalfWidth = 0.1f;
        [Tooltip("Seconds inside the zone needed to succeed.")]
        [SerializeField, Min(0.5f)] private float requiredTimeInZone = 4f;
        [Tooltip("Seconds before the attempt fails. 0 means no limit: the minigame only ends on success or Cancel.")]
        [SerializeField, Min(0f)] private float timeLimit;
        [SerializeField, Min(0f)] private float resultHoldSeconds = 0.6f;
        [Tooltip("Fraction of time progress lost per second while outside the zone.")]
        [SerializeField, Range(0f, 2f)] private float outOfZoneDecay = 0.5f;

        [Header("Difficulty By Hallucination Level")]
        [Tooltip("Index = hallucination level. Multipliers applied to the rules above. The last entry is used for higher levels.")]
        [SerializeField] private List<LevelDifficulty> difficultyByLevel = new()
        {
            new LevelDifficulty(1f, 1f, 1f, 1f, 0f, 0f),
            new LevelDifficulty(0.92f, 1.1f, 1.05f, 1f, 0.02f, 0.06f),
            new LevelDifficulty(0.85f, 1.2f, 1.1f, 1f, 0.04f, 0.12f),
            new LevelDifficulty(0.78f, 1.3f, 1.2f, 1f, 0.06f, 0.18f)
        };
        [Tooltip("How fast the random push on the circle changes.")]
        [SerializeField, Min(0.5f)] private float wobbleSpeed = 2.5f;
        [Tooltip("Speed of the zone drift in radians per second.")]
        [SerializeField, Min(0.1f)] private float driftSpeed = 1.5f;

        [Header("Availability")]
        [SerializeField, Min(0f)] private float retryCooldown;
        [SerializeField] private bool canBeRetriggered = true;
        [SerializeField] private bool lockPlayer = true;

        [Header("Noise")]
        [SerializeField] private NoiseChannel noiseChannel;
        [SerializeField, Min(0f)] private float noiseIntensity = 0.5f;
        [SerializeField, Min(0.1f)] private float noiseInterval = 1.2f;

        [Header("Events")]
        [SerializeField] private UnityEvent onBegin = new();
        [SerializeField] private UnityEvent onSuccess = new();
        [SerializeField] private UnityEvent onFail = new();

        private Coroutine routine;
        private PlayerInputReader input;
        private PlayerControlLock playerLock;
        private float cooldownEnd;
        private bool hasPlayed;

        [Serializable]
        private class LevelDifficulty
        {
            [Tooltip("Multiplier of the zone width.")]
            [SerializeField, Range(0.2f, 1.5f)] private float zoneWidth = 1f;
            [Tooltip("Multiplier of inhale and exhale speed.")]
            [SerializeField, Range(0.5f, 3f)] private float speed = 1f;
            [Tooltip("Multiplier of the time needed inside the zone.")]
            [SerializeField, Range(0.5f, 3f)] private float timeRequired = 1f;
            [Tooltip("Multiplier of the time before the attempt fails.")]
            [SerializeField, Range(0.3f, 1.5f)] private float duration = 1f;
            [Tooltip("How far the zone drifts back and forth, as a fraction of the circle size.")]
            [SerializeField, Range(0f, 0.2f)] private float drift;
            [Tooltip("Random push on the circle in size units per second. Makes the circle unstable.")]
            [SerializeField, Range(0f, 1.5f)] private float instability;

            public LevelDifficulty()
            {
            }

            public LevelDifficulty(float zoneWidth, float speed, float timeRequired, float duration, float drift, float instability)
            {
                this.zoneWidth = zoneWidth;
                this.speed = speed;
                this.timeRequired = timeRequired;
                this.duration = duration;
                this.drift = drift;
                this.instability = instability;
            }

            public float ZoneWidth => zoneWidth;
            public float Speed => speed;
            public float TimeRequired => timeRequired;
            public float Duration => duration;
            public float Drift => drift;
            public float Instability => instability;
        }

        public bool IsActive { get; private set; }

        public float CooldownRemaining => Mathf.Max(0f, cooldownEnd - Time.time);

        public bool CanBegin => !IsActive && CooldownRemaining <= 0f && (canBeRetriggered || !hasPlayed);

        private void Awake()
        {
            panel.alpha = 0f;
            panel.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (IsActive)
            {
                Cleanup();
            }
        }

        public void Begin()
        {
            if (CanBegin)
            {
                routine = StartCoroutine(Run());
            }
        }

        public void Cancel()
        {
            if (!IsActive)
            {
                return;
            }

            StopCoroutine(routine);
            Cleanup();
        }

        private IEnumerator Run()
        {
            IsActive = true;
            hasPlayed = true;

            input = FindFirstObjectByType<PlayerInputReader>();
            playerLock = input != null ? input.GetComponent<PlayerControlLock>() : null;

            if (lockPlayer && playerLock != null)
            {
                playerLock.Lock(this);
            }

            onBegin.Invoke();

            var difficulty = CurrentDifficulty();
            var halfWidth = zoneHalfWidth * difficulty.ZoneWidth;
            var inhale = inhaleSpeed * difficulty.Speed;
            var exhale = exhaleSpeed * difficulty.Speed;
            var required = requiredTimeInZone * difficulty.TimeRequired;
            var limit = timeLimit > 0f ? timeLimit * difficulty.Duration : float.PositiveInfinity;
            var wobbleSeed = UnityEngine.Random.value * 100f;
            PlaceZone(zoneCenter, halfWidth);

            if (hintLabel != null)
            {
                hintLabel.text = hint;
            }

            panel.gameObject.SetActive(true);
            yield return FadePanel(1f);

            var size = startSize;
            var timeInZone = 0f;
            var elapsed = 0f;
            var noiseTimer = 0f;
            var success = false;

            while (elapsed < limit)
            {
                var dt = Time.deltaTime;
                var inhaling = input != null && input.BreatheHeld;

                var wobble = (Mathf.PerlinNoise(elapsed * wobbleSpeed, wobbleSeed) - 0.5f) * 2f;
                size = Mathf.Clamp01(size + ((inhaling ? inhale : -exhale) + wobble * difficulty.Instability) * dt);

                var center = Mathf.Clamp(zoneCenter + difficulty.Drift * Mathf.Sin(elapsed * driftSpeed), halfWidth, 1f - halfWidth);
                PlaceZone(center, halfWidth);

                var inZone = Mathf.Abs(size - center) <= halfWidth;
                timeInZone = inZone ? timeInZone + dt : Mathf.Max(0f, timeInZone - dt * outOfZoneDecay);
                elapsed += dt;

                if (inhaling)
                {
                    noiseTimer -= dt;
                    if (noiseTimer <= 0f)
                    {
                        EmitNoise();
                        noiseTimer = noiseInterval;
                    }
                }

                circle.localScale = Vector3.one * Scale(size);
                circleImage.color = inZone ? inZoneColor : outOfZoneColor;
                progressFill.fillAmount = Mathf.Clamp01(timeInZone / required);

                if (timeInZone >= required)
                {
                    success = true;
                    break;
                }

                yield return null;
            }

            if (resultHoldSeconds > 0f)
            {
                yield return new WaitForSeconds(resultHoldSeconds);
            }

            yield return FadePanel(0f);
            panel.gameObject.SetActive(false);
            Finish(success);
        }

        private void Finish(bool success)
        {
            Cleanup();

            var hallucination = HallucinationManager.Instance;
            if (hallucination != null)
            {
                if (success)
                {
                    hallucination.Decrease();
                }
                else
                {
                    hallucination.Increase();
                }
            }

            (success ? onSuccess : onFail).Invoke();
        }

        private void Cleanup()
        {
            IsActive = false;
            routine = null;
            cooldownEnd = Time.time + retryCooldown;

            if (playerLock != null)
            {
                playerLock.Unlock(this);
            }

            panel.alpha = 0f;
            panel.gameObject.SetActive(false);
        }

        private IEnumerator FadePanel(float target)
        {
            var start = panel.alpha;
            var elapsed = 0f;

            while (elapsed < 1f)
            {
                elapsed += Time.unscaledDeltaTime / Mathf.Max(fadeDuration, 0.01f);
                panel.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed));
                yield return null;
            }

            panel.alpha = target;
        }

        private void EmitNoise()
        {
            if (noiseChannel != null && input != null)
            {
                noiseChannel.Emit(input.transform.position, noiseIntensity);
            }
        }

        private LevelDifficulty CurrentDifficulty()
        {
            if (difficultyByLevel.Count == 0)
            {
                return new LevelDifficulty();
            }

            var level = HallucinationManager.Instance != null ? HallucinationManager.Instance.Level : 0;
            return difficultyByLevel[Mathf.Clamp(level, 0, difficultyByLevel.Count - 1)];
        }

        private void PlaceZone(float center, float halfWidth)
        {
            zoneMinRing.localScale = Vector3.one * Scale(center - halfWidth);
            zoneMaxRing.localScale = Vector3.one * Scale(center + halfWidth);
        }

        private static float Scale(float size)
        {
            return Mathf.Lerp(MinScale, 1f, size);
        }

        private void OnValidate()
        {
            if (panel == null || circle == null || circleImage == null || zoneMinRing == null || zoneMaxRing == null || progressFill == null)
            {
                Debug.LogWarning($"{name}: BreathingMinigame is missing a UI reference.", this);
            }
        }
    }
}
