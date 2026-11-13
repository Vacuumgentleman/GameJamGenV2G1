using UnityEngine;

namespace GameJamG1.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
    public class FirstPersonMotor : MonoBehaviour
    {
        [SerializeField] private FirstPersonSettings settings;

        private CharacterController controller;
        private PlayerInputReader input;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            UpdateHorizontalVelocity();
            UpdateVerticalVelocity();
            controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        private void UpdateHorizontalVelocity()
        {
            var move = Vector2.ClampMagnitude(input.Move, 1f);
            var speed = input.SprintHeld ? settings.SprintSpeed : settings.WalkSpeed;
            var target = transform.TransformDirection(new Vector3(move.x, 0f, move.y)) * speed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, settings.Acceleration * Time.deltaTime);
        }

        private void UpdateVerticalVelocity()
        {
            if (controller.isGrounded)
            {
                verticalVelocity = -settings.GroundedStickForce;

                if (input.JumpPressed)
                {
                    verticalVelocity = Mathf.Sqrt(2f * settings.JumpHeight * settings.Gravity);
                }

                return;
            }

            if (verticalVelocity > 0f && (controller.collisionFlags & CollisionFlags.Above) != 0)
            {
                verticalVelocity = 0f;
            }

            verticalVelocity -= settings.Gravity * Time.deltaTime;
        }
    }
}
