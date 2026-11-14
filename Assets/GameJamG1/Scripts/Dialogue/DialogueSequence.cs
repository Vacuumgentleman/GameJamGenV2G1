using System.Collections.Generic;
using UnityEngine;

namespace GameJamG1.Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "GameJamG1/Dialogue/Sequence")]
    public class DialogueSequence : ScriptableObject
    {
        [SerializeField] private List<DialogueLine> lines = new();
        [Tooltip("Freeze player movement and look while this sequence plays.")]
        [SerializeField] private bool lockPlayer;

        public IReadOnlyList<DialogueLine> Lines => lines;
        public bool LockPlayer => lockPlayer;
    }
}
