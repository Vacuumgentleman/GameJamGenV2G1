using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJamG1.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;
        [SerializeField] private string mapName = "Player";

        private InputActionAsset runtimeActions;
        private InputActionMap map;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction sprintAction;

        public Vector2 Move => moveAction.ReadValue<Vector2>();
        public Vector2 Look => lookAction.ReadValue<Vector2>();
        public bool LookIsPointerDelta => lookAction.activeControl?.device is Pointer;
        public bool SprintHeld => sprintAction.IsPressed();
        public bool JumpPressed => jumpAction.WasPressedThisFrame();

        private void Awake()
        {
            runtimeActions = Instantiate(actions);
            map = runtimeActions.FindActionMap(mapName, true);
            moveAction = map.FindAction("Move", true);
            lookAction = map.FindAction("Look", true);
            jumpAction = map.FindAction("Jump", true);
            sprintAction = map.FindAction("Sprint", true);
        }

        private void OnEnable()
        {
            map.Enable();
        }

        private void OnDisable()
        {
            map.Disable();
        }

        private void OnDestroy()
        {
            if (runtimeActions != null)
            {
                Destroy(runtimeActions);
            }
        }
    }
}
