using UnityEngine;

namespace GameJamG1.Player
{
    [RequireComponent(typeof(PlayerInputReader))]
    public class FirstPersonLook : MonoBehaviour
    {
        [SerializeField] private FirstPersonSettings settings;
        [SerializeField] private Transform cameraTransform;

        private PlayerInputReader input;
        private float pitch;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            var delta = input.Look * GetSensitivity();
            transform.Rotate(Vector3.up, delta.x, Space.Self);
            pitch = Mathf.Clamp(pitch - delta.y, -settings.MaxPitch, settings.MaxPitch);
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private float GetSensitivity()
        {
            return input.LookIsPointerDelta
                ? settings.MouseSensitivity
                : settings.StickSensitivity * Time.deltaTime;
        }
    }
}
