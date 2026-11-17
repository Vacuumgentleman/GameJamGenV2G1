using UnityEngine;

namespace GameJamG1.Enemy
{
    // Sonidos del monstruo: gruñido en bucle mientras esta activo, rugido al empezar a perseguir y golpe al atacar.
    // Durante la persecucion se suma una capa de gruñido agudo y una tension (latido y disonancia) que
    // se oye "en la cabeza" del jugador y se apaga poco a poco al terminar.
    [RequireComponent(typeof(MonsterChaser))]
    public class MonsterAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip growlLoop;
        [SerializeField, Range(0f, 1f)] private float growlVolume = 0.6f;
        [SerializeField] private AudioClip screamClip;
        [SerializeField, Range(0f, 1f)] private float screamVolume = 1f;
        [Tooltip("Played together with the roar when the chase starts.")]
        [SerializeField] private AudioClip roarCompanion;
        [SerializeField, Range(0f, 1f)] private float roarCompanionVolume = 0.9f;
        [SerializeField] private AudioClip attackClip;
        [SerializeField, Range(0f, 1f)] private float attackVolume = 1f;
        [SerializeField, Min(1f)] private float maxDistance = 25f;

        [Header("Chase layer")]
        [Tooltip("Extra loop layered on top of the growl only while chasing.")]
        [SerializeField] private AudioClip chaseLayerLoop;
        [SerializeField, Range(0f, 1f)] private float chaseLayerVolume = 0.6f;
        [SerializeField, Range(0.5f, 2f)] private float chaseLayerPitch = 1.15f;
        [Tooltip("Extra scream layered on top of the scream when the chase starts.")]
        [SerializeField] private AudioClip chaseLayerScream;
        [SerializeField, Range(0f, 1f)] private float chaseLayerScreamVolume = 0.8f;

        [Header("Chase tension")]
        [Tooltip("Non-spatial loop (heartbeat, dissonance) heard while chased.")]
        [SerializeField] private AudioClip chaseTensionLoop;
        [SerializeField, Range(0f, 1f)] private float tensionVolume = 0.55f;
        [SerializeField, Range(0.5f, 2f)] private float tensionPitch = 1.2f;
        [SerializeField, Min(0.01f)] private float tensionFadeIn = 0.8f;
        [Tooltip("Seconds the tension takes to fade after the chase ends.")]
        [SerializeField, Min(0.01f)] private float tensionFadeOut = 6f;

        private MonsterChaser monster;
        private AudioSource loopSource;
        private AudioSource layerSource;
        private AudioSource oneShotSource;
        private AudioSource tensionSource;

        private void Awake()
        {
            monster = GetComponent<MonsterChaser>();
            loopSource = CreateSource(true);
            layerSource = CreateSource(true);
            oneShotSource = CreateSource(false);
            tensionSource = CreateSource(true);
            tensionSource.spatialBlend = 0f;
            tensionSource.volume = 0f;
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

        private void Update()
        {
            if (chaseTensionLoop == null)
            {
                return;
            }

            var chasing = monster.State == EnemyState.Chase;
            var target = chasing ? tensionVolume : 0f;
            var rate = tensionVolume / (chasing ? tensionFadeIn : tensionFadeOut);
            tensionSource.volume = Mathf.MoveTowards(tensionSource.volume, target, rate * Time.deltaTime);

            if (tensionSource.volume > 0f && !tensionSource.isPlaying)
            {
                tensionSource.clip = chaseTensionLoop;
                tensionSource.pitch = tensionPitch;
                tensionSource.Play();
            }
            else if (tensionSource.volume <= 0f && tensionSource.isPlaying)
            {
                tensionSource.Stop();
            }
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
                    layerSource.pitch = chaseLayerPitch;
                    layerSource.Play();
                }

                return;
            }

            layerSource.Stop();
        }

        private void HandleState(EnemyState state)
        {
            UpdateChaseLayer(state);

            if (state == EnemyState.Inactive)
            {
                loopSource.Stop();
                tensionSource.Stop();
                tensionSource.volume = 0f;
                return;
            }

            if (growlLoop != null && !loopSource.isPlaying)
            {
                loopSource.clip = growlLoop;
                loopSource.volume = growlVolume;
                loopSource.Play();
            }

            loopSource.pitch = state == EnemyState.Chase ? 1.15f : 1f;

            if (state != EnemyState.Chase)
            {
                return;
            }

            if (screamClip != null)
            {
                oneShotSource.PlayOneShot(screamClip, screamVolume);
            }

            if (roarCompanion != null)
            {
                oneShotSource.PlayOneShot(roarCompanion, roarCompanionVolume);
            }

            if (chaseLayerScream != null)
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
