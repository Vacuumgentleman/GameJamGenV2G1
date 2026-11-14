using System;
using UnityEngine;

namespace GameJamG1.Dialogue
{
    [Serializable]
    public class DialogueLine
    {
        [SerializeField, TextArea(2, 4)] private string text;
        [Tooltip("Seconds the line stays on screen. 0 uses the automatic duration.")]
        [SerializeField, Min(0f)] private float durationOverride;
        [Tooltip("Replaces the default typing blip for this line.")]
        [SerializeField] private AudioClip sound;

        public string Text => text;
        public float DurationOverride => durationOverride;
        public AudioClip Sound => sound;
    }
}
