using System;
using UnityEngine;

namespace GameJamG1.Items
{
    [Serializable]
    public class LightRuntimeState
    {
        [field: SerializeField] public bool Initialized { get; set; }
        [field: SerializeField] public bool Lit { get; set; }
        [field: SerializeField] public float Remaining { get; set; }
    }
}
