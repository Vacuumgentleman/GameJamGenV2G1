using UnityEngine;

namespace GameJamG1.Player
{
    // Pasos del jugador: un sonido al azar cada cierto recorrido, mas seguidos al correr y mas lentos agachado.
    // De vez en cuando el suelo cruje (pasos "squeaky").
    [RequireComponent(typeof(CharacterController))]
    public class FootstepAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip[] steps;
        [SerializeField] private AudioClip[] squeakySteps;
        [SerializeField, Range(0f, 1f)] private float squeakChance = 0.08f;
        [SerializeField, Range(0f, 1f)] private float volume = 0.45f;

        [Header("Cadence")]
        [Tooltip("Metres walked between two steps.")]
        [SerializeField, Min(0.1f)] private float strideWalk = 1.6f;
        [SerializeField, Min(0.1f)] private float strideSprint = 2.2f;
        [SerializeField, Min(0.1f)] private float strideCrouch = 1.1f;
        [SerializeField, Min(0f)] private float minSpeed = 0.3f;

        private CharacterController controller;
        private FirstPersonMotor motor;
        private PlayerInputReader input;
        private AudioSource source;
        private float travelled;
        private int lastIndex = -1;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            motor = GetComponent<FirstPersonMotor>();
            input = GetComponent<PlayerInputReader>();

            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        private void Update()
        {
            var velocity = controller.velocity;
            velocity.y = 0f;
            var speed = velocity.magnitude;

            if (!controller.isGrounded || speed < minSpeed)
            {
                travelled = Mathf.Min(travelled, StrideLength() * 0.5f);
                return;
            }

            travelled += speed * Time.deltaTime;

            if (travelled >= StrideLength())
            {
                travelled = 0f;
                PlayStep();
            }
        }

        private float StrideLength()
        {
            if (motor != null && motor.IsCrouching)
            {
                return strideCrouch;
            }

            return input != null && input.SprintHeld ? strideSprint : strideWalk;
        }

        private void PlayStep()
        {
            var squeak = squeakySteps != null && squeakySteps.Length > 0 && Random.value < squeakChance;
            var pool = squeak ? squeakySteps : steps;
            if (pool == null || pool.Length == 0)
            {
                return;
            }

            var index = Random.Range(0, pool.Length);
            if (!squeak && pool.Length > 1 && index == lastIndex)
            {
                index = (index + 1) % pool.Length;
            }

            if (!squeak)
            {
                lastIndex = index;
            }

            var crouched = motor != null && motor.IsCrouching;
            source.pitch = Random.Range(0.93f, 1.07f);
            source.PlayOneShot(pool[index], volume * (crouched ? 0.5f : 1f) * (squeak ? 1.2f : 1f));
        }
    }
}
