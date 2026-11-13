using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJamG1.Utils
{
    public class CursorLockController : MonoBehaviour
    {
        [SerializeField] private bool lockOnStart = true;

        private void Start()
        {
            if (lockOnStart)
            {
                Lock();
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Unlock();
                return;
            }

            var clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            if (clicked && Cursor.lockState != CursorLockMode.Locked)
            {
                Lock();
            }
        }

        public void Lock()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Unlock()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
