using UnityEngine;

namespace GameJamG1.Enemy
{
    public static class MonsterAnimatorParameters
    {
        public const string Speed = "Speed";
        public const string Attack = "Attack";
        public const string Scream = "Scream";

        public static readonly int SpeedHash = Animator.StringToHash(Speed);
        public static readonly int AttackHash = Animator.StringToHash(Attack);
        public static readonly int ScreamHash = Animator.StringToHash(Scream);
    }
}
