using UnityEngine;

namespace GameJamG1.Player
{
    [CreateAssetMenu(fileName = "FirstPersonSettings", menuName = "GameJamG1/Player/First Person Settings")]
    public class FirstPersonSettings : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float sprintSpeed = 7f;
        [SerializeField, Min(0f)] private float acceleration = 40f;

        [Header("Jump")]
        [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
        [SerializeField, Min(0f)] private float gravity = 20f;
        [SerializeField, Min(0f)] private float groundedStickForce = 2f;

        [Header("Look")]
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.1f;
        [SerializeField, Min(0f)] private float stickSensitivity = 150f;
        [SerializeField, Range(0f, 90f)] private float maxPitch = 85f;

        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float Acceleration => acceleration;
        public float JumpHeight => jumpHeight;
        public float Gravity => gravity;
        public float GroundedStickForce => groundedStickForce;
        public float MouseSensitivity => mouseSensitivity;
        public float StickSensitivity => stickSensitivity;
        public float MaxPitch => maxPitch;
    }
}
