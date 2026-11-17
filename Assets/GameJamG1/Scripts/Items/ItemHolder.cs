using System;
using GameJamG1.Dialogue;
using GameJamG1.Player;
using UnityEngine;

namespace GameJamG1.Items
{
    // Dos manos: la principal (vela u objeto cargado) y la secundaria (linterna).
    // La linterna convive con un objeto Carry; la vela (slot Light) es exclusiva y suelta todo lo demas.
    [RequireComponent(typeof(PlayerInputReader))]
    public class ItemHolder : MonoBehaviour
    {
        [Header("Anchors")]
        [Tooltip("Hand position for light items (candle and flashlight).")]
        [SerializeField] private Transform handAnchorLight;
        [Tooltip("Hand position for carried items.")]
        [SerializeField] private Transform handAnchorCarry;

        [Header("Drop")]
        [Tooltip("Sideways offset between items dropped at the same time.")]
        [SerializeField, Min(0f)] private float dropSpacing = 0.35f;

        private sealed class Held
        {
            public ItemData Data;
            public GameObject Instance;
            public LightSource Light;
            public EquippableItem World;
        }

        private PlayerInputReader input;
        private Held main;
        private Held offHand;

        public ItemData CurrentItem => main?.Data ?? offHand?.Data;

        public ItemData CurrentLight => offHand?.Data ?? (main != null && main.Data.Slot == ItemSlot.Light ? main.Data : null);

        public ItemData CurrentCarry => main != null && main.Data.Slot == ItemSlot.Carry ? main.Data : null;

        public ItemData CurrentOffHand => offHand?.Data;

        public LightSource CurrentLightSource => offHand?.Light != null ? offHand.Light : main?.Light;

        public event Action<ItemSlot, ItemData> SlotChanged;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            var light = CurrentLightSource;
            if (light != null && input.ToggleLightPressed)
            {
                light.Toggle();
            }
        }

        public bool TryEquip(ItemData data, EquippableItem source = null)
        {
            if (data == null || data.HeldPrefab == null)
            {
                return false;
            }

            var dropIndex = 0;

            switch (data.Slot)
            {
                case ItemSlot.OffHand:
                    Drop(ref offHand, source, ref dropIndex);
                    if (main != null && main.Data.Slot == ItemSlot.Light)
                    {
                        Drop(ref main, source, ref dropIndex);
                    }

                    offHand = Hold(data, source, handAnchorLight);
                    break;

                case ItemSlot.Light:
                    Drop(ref main, source, ref dropIndex);
                    Drop(ref offHand, source, ref dropIndex);
                    main = Hold(data, source, handAnchorLight);
                    break;

                default:
                    Drop(ref main, source, ref dropIndex);
                    main = Hold(data, source, handAnchorCarry);
                    break;
            }

            NotifyPickup(data);
            return true;
        }

        // Luz en la mano que corresponde a ese objeto, o null si no se lleva.
        public LightSource FindHeldLight(ItemData data)
        {
            if (data == null)
            {
                return null;
            }

            if (main != null && main.Data == data)
            {
                return main.Light;
            }

            return offHand != null && offHand.Data == data ? offHand.Light : null;
        }

        public void Clear(ItemSlot slot)
        {
            if (main != null && main.Data.Slot == slot)
            {
                Release(ref main, true);
            }

            if (offHand != null && offHand.Data.Slot == slot)
            {
                Release(ref offHand, true);
            }
        }

        public void ExtinguishLight()
        {
            if (offHand?.Light != null)
            {
                offHand.Light.Extinguish();
            }

            if (main?.Light != null)
            {
                main.Light.Extinguish();
            }
        }

        private Held Hold(ItemData data, EquippableItem source, Transform anchor)
        {
            var instance = Instantiate(data.HeldPrefab, anchor);
            instance.transform.SetLocalPositionAndRotation(data.HandLocalPosition, data.HandLocalRotation);

            var held = new Held
            {
                Data = data,
                Instance = instance,
                Light = instance.GetComponent<LightSource>(),
                World = source
            };

            if (held.Light != null && source != null)
            {
                held.Light.Restore(source.State);
            }

            SlotChanged?.Invoke(data.Slot, data);
            return held;
        }

        private void Drop(ref Held held, EquippableItem replacing, ref int dropIndex)
        {
            if (held == null)
            {
                return;
            }

            var state = held.Light != null ? held.Light.Capture() : new LightRuntimeState();
            var position = replacing != null ? replacing.transform.position : transform.position + transform.forward;
            var rotation = replacing != null ? replacing.transform.rotation : transform.rotation;
            position += transform.right * (dropSpacing * dropIndex);
            dropIndex++;

            var world = held.World;
            if (world == null)
            {
                world = SpawnWorldItem(held.Data, position, rotation);
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

            Release(ref held, false);
        }

        private void Release(ref Held held, bool destroyWorldItem)
        {
            if (held == null)
            {
                return;
            }

            if (held.Instance != null)
            {
                Destroy(held.Instance);
            }

            if (destroyWorldItem && held.World != null)
            {
                Destroy(held.World.gameObject);
            }

            var slot = held.Data.Slot;
            held = null;
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
