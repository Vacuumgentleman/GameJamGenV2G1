using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameJamG1.Hallucination
{
    public class HallucinationManager : MonoBehaviour
    {
        [Header("Levels")]
        [SerializeField, Min(0)] private int minLevel;
        [SerializeField, Min(0)] private int maxLevel = 3;
        [SerializeField, Min(0)] private int startLevel;
        [Tooltip("Profile for each level, indexed by level. Missing entries mean no effect.")]
        [SerializeField] private List<HallucinationLevelProfile> profiles = new();

        [Header("Output")]
        [Tooltip("Global volume that receives the profile of the current level.")]
        [SerializeField] private Volume volume;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField, Min(0f)] private float transitionDuration = 1.5f;

        [Header("Events")]
        [SerializeField] private UnityEvent<int> onLevelChanged = new();

        private Coroutine transition;
        private VolumeProfile runtimeProfile;
        private LensDistortion wobbleLens;
        private float wobbleBase;
        private float wobbleAmount;
        private float wobbleSpeed;

        public static HallucinationManager Instance { get; private set; }

        public int Level { get; private set; }

        public int MinLevel => minLevel;

        public int MaxLevel => maxLevel;

        public event Action<int> LevelChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;
            Level = Mathf.Clamp(startLevel, minLevel, maxLevel);
        }

        private void Start()
        {
            Apply(true);
        }

        private void Update()
        {
            if (wobbleLens == null || wobbleAmount <= 0f)
            {
                return;
            }

            var noise = Mathf.PerlinNoise(Time.time * wobbleSpeed, 0.37f) * 2f - 1f;
            wobbleLens.intensity.value = Mathf.Clamp(wobbleBase + noise * wobbleAmount, -1f, 1f);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            DestroyRuntimeProfile();
        }

        public void Increase()
        {
            SetLevel(Level + 1);
        }

        public void Decrease()
        {
            SetLevel(Level - 1);
        }

        public void SetLevel(int level)
        {
            level = Mathf.Clamp(level, minLevel, maxLevel);

            if (level == Level)
            {
                return;
            }

            Level = level;
            Apply(false);
            LevelChanged?.Invoke(Level);
            onLevelChanged.Invoke(Level);
        }

        private HallucinationLevelProfile CurrentProfile()
        {
            return Level >= 0 && Level < profiles.Count ? profiles[Level] : null;
        }

        private void Apply(bool immediate)
        {
            if (transition != null)
            {
                StopCoroutine(transition);
            }

            if (immediate || transitionDuration <= 0f)
            {
                transition = null;
                ApplyProfile(CurrentProfile());
                SetWeight(TargetWeight(CurrentProfile()));
                return;
            }

            transition = StartCoroutine(Transition(CurrentProfile()));
        }

        private IEnumerator Transition(HallucinationLevelProfile next)
        {
            var half = transitionDuration * 0.5f;
            yield return FadeWeight(0f, half);
            ApplyProfile(next);
            yield return FadeWeight(TargetWeight(next), half);
            transition = null;
        }

        private IEnumerator FadeWeight(float target, float duration)
        {
            if (volume == null)
            {
                yield break;
            }

            var start = volume.weight;
            var elapsed = 0f;

            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime / Mathf.Max(duration, 0.01f);
                volume.weight = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed));
                yield return null;
            }

            volume.weight = target;
        }

        private void ApplyProfile(HallucinationLevelProfile profile)
        {
            ApplyVolumeProfile(profile);

            if (ambientSource == null)
            {
                return;
            }

            var clip = profile != null ? profile.AmbientClip : null;

            if (clip == null)
            {
                ambientSource.Stop();
                return;
            }

            ambientSource.clip = clip;
            ambientSource.volume = profile.AmbientVolume;
            ambientSource.loop = true;
            ambientSource.Play();
        }

        // Copia profunda del perfil: el wobble modifica la copia, nunca el asset del proyecto.
        private void ApplyVolumeProfile(HallucinationLevelProfile profile)
        {
            DestroyRuntimeProfile();

            if (volume == null)
            {
                return;
            }

            var source = profile != null ? profile.VolumeProfile : null;
            if (source == null)
            {
                volume.sharedProfile = null;
                return;
            }

            runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            runtimeProfile.name = source.name + " (Runtime)";
            foreach (var component in source.components)
            {
                if (component != null)
                {
                    runtimeProfile.components.Add(Instantiate(component));
                }
            }

            volume.sharedProfile = runtimeProfile;

            if (profile.DistortionWobble > 0f && runtimeProfile.TryGet(out LensDistortion lens))
            {
                wobbleLens = lens;
                wobbleBase = lens.intensity.value;
                wobbleAmount = profile.DistortionWobble;
                wobbleSpeed = profile.DistortionWobbleSpeed;
            }
        }

        private void DestroyRuntimeProfile()
        {
            wobbleLens = null;
            wobbleAmount = 0f;

            if (runtimeProfile == null)
            {
                return;
            }

            foreach (var component in runtimeProfile.components)
            {
                if (component != null)
                {
                    Destroy(component);
                }
            }

            Destroy(runtimeProfile);
            runtimeProfile = null;
        }

        private static float TargetWeight(HallucinationLevelProfile profile)
        {
            return profile != null && profile.VolumeProfile != null ? profile.Intensity : 0f;
        }

        private void SetWeight(float weight)
        {
            if (volume != null)
            {
                volume.weight = weight;
            }
        }

        private void OnValidate()
        {
            if (maxLevel < minLevel)
            {
                maxLevel = minLevel;
            }

            if (volume == null)
            {
                Debug.LogWarning($"{name}: HallucinationManager has no volume assigned.", this);
            }
        }
    }
}
