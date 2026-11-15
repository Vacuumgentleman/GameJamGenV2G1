using GameJamG1.Dialogue;
using UnityEngine;

namespace GameJamG1.Items
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "GameJamG1/Items/Item")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private ItemSlot slot;

        [Header("Prefabs")]
        [Tooltip("Object shown in the player's hand.")]
        [SerializeField] private GameObject heldPrefab;
        [Tooltip("Optional. Used to drop the item back in the world when it has no world object.")]
        [SerializeField] private GameObject worldPrefab;

        [Header("Hand")]
        [SerializeField] private Vector3 handLocalPosition;
        [SerializeField] private Vector3 handLocalEulerAngles;

        [Header("Feedback")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private DialogueSequence pickupDialogue;

        public string Id => id;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public ItemSlot Slot => slot;
        public GameObject HeldPrefab => heldPrefab;
        public GameObject WorldPrefab => worldPrefab;
        public Vector3 HandLocalPosition => handLocalPosition;
        public Quaternion HandLocalRotation => Quaternion.Euler(handLocalEulerAngles);
        public AudioClip PickupSound => pickupSound;
        public DialogueSequence PickupDialogue => pickupDialogue;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"{name}: item has no id.", this);
            }

            if (heldPrefab == null)
            {
                Debug.LogWarning($"{name}: item has no held prefab.", this);
            }
        }
    }
}
