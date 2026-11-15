using GameJamG1.Interaction;
using UnityEngine;

namespace GameJamG1.Items
{
    public class MatchPickup : InteractableBase
    {
        [Header("Matches")]
        [SerializeField, Min(1)] private int amount = 1;

        protected override bool OnInteract(PlayerInteractor interactor)
        {
            if (!interactor.TryGetComponent(out MatchBox matchBox))
            {
                return false;
            }

            matchBox.Add(amount);
            return true;
        }
    }
}
