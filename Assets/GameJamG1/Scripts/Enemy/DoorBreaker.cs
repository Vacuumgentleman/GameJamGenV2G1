using GameJamG1.Objects;
using UnityEngine;

namespace GameJamG1.Enemy
{
    // El NavMesh atraviesa las puertas, asi que el monstruo pasaria a traves de ellas como un fantasma.
    // Este componente revienta las puertas cerradas que tiene delante mientras esta activo.
    [RequireComponent(typeof(EnemyController))]
    public class DoorBreaker : MonoBehaviour
    {
        [Tooltip("Distance in front of the monster where doors are checked.")]
        [SerializeField, Min(0f)] private float reach = 0.9f;
        [SerializeField, Min(0.1f)] private float radius = 0.9f;
        [SerializeField, Min(0f)] private float checkHeight = 1f;
        [SerializeField, Min(0.02f)] private float checkInterval = 0.1f;
        [Tooltip("Only break doors while chasing. Off: also while patrolling or investigating.")]
        [SerializeField] private bool onlyWhileChasing;

        private readonly Collider[] hits = new Collider[16];
        private EnemyController enemy;
        private float nextCheck;

        private void Awake()
        {
            enemy = GetComponent<EnemyController>();
        }

        private void Update()
        {
            if (Time.time < nextCheck || enemy.State == EnemyState.Inactive)
            {
                return;
            }

            if (onlyWhileChasing && enemy.State != EnemyState.Chase)
            {
                return;
            }

            nextCheck = Time.time + checkInterval;

            var center = transform.position + Vector3.up * checkHeight + transform.forward * reach;
            var count = Physics.OverlapSphereNonAlloc(center, radius, hits, ~0, QueryTriggerInteraction.Ignore);

            for (var i = 0; i < count; i++)
            {
                var door = hits[i].GetComponentInParent<Door>();
                if (door != null && !door.IsBroken && door.State != DoorState.Barricaded)
                {
                    door.Break(transform.position);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * checkHeight + transform.forward * reach, radius);
        }
    }
}
