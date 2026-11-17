using System.Collections;
using System.Collections.Generic;
using GameJamG1.Hallucination;
using UnityEngine;

namespace GameJamG1.Gameplay
{
    // Grupo de luces de una habitacion que titilan a rafagas y se apagan durante periodos,
    // para que la linterna y la vela hagan falta. Con mas alucinacion, mas a menudo y mas tiempo.
    public class FlickeringLights : MonoBehaviour
    {
        [SerializeField] private List<Light> lights = new();

        [Header("Flicker")]
        [SerializeField] private bool flicker = true;
        [Tooltip("Seconds between flicker bursts (min, max).")]
        [SerializeField] private Vector2 flickerInterval = new(30f, 75f);
        [Tooltip("Length of a burst in seconds (min, max).")]
        [SerializeField] private Vector2 burstDuration = new(0.2f, 0.7f);
        [SerializeField, Range(0f, 1f)] private float flickerFloor = 0.05f;
        [Tooltip("Never goes fully dark (outside lights: the monster must stay visible). Disables outages.")]
        [SerializeField] private bool neverOff;
        [Tooltip("Lowest brightness while flickering when 'Never Off' is on.")]
        [SerializeField, Range(0f, 1f)] private float neverOffFloor = 0.35f;

        [Header("Outages")]
        [SerializeField] private bool outages;
        [Tooltip("Seconds the lights stay on between outages (min, max).")]
        [SerializeField] private Vector2 onDuration = new(70f, 140f);
        [Tooltip("Seconds the lights stay off (min, max).")]
        [SerializeField] private Vector2 offDuration = new(15f, 30f);
        [Tooltip("Start in the middle of an outage.")]
        [SerializeField] private bool startOff;

        [Header("Hallucination")]
        [Tooltip("Per hallucination level: intervals shrink and outages grow by this fraction.")]
        [SerializeField, Range(0f, 1f)] private float levelScale = 0.15f;

        [Header("Audio")]
        [SerializeField] private AudioClip buzzClip;
        [SerializeField] private AudioClip offClip;
        [SerializeField] private AudioClip onClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.25f;

        private readonly List<float> baseIntensity = new();
        private AudioSource source;
        private bool off;

        public bool IsOff => off;

        private void Awake()
        {
            lights.RemoveAll(l => l == null);
            foreach (var l in lights)
            {
                baseIntensity.Add(l.intensity);
            }

            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 8f;
        }

        private void OnEnable()
        {
            if (flicker)
            {
                StartCoroutine(FlickerLoop());
            }

            if (outages && !neverOff)
            {
                StartCoroutine(OutageLoop());
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            off = false;
            SetLevel(1f);
        }

        private float Pressure => 1f + levelScale * (HallucinationManager.Instance != null ? HallucinationManager.Instance.Level : 0);

        private IEnumerator FlickerLoop()
        {
            yield return new WaitForSeconds(Random.Range(0f, flickerInterval.y));

            while (true)
            {
                if (!off)
                {
                    yield return Burst(Random.Range(burstDuration.x, burstDuration.y));
                }

                yield return new WaitForSeconds(Random.Range(flickerInterval.x, flickerInterval.y) / Pressure);
            }
        }

        private IEnumerator OutageLoop()
        {
            if (startOff)
            {
                yield return Outage();
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(0f, onDuration.y) / Pressure);
            }

            while (true)
            {
                yield return Burst(0.5f);
                yield return Outage();
                yield return new WaitForSeconds(Random.Range(onDuration.x, onDuration.y) / Pressure);
            }
        }

        private IEnumerator Outage()
        {
            off = true;
            SetLevel(0f);
            Play(offClip);
            yield return new WaitForSeconds(Random.Range(offDuration.x, offDuration.y) * Pressure);
            yield return Burst(0.5f);
            off = false;
            SetLevel(1f);
            Play(onClip);
        }

        // Parpadeo irregular: saltos bruscos entre casi apagado y encendido.
        private IEnumerator Burst(float duration)
        {
            if (buzzClip != null)
            {
                source.clip = buzzClip;
                source.loop = true;
                source.volume = volume * 0.4f;
                source.Play();
            }

            var end = Time.time + duration;
            while (Time.time < end)
            {
                var low = neverOff ? Random.Range(neverOffFloor, neverOffFloor + 0.15f) : Random.Range(0f, flickerFloor * 2f);
                SetLevel(Random.value < 0.45f ? low : Random.Range(0.6f, 1f));
                yield return new WaitForSeconds(Random.Range(0.03f, 0.12f));
            }

            source.loop = false;
            source.Stop();
            SetLevel(off ? 0f : 1f);
        }

        private void SetLevel(float level)
        {
            // Tras una recarga de dominio en Play mode la lista privada llega vacia: se rehace.
            while (baseIntensity.Count < lights.Count)
            {
                var l = lights[baseIntensity.Count];
                baseIntensity.Add(l != null ? l.intensity : 1f);
            }

            for (var i = 0; i < lights.Count; i++)
            {
                var l = lights[i];
                if (l == null)
                {
                    continue;
                }

                l.intensity = baseIntensity[i] * level;
                l.enabled = level > 0.001f;
            }
        }

        private void Play(AudioClip clip)
        {
            if (clip != null)
            {
                source.PlayOneShot(clip, volume);
            }
        }
    }
}
