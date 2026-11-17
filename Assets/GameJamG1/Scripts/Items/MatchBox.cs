using System;
using UnityEngine;

namespace GameJamG1.Items
{
    public class MatchBox : MonoBehaviour
    {
        [SerializeField, Min(0)] private int startMatches = 1;

        public int Count { get; private set; }

        public event Action<int> CountChanged;

        private void Awake()
        {
            Count = startMatches;
        }

        public void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Count += amount;
            CountChanged?.Invoke(Count);
        }

        // Para volver al numero de cerillas del ultimo checkpoint al morir.
        public void SetCount(int count)
        {
            count = Mathf.Max(0, count);
            if (count == Count)
            {
                return;
            }

            Count = count;
            CountChanged?.Invoke(Count);
        }

        public bool TryConsume(int amount)
        {
            if (amount <= 0 || Count < amount)
            {
                return false;
            }

            Count -= amount;
            CountChanged?.Invoke(Count);
            return true;
        }
    }
}
