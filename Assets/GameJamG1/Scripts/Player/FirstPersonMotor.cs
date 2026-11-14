using UnityEngine;

namespace GameJamG1.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
    public class FirstPersonMotor : MonoBehaviour
    {
        [SerializeField] private FirstPersonSettings settings;

        private CharacterController controller;
        private PlayerInputReader input;
        private Transform cameraTransform;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float standingHeight;
        private float feetOffset;
        private float cameraStandingY;

        public bool IsCrouching { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputReader>();
            cameraTransform = GetComponentInChildren<Camera>().transform;

            standingHeight = controller.height;
            feetOffset = controller.center.y - controller.height * 0.5f;
            cameraStandingY = cameraTransform.localPosition.y;
        }

        private void Update()
        {
            UpdateCrouch();
            UpdateHorizontalVelocity();
            UpdateVerticalVelocity();
            controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        private void UpdateCrouch()
        {
            var wantsCrouch = input.CrouchHeld || (IsCrouching && !CanStand());
            IsCrouching = wantsCrouch;

            var target = IsCrouching ? settings.CrouchHeight : standingHeight;
            var height = Mathf.MoveTowards(controller.height, target, settings.CrouchTransitionSpeed * Time.deltaTime);
            ApplyHeight(height);
        }

        private void ApplyHeight(float height)
        {
            controller.height = height;
            controller.center = new Vector3(controller.center.x, feetOffset + height * 0.5f, controller.center.z);

            var position = cameraTransform.localPosition;
            position.y = cameraStandingY - (standingHeight - height);
            cameraTransform.localPosition = position;
        }

        private bool CanStand()
        {
            var topSphere = transform.TransformPoint(controller.center) + Vector3.up * (controller.height * 0.5f - controller.radius);
            var distance = standingHeight - controller.height + 0.05f;
            return !Physics.SphereCast(topSphere, controller.radius * 0.95f, Vector3.up, out _, distance, ~0, QueryTriggerInteraction.Ignore);
        }

        private void UpdateHorizontalVelocity()
        {
            var move = Vector2.ClampMagnitude(input.Move, 1f);
            var target = transform.TransformDirection(new Vector3(move.x, 0f, move.y)) * CurrentSpeed();
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, settings.Acceleration * Time.deltaTime);
        }

        private float CurrentSpeed()
        {
            if (IsCrouching)
            {
                return settings.CrouchSpeed;
            }

            return input.SprintHeld ? settings.SprintSpeed : settings.WalkSpeed;
        }

        private void UpdateVerticalVelocity()
        {
            if (controller.isGrounded)
            {
                verticalVelocity = -settings.GroundedStickForce;
                return;
            }

            verticalVelocity -= settings.Gravity * Time.deltaTime;
        }
    }
}
