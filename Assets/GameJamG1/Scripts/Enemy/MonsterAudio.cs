using UnityEngine;

namespace GameJamG1.Enemy
{
    // Sonidos del monstruo: gruñido en bucle mientras esta activo, grito al empezar a perseguir y golpe al atacar.
    [RequireComponent(typeof(MonsterChaser))]
    public class MonsterAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip growlLoop;
        [SerializeField, Range(0f, 1f)] private float growlVolume = 0.6f;
        [SerializeField] private AudioClip screamClip;
        [SerializeField, Range(0f, 1f)] private float screamVolume = 1f;
        [SerializeField] private AudioClip attackClip;
        [SerializeField, Range(0f, 1f)] private float attackVolume = 1f;
        [SerializeField, Min(1f)] private float maxDistance = 25f;

        [Header("Chase layer")]
        [Tooltip("Extra loop layered on top of the growl only while chasing.")]
        [SerializeField] private AudioClip chaseLayerLoop;
        [SerializeField, Range(0f, 1f)] private float chaseLayerVolume = 0.6f;
        [Tooltip("Extra scream layered on top of the scream when the chase starts.")]
        [SerializeField] private AudioClip chaseLayerScream;
        [SerializeField, Range(0f, 1f)] private float chaseLayerScreamVolume = 0.8f;

        private MonsterChaser monster;
        private AudioSource loopSource;
        private AudioSource layerSource;
        private AudioSource oneShotSource;

        private void Awake()
        {
            monster = GetComponent<MonsterChaser>();
            loopSource = CreateSource(true);
            layerSource = CreateSource(true);
            oneShotSource = CreateSource(false);
        }

        // Capa de persecucion: suena junto al sonido normal solo mientras persigue.
        private void UpdateChaseLayer(EnemyState state)
        {
            if (state == EnemyState.Chase && chaseLayerLoop != null)
            {
                if (!layerSource.isPlaying)
                {
                    layerSource.clip = chaseLayerLoop;
                    layerSource.volume = chaseLayerVolume;
                    layerSource.Play();
                }

                return;
            }

            layerSource.Stop();
        }

        private void OnEnable()
        {
            monster.StateChanged += HandleState;
            monster.AttackHit += HandleAttack;
        }

        private void OnDisable()
        {
            monster.StateChanged -= HandleState;
            monster.AttackHit -= HandleAttack;
        }

        private void Start()
        {
            HandleState(monster.State);
        }

        private void HandleState(EnemyState state)
        {
            UpdateChaseLayer(state);

            if (state == EnemyState.Inactive)
            {
                loopSource.Stop();
                return;
            }

            if (growlLoop != null && !loopSource.isPlaying)
            {
                loopSource.clip = growlLoop;
                loopSource.volume = growlVolume;
                loopSource.Play();
            }

            loopSource.pitch = state == EnemyState.Chase ? 1.15f : 1f;

            if (state == EnemyState.Chase && screamClip != null)
            {
                oneShotSource.PlayOneShot(screamClip, screamVolume);
            }

            if (state == EnemyState.Chase && chaseLayerScream != null)
            {
                oneShotSource.PlayOneShot(chaseLayerScream, chaseLayerScreamVolume);
            }
        }

        private void HandleAttack()
        {
            if (attackClip != null)
            {
                oneShotSource.PlayOneShot(attackClip, attackVolume);
            }
        }

        private AudioSource CreateSource(bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 2f;
            source.maxDistance = maxDistance;
            source.dopplerLevel = 0f;
            return source;
        }
    }
}
