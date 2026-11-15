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

        [Header("Audio")]
        [SerializeField] private AudioClip ambientClip;
        [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.5f;

        public string Description => description;
        public VolumeProfile VolumeProfile => volumeProfile;
        public float Intensity => intensity;
        public AudioClip AmbientClip => ambientClip;
        public float AmbientVolume => ambientVolume;
    }
}
