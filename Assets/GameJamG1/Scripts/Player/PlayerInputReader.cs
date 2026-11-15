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
        private PlayerControlLock controlLock;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction crouchAction;
        private InputAction sprintAction;
        private InputAction interactAction;
        private InputAction toggleLightAction;
        private InputAction breatheAction;

        public bool IsLocked => controlLock.IsLocked;
        public Vector2 Move => IsLocked ? Vector2.zero : moveAction.ReadValue<Vector2>();
        public Vector2 Look => IsLocked ? Vector2.zero : lookAction.ReadValue<Vector2>();
        public bool LookIsPointerDelta => lookAction.activeControl?.device is Pointer;
        public bool SprintHeld => !IsLocked && sprintAction.IsPressed();
        public bool CrouchHeld => !IsLocked && crouchAction.IsPressed();
        public bool InteractPressed => !IsLocked && interactAction.WasPressedThisFrame();
        public bool ToggleLightPressed => !IsLocked && toggleLightAction.WasPressedThisFrame();
        public bool BreatheHeld => breatheAction.IsPressed();

        private void Awake()
        {
            if (!TryGetComponent(out controlLock))
            {
                controlLock = gameObject.AddComponent<PlayerControlLock>();
            }

            runtimeActions = Instantiate(actions);
            map = runtimeActions.FindActionMap(mapName, true);
            moveAction = map.FindAction("Move", true);
            lookAction = map.FindAction("Look", true);
            crouchAction = map.FindAction("Crouch", true);
            sprintAction = map.FindAction("Sprint", true);
            interactAction = map.FindAction("Interact", true);
            toggleLightAction = map.FindAction("ToggleLight", true);
            breatheAction = map.FindAction("Breathe", true);
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
