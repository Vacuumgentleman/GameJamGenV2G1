using System;
using GameJamG1.Dialogue;
using GameJamG1.Player;
using UnityEngine;

namespace GameJamG1.Items
{
    [RequireComponent(typeof(PlayerInputReader))]
    public class ItemHolder : MonoBehaviour
    {
        [Header("Anchors")]
        [Tooltip("Hand position for light items.")]
        [SerializeField] private Transform handAnchorLight;
        [Tooltip("Hand position for carried items.")]
        [SerializeField] private Transform handAnchorCarry;

        private PlayerInputReader input;
        private GameObject heldInstance;
        private LightSource lightSource;
        private EquippableItem heldWorldItem;

        public ItemData CurrentItem { get; private set; }

        public ItemData CurrentLight => CurrentItem != null && CurrentItem.Slot == ItemSlot.Light ? CurrentItem : null;

        public ItemData CurrentCarry => CurrentItem != null && CurrentItem.Slot == ItemSlot.Carry ? CurrentItem : null;

        public LightSource CurrentLightSource => lightSource;

        public event Action<ItemSlot, ItemData> SlotChanged;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            if (lightSource != null && input.ToggleLightPressed)
            {
                lightSource.Toggle();
            }
        }

        public bool TryEquip(ItemData data, EquippableItem source = null)
        {
            if (data == null || data.HeldPrefab == null)
            {
                return false;
            }

            DropCurrent(source);
            Hold(data, source);
            NotifyPickup(data);
            return true;
        }

        public void Clear(ItemSlot slot)
        {
            if (CurrentItem != null && CurrentItem.Slot == slot)
            {
                Release(true);
            }
        }

        public void ExtinguishLight()
        {
            if (lightSource != null)
            {
                lightSource.Extinguish();
            }
        }

        private void Hold(ItemData data, EquippableItem source)
        {
            var anchor = data.Slot == ItemSlot.Light ? handAnchorLight : handAnchorCarry;

            heldInstance = Instantiate(data.HeldPrefab, anchor);
            heldInstance.transform.SetLocalPositionAndRotation(data.HandLocalPosition, data.HandLocalRotation);

            lightSource = heldInstance.GetComponent<LightSource>();
            if (lightSource != null && source != null)
            {
                lightSource.Restore(source.State);
            }

            heldWorldItem = source;
            CurrentItem = data;
            SlotChanged?.Invoke(data.Slot, data);
        }

        private void DropCurrent(EquippableItem replacing)
        {
            if (CurrentItem == null)
            {
                return;
            }

            var state = lightSource != null ? lightSource.Capture() : new LightRuntimeState();
            var position = replacing != null ? replacing.transform.position : transform.position + transform.forward;
            var rotation = replacing != null ? replacing.transform.rotation : transform.rotation;

            var world = heldWorldItem;
            if (world == null)
            {
                world = SpawnWorldItem(CurrentItem, position, rotation);
            }
            else
            {
                world.transform.SetPositionAndRotation(position, rotation);
            }

            if (world != null)
            {
                world.State = state;
                world.gameObject.SetActive(true);
            }

            Release(false);
        }

        private void Release(bool destroyWorldItem)
        {
            if (heldInstance != null)
            {
                Destroy(heldInstance);
            }

            if (destroyWorldItem && heldWorldItem != null)
            {
                Destroy(heldWorldItem.gameObject);
            }

            var slot = CurrentItem != null ? CurrentItem.Slot : ItemSlot.Carry;

            heldInstance = null;
            lightSource = null;
            heldWorldItem = null;
            CurrentItem = null;
            SlotChanged?.Invoke(slot, null);
        }

        private static EquippableItem SpawnWorldItem(ItemData data, Vector3 position, Quaternion rotation)
        {
            if (data == null || data.WorldPrefab == null)
            {
                return null;
            }

            var instance = Instantiate(data.WorldPrefab, position, rotation);
            return instance.GetComponent<EquippableItem>();
        }

        private void NotifyPickup(ItemData data)
        {
            if (data.PickupSound != null)
            {
                AudioSource.PlayClipAtPoint(data.PickupSound, transform.position);
            }

            if (data.PickupDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.Play(data.PickupDialogue, false);
            }
        }

        private void OnValidate()
        {
            if (handAnchorLight == null || handAnchorCarry == null)
            {
                Debug.LogWarning($"{name}: ItemHolder is missing a hand anchor.", this);
            }
        }
    }
}
