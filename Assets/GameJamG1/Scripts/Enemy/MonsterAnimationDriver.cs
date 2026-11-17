using UnityEngine;
using UnityEngine.AI;

namespace GameJamG1.Enemy
{
    public class MonsterAnimationDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField, Min(0f)] private float speedDamping = 0.1f;

        private NavMeshAgent agent;

        private void Awake()
        {
            TryGetComponent(out agent);

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Update()
        {
            if (agent == null || animator == null)
            {
                return;
            }

            var speed = agent.enabled ? agent.velocity.magnitude : 0f;
            animator.SetFloat(MonsterAnimatorParameters.SpeedHash, speed, speedDamping, Time.deltaTime);
        }

        public void TriggerAttack()
        {
            animator.SetTrigger(MonsterAnimatorParameters.AttackHash);
        }

        public void TriggerScream()
        {
            animator.SetTrigger(MonsterAnimatorParameters.ScreamHash);
        }
    }
}
