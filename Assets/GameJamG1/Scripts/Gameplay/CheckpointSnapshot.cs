using System.Collections.Generic;
using GameJamG1.Items;
using GameJamG1.Objects;
using UnityEngine;

namespace GameJamG1.Gameplay
{
    // Foto de los recursos del jugador al guardar un checkpoint. Al morir se repone:
    // cerillas (y las cajas recogidas despues vuelven a su sitio), pila de la linterna y vela,
    // y las puertas que el monstruo rompio vuelven a estar enteras y cerradas.
    public class CheckpointSnapshot
    {
        private readonly List<MatchPickup> availablePickups = new();
        private readonly Dictionary<ItemData, LightRuntimeState> lightStates = new();
        private int matches = -1;

        public void Capture(Transform player)
        {
            var box = player != null ? player.GetComponentInChildren<MatchBox>() : null;
            matches = box != null ? box.Count : -1;

            availablePickups.Clear();
            foreach (var pickup in Object.FindObjectsByType<MatchPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                // Incluye las escondidas en cajones (inactivas pero sin recoger).
                if (!pickup.IsConsumed)
                {
                    availablePickups.Add(pickup);
                }
            }

            lightStates.Clear();
            var holder = player != null ? player.GetComponentInChildren<ItemHolder>() : null;
            foreach (var item in LightItems())
            {
                var held = holder != null ? holder.FindHeldLight(item.Data) : null;
                lightStates[item.Data] = Copy(held != null ? held.Capture() : item.State);
            }
        }

        public void Restore(Transform player)
        {
            var box = player != null ? player.GetComponentInChildren<MatchBox>() : null;
            if (box != null && matches >= 0)
            {
                box.SetCount(matches);
            }

            foreach (var pickup in availablePickups)
            {
                // Solo las recogidas: una que sigue escondida en un cajon cerrado no debe aparecer.
                if (pickup != null && pickup.IsConsumed)
                {
                    pickup.ResetInteraction();
                }
            }

            var holder = player != null ? player.GetComponentInChildren<ItemHolder>() : null;
            foreach (var pair in lightStates)
            {
                var held = holder != null ? holder.FindHeldLight(pair.Key) : null;
                if (held != null)
                {
                    if (pair.Value.Initialized)
                    {
                        held.Restore(pair.Value);
                    }
                    else
                    {
                        held.ResetState();
                    }
                }

                foreach (var item in LightItems())
                {
                    if (item.Data == pair.Key)
                    {
                        item.State = Copy(pair.Value);
                    }
                }
            }

            foreach (var door in Object.FindObjectsByType<Door>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                door.Repair();
            }
        }

        // Objetos del mundo cuyo objeto en la mano es una luz (linterna, vela).
        private static IEnumerable<EquippableItem> LightItems()
        {
            foreach (var item in Object.FindObjectsByType<EquippableItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var data = item.Data;
                if (data != null && data.HeldPrefab != null && data.HeldPrefab.GetComponentInChildren<LightSource>(true) != null)
                {
                    yield return item;
                }
            }
        }

        private static LightRuntimeState Copy(LightRuntimeState state)
        {
            return state == null
                ? new LightRuntimeState()
                : new LightRuntimeState { Initialized = state.Initialized, Lit = state.Lit, Remaining = state.Remaining };
        }
    }
}
