using GameJamG1.Dialogue;
using GameJamG1.Player;
using UnityEngine;

namespace GameJamG1.Items
{
    public class Candle : LightSource
    {
        [Header("Matches")]
        [Tooltip("Seconds the candle burns after lighting it with one match.")]
        [SerializeField, Min(1f)] private float burnTimePerMatch = 60f;
        [SerializeField] private DialogueSequence noMatchesDialogue;

        [Header("Wind")]
        [Tooltip("The flame goes out while the player sprints.")]
        [SerializeField] private bool extinguishOnSprint = true;

        private MatchBox matchBox;
        private PlayerInputReader input;

        protected override float InitialRemaining => StartsLit ? burnTimePerMatch : 0f;

        protected override void Awake()
        {
            base.Awake();
            input = GetComponentInParent<PlayerInputReader>();
        }

        protected override bool CanTurnOn()
        {
            if (matchBox == null)
            {
                matchBox = GetComponentInParent<MatchBox>();
            }

            if (matchBox == null || !matchBox.TryConsume(1))
            {
                PlayDialogue(noMatchesDialogue);
                return false;
            }

            Remaining = burnTimePerMatch;
            return true;
        }

        protected override void Tick(float deltaTime)
        {
            if (extinguishOnSprint && IsSprinting())
            {
                TurnOff();
                return;
            }

            Remaining -= deltaTime;

            if (Remaining <= 0f)
            {
                Deplete();
            }
        }

        private bool IsSprinting()
        {
            return input != null && input.SprintHeld && input.Move.sqrMagnitude > 0.01f;
        }
    }
}
