using System.Collections.Generic;
using GameJamG1.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace GameJamG1.Objects
{
    public class ExclusiveChoiceGroup : MonoBehaviour
    {
        [Tooltip("Only one of these can be used. Using one disables the rest.")]
        [SerializeField] private List<InteractableBase> options = new();

        [SerializeField] private UnityEvent onChoiceMade;

        private void OnEnable()
        {
            foreach (var option in options)
            {
                if (option != null)
                {
                    option.Interacted += HandleInteracted;
                }
            }
        }

        private void OnDisable()
        {
            foreach (var option in options)
            {
                if (option != null)
                {
                    option.Interacted -= HandleInteracted;
                }
            }
        }

        private void HandleInteracted(InteractableBase chosen)
        {
            foreach (var option in options)
            {
                if (option != null && option != chosen)
                {
                    option.Deactivate();
                }
            }

            onChoiceMade.Invoke();
        }

        private void OnValidate()
        {
            if (options.Count < 2)
            {
                Debug.LogWarning($"{name}: an exclusive choice group needs at least two options.", this);
            }

            if (options.Contains(null))
            {
                Debug.LogWarning($"{name}: the options list contains an empty entry.", this);
            }
        }
    }
}
