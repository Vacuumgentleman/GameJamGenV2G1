using System.Collections.Generic;
using GameJamG1.Dialogue;
using UnityEngine;
using UnityEngine.Events;

namespace GameJamG1.Story
{
    public class StoryTrigger : MonoBehaviour
    {
        public enum ConditionMode
        {
            All,
            Any
        }

        [Header("Conditions")]
        [Tooltip("Flags evaluated when the trigger is activated. Leave empty to always pass.")]
        [SerializeField] private List<StoryFlag> requiredFlags = new();
        [Tooltip("All: every flag must be active. Any: at least one flag must be active.")]
        [SerializeField] private ConditionMode mode = ConditionMode.All;

        [Header("Activation")]
        [Tooltip("Fire when the player enters this object's trigger collider.")]
        [SerializeField] private bool triggerOnZoneEnter;
        [SerializeField] private string playerTag = "Player";
        [Tooltip("Evaluate the conditions again whenever one of the required flags changes.")]
        [SerializeField] private bool triggerOnFlagChanged;
        [SerializeField] private bool fireOnce = true;

        [Header("Result")]
        [Tooltip("Monologue played when the trigger fires.")]
        [SerializeField] private DialogueSequence dialogueOnTrigger;

        [Header("Events")]
        [SerializeField] private UnityEvent onTriggered;

        private StoryManager subscribedManager;
        private bool fired;

        public UnityEvent OnTriggered => onTriggered;

        private void Start()
        {
            if (!triggerOnFlagChanged || StoryManager.Instance == null)
            {
                return;
            }

            subscribedManager = StoryManager.Instance;
            subscribedManager.OnFlagChanged += HandleFlagChanged;
        }

        private void OnDestroy()
        {
            if (subscribedManager != null)
            {
                subscribedManager.OnFlagChanged -= HandleFlagChanged;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnZoneEnter && other.CompareTag(playerTag))
            {
                Trigger();
            }
        }

        public void Trigger()
        {
            if (!isActiveAndEnabled || (fireOnce && fired) || !ConditionsMet())
            {
                return;
            }

            fired = true;

            if (dialogueOnTrigger != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.Play(dialogueOnTrigger);
            }

            onTriggered.Invoke();
        }

        private void HandleFlagChanged(StoryFlag changed)
        {
            if (requiredFlags.Contains(changed))
            {
                Trigger();
            }
        }

        private bool ConditionsMet()
        {
            if (requiredFlags.Count == 0)
            {
                return true;
            }

            var manager = StoryManager.Instance;
            if (manager == null)
            {
                return false;
            }

            return mode == ConditionMode.All ? manager.HasAll(requiredFlags) : manager.HasAny(requiredFlags);
        }

        private void OnValidate()
        {
            if (triggerOnZoneEnter && TryGetComponent(out Collider zone) && !zone.isTrigger)
            {
                Debug.LogWarning($"{name}: zone activation needs a collider with Is Trigger enabled.", this);
            }

            if (triggerOnZoneEnter && !TryGetComponent<Collider>(out _))
            {
                Debug.LogWarning($"{name}: zone activation needs a trigger collider on this object.", this);
            }

            if (triggerOnFlagChanged && requiredFlags.Count == 0)
            {
                Debug.LogWarning($"{name}: 'Trigger On Flag Changed' has no required flags to watch.", this);
            }
        }
    }
}
