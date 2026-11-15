using GameJamG1.Interaction;
using GameJamG1.Items;
using UnityEngine;

namespace GameJamG1.Barricade
{
    public class BreakableFurniture : InteractableBase
    {
        [Header("Planks")]
        [Tooltip("World plank spawned when the furniture breaks.")]
        [SerializeField] private EquippableItem plankPrefab;
        [SerializeField, Min(1)] private int plankCount = 2;
        [Tooltip("Spawn point relative to this object.")]
        [SerializeField] private Vector3 spawnOffset = new(0f, 0.3f, 0f);
        [SerializeField, Min(0f)] private float spreadRadius = 0.5f;

        protected override bool OnInteract(PlayerInteractor interactor)
        {
            var origin = transform.TransformPoint(spawnOffset);

            for (var i = 0; i < plankCount; i++)
            {
                var offset = Random.insideUnitCircle * spreadRadius;
                var position = origin + new Vector3(offset.x, 0f, offset.y);
                Instantiate(plankPrefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            }

            return true;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (plankPrefab == null)
            {
                Debug.LogWarning($"{name}: BreakableFurniture has no plank prefab assigned.", this);
            }
        }
    }
}
