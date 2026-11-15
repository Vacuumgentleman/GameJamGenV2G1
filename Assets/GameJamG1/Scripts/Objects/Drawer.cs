using System.Collections.Generic;
using UnityEngine;

namespace GameJamG1.Objects
{
    public class Drawer : AnimatedInteractable
    {
        [Header("Drawer")]
        [Tooltip("Local displacement when fully open.")]
        [SerializeField] private Vector3 openOffset = new(0f, 0f, 0.4f);
        [Tooltip("Objects hidden until the drawer opens for the first time.")]
        [SerializeField] private List<GameObject> contents = new();

        private Vector3 closedPosition;

        protected override void Awake()
        {
            base.Awake();
            closedPosition = transform.localPosition;

            foreach (var item in contents)
            {
                if (item != null)
                {
                    item.SetActive(false);
                }
            }
        }

        protected override void OnOpenStarted()
        {
            foreach (var item in contents)
            {
                if (item != null)
                {
                    item.SetActive(true);
                }
            }
        }

        protected override void Evaluate(float value)
        {
            transform.localPosition = closedPosition + openOffset * value;
        }
    }
}
