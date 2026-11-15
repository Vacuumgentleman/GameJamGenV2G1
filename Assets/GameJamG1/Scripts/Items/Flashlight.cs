using GameJamG1.Dialogue;
using UnityEngine;

namespace GameJamG1.Items
{
    public class Flashlight : LightSource
    {
        [Header("Battery")]
        [Tooltip("Seconds of light in the single battery.")]
        [SerializeField, Min(1f)] private float batteryDuration = 120f;
        [Tooltip("Below this many seconds the light flickers harder.")]
        [SerializeField, Min(0f)] private float lowBatteryThreshold = 20f;
        [SerializeField] private DialogueSequence depletedDialogue;

        protected override float InitialRemaining => batteryDuration;

        protected override float FlickerMultiplier => Remaining <= lowBatteryThreshold ? 5f : 1f;

        protected override bool CanTurnOn()
        {
            if (Remaining > 0f)
            {
                return true;
            }

            PlayDialogue(depletedDialogue);
            return false;
        }

        protected override void Tick(float deltaTime)
        {
            Remaining = Mathf.Max(0f, Remaining - deltaTime);

            if (Remaining <= 0f)
            {
                Deplete();
                PlayDialogue(depletedDialogue);
            }
        }
    }
}
