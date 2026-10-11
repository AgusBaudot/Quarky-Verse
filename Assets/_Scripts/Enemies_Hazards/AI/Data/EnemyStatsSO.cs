using UnityEngine;

namespace Enemies_Hazards
{
    [CreateAssetMenu(fileName = "NewEnemyStats", menuName = "QuarkyVerse/AI/Enemy Stats")]
    public class EnemyStatsSO : ScriptableObject
    {
        [Header("Locomotion")] [Tooltip("Must be strictly less than the player's run speed for base enemies.")]
        public float MaxSpeed = 5f;
        public float Acceleration = 15f;
        public float DetectionRadius = 10f;

        [Header("Attack")] 
        public float AttackRange = 1.5f;
        public float AttackStartupTime = 0.2f;
        public float AttackActiveFramesDuration = 0.3f;

        [Header("Bounding (Jumping Enemy)")]
        public Vector3 JumpForce = new(0, 15f, 5f);
        public float JumpCooldown = 2f;
    }
}