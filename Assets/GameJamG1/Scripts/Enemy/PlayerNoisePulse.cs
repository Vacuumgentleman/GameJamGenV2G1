using UnityEngine;

namespace GameJamG1.Enemy
{
    public class PlayerNoisePulse : MonoBehaviour
    {
        [Tooltip("Channel the enemies listen to.")]
        [SerializeField] private NoiseChannel channel;
        [Tooltip("Optional. Found by tag when empty.")]
        [SerializeField] private Transform player;
        [SerializeField] private string playerTag = "Player";
        [Tooltip("Seconds between noises emitted at the player position while this object is active.")]
        [SerializeField, Min(0.2f)] private float interval = 3f;
        [SerializeField, Min(0f)] private float intensity = 1f;

        private float timer;

        private void OnEnable()
        {
            timer = 0f;
        }

        private void Update()
        {
            if (channel == null)
            {
                return;
            }

            if (player == null)
            {
                var found = GameObject.FindGameObjectWithTag(playerTag);
                if (found == null)
                {
                    return;
                }

                player = found.transform;
            }

            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                timer = interval;
                channel.Emit(player.position, intensity);
            }
        }
    }
}
