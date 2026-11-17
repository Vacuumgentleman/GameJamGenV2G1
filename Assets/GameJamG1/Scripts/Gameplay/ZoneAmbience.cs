using UnityEngine;

namespace GameJamG1.Gameplay
{
    // Ambiente en bucle que solo se oye con el jugador dentro de una zona (p. ej. el sotano).
    // Funde la entrada y la salida para que no corte de golpe.
    [RequireComponent(typeof(AudioSource))]
    public class ZoneAmbience : MonoBehaviour
    {
        [Tooltip("World-space box where the ambience is heard, centred on this object.")]
        [SerializeField] private Vector3 size = new(14f, 4f, 16f);
        [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
        [SerializeField, Min(0.01f)] private float fadeSeconds = 1.5f;
        [SerializeField] private string playerTag = "Player";

        private AudioSource source;
        private Transform player;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
        }

        private void Update()
        {
            if (player == null)
            {
                var found = GameObject.FindGameObjectWithTag(playerTag);
                player = found != null ? found.transform : null;
                if (player == null)
                {
                    return;
                }
            }

            var local = player.position - transform.position;
            var inside = Mathf.Abs(local.x) <= size.x * 0.5f && Mathf.Abs(local.y) <= size.y * 0.5f && Mathf.Abs(local.z) <= size.z * 0.5f;

            source.volume = Mathf.MoveTowards(source.volume, inside ? volume : 0f, Time.deltaTime * volume / fadeSeconds);

            if (source.volume > 0f && !source.isPlaying)
            {
                source.Play();
            }
            else if (source.volume <= 0f && source.isPlaying)
            {
                source.Stop();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.4f);
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}
