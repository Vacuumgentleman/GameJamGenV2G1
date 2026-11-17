using UnityEngine;
using UnityEngine.Rendering;

namespace GameJamG1.Hallucination
{
    [CreateAssetMenu(fileName = "HallucinationLevel", menuName = "GameJamG1/Hallucination/Level Profile")]
    public class HallucinationLevelProfile : ScriptableObject
    {
        [SerializeField, TextArea] private string description;

        [Header("Visuals")]
        [Tooltip("URP volume profile applied at this level. Empty means no effect.")]
        [SerializeField] private VolumeProfile volumeProfile;
        [Tooltip("Weight of the volume at this level.")]
        [SerializeField, Range(0f, 1f)] private float intensity = 1f;
        [Tooltip("How much the Lens Distortion of the profile wobbles around its value. 0 = static.")]
        [SerializeField, Range(0f, 0.5f)] private float distortionWobble;
        [Tooltip("Speed of the distortion wobble.")]
        [SerializeField, Min(0f)] private float distortionWobbleSpeed = 0.6f;

        [Header("Audio")]
        [SerializeField] private AudioClip ambientClip;
        [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.5f;

        public string Description => description;
        public VolumeProfile VolumeProfile => volumeProfile;
        public float Intensity => intensity;
        public float DistortionWobble => distortionWobble;
        public float DistortionWobbleSpeed => distortionWobbleSpeed;
        public AudioClip AmbientClip => ambientClip;
        public float AmbientVolume => ambientVolume;
    }
}
