using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameJamG1.Story
{
    public class StoryManager : MonoBehaviour
    {
        [Header("Initial State")]
        [Tooltip("Flags that are already active when the scene starts.")]
        [SerializeField] private List<StoryFlag> initialFlags = new();

        private readonly HashSet<StoryFlag> activeFlags = new();

        public static StoryManager Instance { get; private set; }

        public event Action<StoryFlag> OnFlagChanged;

        public IReadOnlyCollection<StoryFlag> ActiveFlags => activeFlags;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;

            foreach (var flag in initialFlags)
            {
                if (flag != null)
                {
                    activeFlags.Add(flag);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool Has(StoryFlag flag)
        {
            return flag != null && activeFlags.Contains(flag);
        }

        public bool HasAll(IReadOnlyList<StoryFlag> flags)
        {
            for (var i = 0; i < flags.Count; i++)
            {
                if (flags[i] != null && !activeFlags.Contains(flags[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasAny(IReadOnlyList<StoryFlag> flags)
        {
            for (var i = 0; i < flags.Count; i++)
            {
                if (flags[i] != null && activeFlags.Contains(flags[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public void Set(StoryFlag flag)
        {
            if (flag != null && activeFlags.Add(flag))
            {
                OnFlagChanged?.Invoke(flag);
            }
        }

        public void SetMany(IReadOnlyList<StoryFlag> flags)
        {
            for (var i = 0; i < flags.Count; i++)
            {
                Set(flags[i]);
            }
        }

        public void Clear(StoryFlag flag)
        {
            if (flag != null && activeFlags.Remove(flag))
            {
                OnFlagChanged?.Invoke(flag);
            }
        }
    }
}
