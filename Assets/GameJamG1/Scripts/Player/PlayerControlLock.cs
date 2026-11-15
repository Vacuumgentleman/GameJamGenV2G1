using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameJamG1.Player
{
    public class PlayerControlLock : MonoBehaviour
    {
        private readonly HashSet<object> owners = new();

        public bool IsLocked => owners.Count > 0;

        public event Action<bool> LockChanged;

        public void Lock(object owner)
        {
            if (owners.Add(owner) && owners.Count == 1)
            {
                LockChanged?.Invoke(true);
            }
        }

        public void Unlock(object owner)
        {
            if (owners.Remove(owner) && owners.Count == 0)
            {
                LockChanged?.Invoke(false);
            }
        }
    }
}
