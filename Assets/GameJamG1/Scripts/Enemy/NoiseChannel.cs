using System;
using UnityEngine;

namespace GameJamG1.Enemy
{
    [CreateAssetMenu(fileName = "NoiseChannel", menuName = "GameJamG1/Enemy/Noise Channel")]
    public class NoiseChannel : ScriptableObject
    {
        [Tooltip("Print every emission to the console. Useful while testing.")]
        [SerializeField] private bool logEmissions;

        public event Action<Vector3, float> Emitted;

        public void Emit(Vector3 position, float intensity)
        {
            if (logEmissions)
            {
                Debug.Log($"{name}: noise {intensity:0.##} at {position}", this);
            }

            Emitted?.Invoke(position, intensity);
        }
    }
}
