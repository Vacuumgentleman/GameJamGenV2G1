using UnityEngine;

namespace GameJamG1.Gameplay
{
    // Señal visible de que un aparato esta encendido. Vive en un hijo que ElectronicsController
    // activa y desactiva con el aparato (lista 'Active While On').
    // Pantalla: luz azulada que cambia como la imagen de una tele. Maquina: piloto que pulsa y el aparato vibra.
    [RequireComponent(typeof(Light))]
    public class DeviceIndicator : MonoBehaviour
    {
        public enum Kind
        {
            Screen,
            Machine
        }

        [SerializeField] private Kind kind = Kind.Screen;

        [Header("Screen")]
        [SerializeField] private Vector2 screenIntensity = new(0.8f, 2.2f);
        [SerializeField] private Vector2 screenChangeInterval = new(0.06f, 0.35f);

        [Header("Machine")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private Vector2 pulseIntensity = new(0.5f, 1.4f);
        [Tooltip("Object that vibrates while on. Empty: the parent.")]
        [SerializeField] private Transform shakeTarget;
        [SerializeField, Min(0f)] private float shakeAmount = 0.012f;
        [SerializeField, Min(0f)] private float shakeSpeed = 28f;

        private Light glow;
        private Color baseColor;
        private float nextChange;
        private Vector3 restPosition;

        private void Awake()
        {
            glow = GetComponent<Light>();
            baseColor = glow.color;

            if (shakeTarget == null && transform.parent != null)
            {
                shakeTarget = transform.parent;
            }
        }

        private void OnEnable()
        {
            if (kind == Kind.Machine && shakeTarget != null)
            {
                restPosition = shakeTarget.position;
            }

            nextChange = 0f;
        }

        private void OnDisable()
        {
            if (kind == Kind.Machine && shakeTarget != null)
            {
                shakeTarget.position = restPosition;
            }
        }

        private void Update()
        {
            if (kind == Kind.Screen)
            {
                if (Time.time >= nextChange)
                {
                    nextChange = Time.time + Random.Range(screenChangeInterval.x, screenChangeInterval.y);
                    glow.intensity = Random.Range(screenIntensity.x, screenIntensity.y);
                    glow.color = Color.Lerp(baseColor, new Color(Random.value * 0.4f + 0.6f, Random.value * 0.3f + 0.7f, 1f), 0.35f);
                }

                return;
            }

            var pulse = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            glow.intensity = Mathf.Lerp(pulseIntensity.x, pulseIntensity.y, pulse);

            if (shakeTarget != null)
            {
                var t = Time.time * shakeSpeed;
                var offset = new Vector3(Mathf.PerlinNoise(t, 0.1f) - 0.5f, Mathf.PerlinNoise(0.3f, t) - 0.5f, Mathf.PerlinNoise(t, 0.7f) - 0.5f);
                shakeTarget.position = restPosition + offset * (shakeAmount * 2f);
            }
        }
    }
}
