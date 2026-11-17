using System;
using System.Collections;
using GameJamG1.Player;
using UnityEngine;
using UnityEngine.Events;

namespace GameJamG1.Enemy
{
    [RequireComponent(typeof(MonsterAnimationDriver))]
    public class MonsterChaser : EnemyController
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private MonsterAnimationDriver animationDriver;

        [Header("Speeds")]
        [SerializeField, Min(0f)] private float patrolSpeed = 1.5f;
        [SerializeField, Min(0f)] private float investigateSpeed = 2.5f;
        [SerializeField, Min(0f)] private float chaseSpeed = 4f;

        [Header("Vision")]
        [SerializeField, Min(0f)] private float sightRange = 15f;
        [SerializeField, Range(1f, 360f)] private float sightAngle = 120f;
        [SerializeField, Min(0f)] private float eyeHeight = 1.6f;
        [SerializeField, Min(0f)] private float targetHeight = 1f;
        [SerializeField] private LayerMask obstacleMask = ~0;
        [SerializeField, Min(0f)] private float loseSightTime = 3f;

        [Header("Navigation")]
        [SerializeField, Min(0.05f)] private float arrivalThreshold = 0.6f;
        [SerializeField, Min(0f)] private float waitAtPoint = 2f;

        [Header("Scream")]
        [SerializeField, Min(0f)] private float screamDuration = 1.5f;

        [Header("Attack")]
        [SerializeField, Min(0.1f)] private float attackRange = 1.6f;
        [SerializeField, Min(0f)] private float attackHitDelay = 0.45f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 1.6f;
        [SerializeField, Min(0f)] private float hitTolerance = 0.75f;

        [Header("Events")]
        [SerializeField] private UnityEvent onPlayerSpotted;
        [SerializeField] private UnityEvent onAttackHit;

        private const float PlayerSearchInterval = 2f;
        private const float FaceTurnSpeed = 10f;

        private EnemyState previousState;
        private Vector3 investigateTarget;
        private bool hasInvestigateTarget;
        private Vector3 lastSeenPosition;
        private float lastSeenTime;
        private float waitUntil = -1f;
        private float nextAttackTime;
        private float nextPlayerSearch;
        private int waypointIndex;
        private bool busy;
        private Coroutine routine;

        public event Action AttackHit;

        private bool AgentReady => Agent.enabled && Agent.isOnNavMesh;

        private void Update()
        {
            if (State == EnemyState.Inactive || busy || !AgentReady)
            {
                return;
            }

            var sees = CanSeePlayer();

            if (sees)
            {
                lastSeenTime = Time.time;
                lastSeenPosition = player.position;
            }

            switch (State)
            {
                case EnemyState.Patrol:
                    if (sees)
                    {
                        SetState(EnemyState.Chase);
                        return;
                    }

                    UpdatePatrol();
                    break;
                case EnemyState.Investigate:
                    if (sees)
                    {
                        SetState(EnemyState.Chase);
                        return;
                    }

                    UpdateInvestigate();
                    break;
                case EnemyState.Chase:
                    UpdateChase(sees);
                    break;
            }
        }

        protected override void OnStateEnter(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Patrol:
                    Configure(patrolSpeed, arrivalThreshold * 0.5f);
                    GoToWaypoint();
                    break;
                case EnemyState.Investigate:
                    Configure(investigateSpeed, arrivalThreshold * 0.5f);
                    BeginInvestigation();
                    break;
                case EnemyState.Chase:
                    Configure(chaseSpeed, attackRange * 0.8f);
                    if (previousState != EnemyState.Chase)
                    {
                        StartRoutine(ScreamRoutine());
                    }

                    break;
                case EnemyState.Inactive:
                    busy = false;
                    break;
            }
        }

        protected override void OnStateExit(EnemyState state)
        {
            previousState = state;
            CancelRoutine();
        }

        protected override void OnNoiseHeard(Vector3 position, float intensity)
        {
            if (State == EnemyState.Inactive || State == EnemyState.Chase || busy)
            {
                return;
            }

            investigateTarget = position;
            hasInvestigateTarget = true;

            if (State == EnemyState.Investigate)
            {
                BeginInvestigation();
            }
            else
            {
                SetState(EnemyState.Investigate);
            }
        }

        private void UpdatePatrol()
        {
            if (Waypoints.Count == 0 || !HasArrived())
            {
                return;
            }

            if (waitUntil < 0f)
            {
                waitUntil = Time.time + waitAtPoint;
            }
            else if (Time.time >= waitUntil)
            {
                waypointIndex = (waypointIndex + 1) % Waypoints.Count;
                GoToWaypoint();
            }
        }

        private void UpdateInvestigate()
        {
            if (!HasArrived())
            {
                return;
            }

            if (waitUntil < 0f)
            {
                waitUntil = Time.time + waitAtPoint;
            }
            else if (Time.time >= waitUntil)
            {
                SetState(EnemyState.Patrol);
            }
        }

        private void UpdateChase(bool sees)
        {
            if (sees)
            {
                if (FlatDistanceTo(player.position) <= attackRange && Time.time >= nextAttackTime)
                {
                    StartRoutine(AttackRoutine());
                    return;
                }

                MoveTo(player.position);
                return;
            }

            if (Time.time - lastSeenTime > loseSightTime)
            {
                investigateTarget = lastSeenPosition;
                hasInvestigateTarget = true;
                SetState(EnemyState.Investigate);
                return;
            }

            MoveTo(lastSeenPosition);
        }

        private IEnumerator ScreamRoutine()
        {
            busy = true;
            StopAgent(true);
            animationDriver.TriggerScream();
            onPlayerSpotted.Invoke();

            var end = Time.time + screamDuration;
            while (Time.time < end)
            {
                FacePlayer();
                yield return null;
            }

            EndRoutine();
        }

        private IEnumerator AttackRoutine()
        {
            busy = true;
            nextAttackTime = Time.time + attackCooldown;
            StopAgent(true);
            animationDriver.TriggerAttack();

            var hitTime = Time.time + attackHitDelay;
            while (Time.time < hitTime)
            {
                FacePlayer();
                yield return null;
            }

            if (FlatDistanceTo(player.position) <= attackRange + hitTolerance)
            {
                AttackHit?.Invoke();
                onAttackHit.Invoke();
            }

            while (Time.time < nextAttackTime)
            {
                FacePlayer();
                yield return null;
            }

            EndRoutine();
        }

        private void StartRoutine(IEnumerator enumerator)
        {
            CancelRoutine();
            routine = StartCoroutine(enumerator);
        }

        private void CancelRoutine()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            busy = false;
            StopAgent(false);
        }

        private void EndRoutine()
        {
            routine = null;
            busy = false;
            StopAgent(false);
        }

        private void Configure(float speed, float stoppingDistance)
        {
            Agent.speed = speed;
            Agent.stoppingDistance = stoppingDistance;
        }

        private void BeginInvestigation()
        {
            if (!hasInvestigateTarget)
            {
                investigateTarget = transform.position;
            }

            waitUntil = -1f;
            MoveTo(investigateTarget);
        }

        private void GoToWaypoint()
        {
            waitUntil = -1f;

            if (Waypoints.Count > 0)
            {
                MoveTo(Waypoints[waypointIndex].position);
            }
        }

        private void MoveTo(Vector3 position)
        {
            if (AgentReady)
            {
                Agent.SetDestination(position);
            }
        }

        private void StopAgent(bool stop)
        {
            if (!AgentReady)
            {
                return;
            }

            Agent.isStopped = stop;

            if (stop)
            {
                Agent.velocity = Vector3.zero;
            }
        }

        private bool HasArrived()
        {
            return !Agent.pathPending && Agent.remainingDistance <= arrivalThreshold;
        }

        private bool CanSeePlayer()
        {
            if (!EnsurePlayer())
            {
                return false;
            }

            var eye = transform.position + Vector3.up * eyeHeight;
            var target = player.position + Vector3.up * targetHeight;
            var toTarget = target - eye;

            if (toTarget.sqrMagnitude > sightRange * sightRange)
            {
                return false;
            }

            if (State != EnemyState.Chase)
            {
                var flat = Vector3.ProjectOnPlane(toTarget, Vector3.up);
                if (Vector3.Angle(transform.forward, flat) > sightAngle * 0.5f)
                {
                    return false;
                }
            }

            if (Physics.Linecast(eye, target, out var hit, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                return hit.transform.IsChildOf(player);
            }

            return true;
        }

        private bool EnsurePlayer()
        {
            if (player != null)
            {
                return true;
            }

            if (Time.time < nextPlayerSearch)
            {
                return false;
            }

            nextPlayerSearch = Time.time + PlayerSearchInterval;
            var motor = FindFirstObjectByType<FirstPersonMotor>();

            if (motor != null)
            {
                player = motor.transform;
            }

            return player != null;
        }

        private float FlatDistanceTo(Vector3 position)
        {
            var delta = position - transform.position;
            delta.y = 0f;
            return delta.magnitude;
        }

        private void FacePlayer()
        {
            var direction = player.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            var target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, FaceTurnSpeed * Time.deltaTime);
        }

        private void OnValidate()
        {
            attackCooldown = Mathf.Max(attackCooldown, attackHitDelay);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
