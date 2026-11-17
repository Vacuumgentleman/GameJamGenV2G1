using System;
using System.Collections.Generic;
using GameJamG1.Interaction;
using GameJamG1.Story;
using UnityEngine;
using UnityEngine.Events;

namespace GameJamG1.Gameplay
{
    public class ElectronicsController : MonoBehaviour
    {
        [Serializable]
        public class Device
        {
            [SerializeField] private string label;
            [Tooltip("Object the player uses to switch the device off.")]
            [SerializeField] private InteractableBase interactable;
            [Tooltip("Sounds that play while the device is on. Assign the clips on each AudioSource.")]
            [SerializeField] private List<AudioSource> audioSources = new();
            [Tooltip("Objects only visible while the device is on, such as screen glow.")]
            [SerializeField] private List<GameObject> activeWhileOn = new();

            public InteractableBase Interactable => interactable;
            public bool IsOn { get; private set; }

            public void SetOn(bool on)
            {
                IsOn = on;

                foreach (var source in audioSources)
                {
                    if (source == null)
                    {
                        continue;
                    }

                    if (on)
                    {
                        source.Play();
                    }
                    else
                    {
                        source.Stop();
                    }
                }

                foreach (var target in activeWhileOn)
                {
                    if (target != null)
                    {
                        target.SetActive(on);
                    }
                }
            }
        }

        [Tooltip("Devices switch on when every flag here is active.")]
        [SerializeField] private List<StoryFlag> turnOnFlags = new();
        [Tooltip("Flags activated when the player has switched off every device.")]
        [SerializeField] private List<StoryFlag> flagsWhenAllOff = new();
        [SerializeField] private List<Device> devices = new();
        [SerializeField] private UnityEvent onTurnedOn = new();
        [SerializeField] private UnityEvent onAllOff = new();

        private StoryManager story;
        private bool started;
        private bool finished;

        private void Start()
        {
            foreach (var device in devices)
            {
                device.SetOn(false);

                if (device.Interactable != null)
                {
                    device.Interactable.Interacted += HandleInteracted;
                }
            }

            story = StoryManager.Instance;
            if (story != null)
            {
                story.OnFlagChanged += HandleFlagChanged;
            }

            TryTurnOn();
        }

        private void OnDestroy()
        {
            if (story != null)
            {
                story.OnFlagChanged -= HandleFlagChanged;
            }

            foreach (var device in devices)
            {
                if (device.Interactable != null)
                {
                    device.Interactable.Interacted -= HandleInteracted;
                }
            }
        }

        private void HandleFlagChanged(StoryFlag changed)
        {
            if (turnOnFlags.Contains(changed))
            {
                TryTurnOn();
            }
        }

        private void TryTurnOn()
        {
            if (started || story == null || turnOnFlags.Count == 0 || !story.HasAll(turnOnFlags))
            {
                return;
            }

            started = true;

            foreach (var device in devices)
            {
                if (device.Interactable != null)
                {
                    device.SetOn(true);
                }
            }

            onTurnedOn.Invoke();
            CheckAllOff();
        }

        private void HandleInteracted(InteractableBase used)
        {
            if (!started)
            {
                return;
            }

            foreach (var device in devices)
            {
                if (device.Interactable == used)
                {
                    device.SetOn(false);
                }
            }

            CheckAllOff();
        }

        private void CheckAllOff()
        {
            if (finished)
            {
                return;
            }

            foreach (var device in devices)
            {
                if (device.IsOn)
                {
                    return;
                }
            }

            finished = true;

            if (story != null)
            {
                story.SetMany(flagsWhenAllOff);
            }

            onAllOff.Invoke();
        }
    }
}
