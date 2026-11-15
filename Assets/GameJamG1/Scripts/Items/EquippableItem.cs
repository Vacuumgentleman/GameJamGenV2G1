using GameJamG1.Interaction;
using UnityEngine;

namespace GameJamG1.Items
{
    public class EquippableItem : InteractableBase
    {
        [Header("Item")]
        [SerializeField] private ItemData data;
        [Tooltip("{0} is the item display name.")]
        [SerializeField] private string promptFormat = "Tomar {0}";
        [SerializeField] private LightRuntimeState state = new();

        public ItemData Data => data;

        public LightRuntimeState State
        {
            get => state;
            set => state = value ?? new LightRuntimeState();
        }

        public override string GetPrompt()
        {
            return data != null ? string.Format(promptFormat, data.DisplayName) : base.GetPrompt();
        }

        protected override bool OnInteract(PlayerInteractor interactor)
        {
            if (!interactor.TryGetComponent(out ItemHolder holder) || !holder.TryEquip(data, this))
            {
                return false;
            }

            gameObject.SetActive(false);
            return true;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (data == null)
            {
                Debug.LogWarning($"{name}: EquippableItem has no item data assigned.", this);
            }
        }
    }
}
