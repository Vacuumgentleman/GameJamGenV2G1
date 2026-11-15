using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GameJamG1.Enemy
{
    public enum EnemyState
    {
        Inactive,
        Patrol,
        Investigate,
        Chase
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyState startState = EnemyState.Inactive;
        [SerializeField] private List<Transform> waypoints = new();
        [Tooltip("Optional. Noises on this channel are reported to OnNoiseHeard.")]
        [SerializeField] private NoiseChannel noiseChannel;

        public EnemyState State { get; private set; }

        public NavMeshAgent Agent { get; private set; }

        public IReadOnlyList<Transform> Waypoints => waypoints;

        public Vector3 LastNoisePosition { get; private set; }

        public event Action<EnemyState> StateChanged;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            if (noiseChannel != null)
            {
                noiseChannel.Emitted += HandleNoise;
            }
        }

        private void OnDisable()
        {
            if (noiseChannel != null)
            {
                noiseChannel.Emitted -= HandleNoise;
            }
        }

        private void Start()
        {
            Enter(startState);
        }

        public void SetActive(bool active)
        {
            SetState(active ? EnemyState.Patrol : EnemyState.Inactive);
        }

        public void SetState(EnemyState next)
        {
            if (next == State)
            {
                return;
            }

            OnStateExit(State);
            Enter(next);
        }

        protected virtual void OnStateEnter(EnemyState state)
        {
        }

        protected virtual void OnStateExit(EnemyState state)
        {
        }

        protected virtual void OnNoiseHeard(Vector3 position, float intensity)
        {
        }

        private void Enter(EnemyState next)
        {
            State = next;
            Agent.enabled = next != EnemyState.Inactive;
            OnStateEnter(next);
            StateChanged?.Invoke(next);
        }

        private void HandleNoise(Vector3 position, float intensity)
        {
            LastNoisePosition = position;
            OnNoiseHeard(position, intensity);
        }
    }
}
