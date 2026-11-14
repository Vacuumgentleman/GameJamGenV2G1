using UnityEngine;

namespace GameJamG1.Enemy
{
    public class NoiseEmitter : MonoBehaviour
    {
        [SerializeField] private NoiseChannel channel;
        [SerializeField, Min(0f)] private float intensity = 1f;

        public void Emit()
        {
            EmitWithIntensity(intensity);
        }

        public void EmitWithIntensity(float value)
        {
            if (channel != null)
            {
                channel.Emit(transform.position, value);
            }
        }

        private void OnValidate()
        {
            if (channel == null)
            {
                Debug.LogWarning($"{name}: NoiseEmitter has no channel assigned.", this);
            }
        }
    }
}
