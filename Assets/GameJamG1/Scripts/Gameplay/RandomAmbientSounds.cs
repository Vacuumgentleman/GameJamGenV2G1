using UnityEngine;

namespace GameJamG1.Gameplay
{
    // Sonidos sueltos de ambiente (rafagas de viento...) que suenan al azar cada cierto tiempo.
    public class RandomAmbientSounds : MonoBehaviour
    {
        [SerializeField] private AudioClip[] clips;
        [Tooltip("Seconds between two sounds (min, max).")]
        [SerializeField] private Vector2 interval = new(25f, 60f);
        [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
        [SerializeField, Range(0f, 1f)] private float spatialBlend;

        private AudioSource source;
        private float next;

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = spatialBlend;
            next = Time.time + Random.Range(interval.x * 0.3f, interval.y * 0.6f);
        }

        private void Update()
        {
            if (Time.time < next || clips == null || clips.Length == 0)
            {
                return;
            }

            next = Time.time + Random.Range(interval.x, interval.y);
            var clip = clips[Random.Range(0, clips.Length)];
            if (clip != null)
            {
                source.pitch = Random.Range(0.9f, 1.1f);
                source.PlayOneShot(clip, volume);
            }
        }
    }
}
