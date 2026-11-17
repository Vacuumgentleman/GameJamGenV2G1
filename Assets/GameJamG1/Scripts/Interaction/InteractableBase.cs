using System;
using System.Collections.Generic;
using GameJamG1.Dialogue;
using GameJamG1.Story;
using UnityEngine;
using UnityEngine.Events;

namespace GameJamG1.Interaction
{
    public class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Prompt")]
        [Tooltip("Text shown under the crosshair while this object is targeted.")]
        [SerializeField] private string promptText = "Interactuar";

        [Header("Conditions")]
        [Tooltip("The object stays locked until every flag in this list is active.")]
        [SerializeField] private List<StoryFlag> requiredFlags = new();
        [Tooltip("Monologue played when the player uses the object while locked. Empty uses the default locked dialogue.")]
        [SerializeField] private DialogueSequence lockedDialogue;

        [Header("Result")]
        [Tooltip("Monologue played when the player uses the object.")]
        [SerializeField] private DialogueSequence interactDialogue;
        [SerializeField] private List<StoryFlag> flagsToSetOnInteract = new();
        [Tooltip("Disable the object after it has been used once.")]
        [SerializeField] private bool oneShot;
        [Tooltip("Hide the whole object when it is consumed.")]
        [SerializeField] private bool hideWhenConsumed;
        [SerializeField] private AudioClip interactSound;

        [Header("Events")]
        [SerializeField] private UnityEvent onInteract;
        [SerializeField] private UnityEvent onLockedInteract;

        private OutlineHighlight highlight;
        private bool consumed;

        protected string PromptText => promptText;
        protected bool Consumed => consumed;

        public event Action<InteractableBase> Interacted;

        public UnityEvent InteractEvent => onInteract;

        public virtual bool IsLocked => !AreRequiredFlagsActive();

        public bool IsConsumed => consumed;

        public virtual string GetPrompt()
        {
            return promptText;
        }

        public virtual bool CanInteract()
        {
            return isActiveAndEnabled && !consumed;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!CanInteract() || IsLocked)
            {
                return;
            }

            if (!OnInteract(interactor))
            {
                return;
            }

            ApplyInteractionResult();
        }

        public virtual void InteractLocked(PlayerInteractor interactor)
        {
            var dialogue = ResolveLockedDialogue();
            var manager = DialogueManager.Instance;

            if (dialogue != null && manager != null)
            {
                manager.Play(dialogue, false);
            }

            onLockedInteract.Invoke();
        }

        public void SetHighlight(bool highlighted)
        {
            if (highlight != null)
            {
                highlight.SetHighlight(highlighted);
            }
        }

        protected virtual void Awake()
        {
            TryGetComponent(out highlight);
        }

        protected virtual void OnDisable()
        {
            SetHighlight(false);
        }

        public void Deactivate()
        {
            Consume();
        }

        // Devuelve el objeto a su estado sin usar (para repetir una escena tras morir).
        public virtual void ResetInteraction()
        {
            consumed = false;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        protected virtual bool OnInteract(PlayerInteractor interactor)
        {
            return true;
        }

        protected virtual DialogueSequence ResolveLockedDialogue()
        {
            if (lockedDialogue != null)
            {
                return lockedDialogue;
            }

            var manager = DialogueManager.Instance;
            return manager != null ? manager.DefaultLockedDialogue : null;
        }

        protected void Consume()
        {
            consumed = true;
            SetHighlight(false);

            if (hideWhenConsumed)
            {
                gameObject.SetActive(false);
            }
        }

        protected bool AreRequiredFlagsActive()
        {
            if (requiredFlags.Count == 0)
            {
                return true;
            }

            var manager = StoryManager.Instance;
            return manager != null && manager.HasAll(requiredFlags);
        }

        private void ApplyInteractionResult()
        {
            var manager = StoryManager.Instance;
            if (manager != null)
            {
                manager.SetMany(flagsToSetOnInteract);
            }

            if (interactSound != null)
            {
                AudioSource.PlayClipAtPoint(interactSound, transform.position);
            }

            if (interactDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.Play(interactDialogue, false);
            }

            onInteract.Invoke();

            if (oneShot)
            {
                Consume();
            }

            Interacted?.Invoke(this);
        }

        protected virtual void OnValidate()
        {
            if (GetComponentInChildren<Collider>() == null)
            {
                Debug.LogWarning($"{name}: interactables need a collider on this object or a child.", this);
            }

            if (requiredFlags.Contains(null) || flagsToSetOnInteract.Contains(null))
            {
                Debug.LogWarning($"{name}: a flag list contains an empty entry.", this);
            }
        }
    }
}
