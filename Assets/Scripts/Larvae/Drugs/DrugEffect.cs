using UnityEngine;

namespace Larvae.Drugs
{
    [System.Serializable]
    public struct MovementModifier
    {
        [Range(0f, 2f)]
        public float speedMultiplier;
        
        [Range(0f, 2f)]
        public float coordinationMultiplier; // How well segments work together
        
        [Range(0f, 5f)]
        public float randomnessMultiplier; // Amount of random movement
        
        [Range(0f, 1f)]
        public float directionStability; // How well the larva maintains direction
        
        [Range(0f, 1f)]
        public float segmentSyncMultiplier; // How synchronized the segments are
        
        [Range(0f, 2f)]
        public float headForceMultiplier;
        
        [Range(0f, 2f)]
        public float restoreForceMultiplier;
        
        public bool canMove;
        public bool canChangeDirection;
        
        public static MovementModifier Normal => new MovementModifier
        {
            speedMultiplier = 1f,
            coordinationMultiplier = 1f,
            randomnessMultiplier = 0f,
            directionStability = 1f,
            segmentSyncMultiplier = 1f,
            headForceMultiplier = 1f,
            restoreForceMultiplier = 1f,
            canMove = true,
            canChangeDirection = true
        };
    }
    
    public abstract class DrugEffect : ScriptableObject
    {
        [Header("Drug Properties")]
        public string drugName;
        [TextArea(3, 6)]
        public string description;
        
        [Header("Effect Parameters")]
        [Range(0f, 1f)]
        public float maxIntensity = 1f;
        
        [Range(0.1f, 60f)]
        public float duration = 10f;
        
        [Range(0.1f, 10f)]
        public float onsetTime = 1f;
        
        [Range(0.1f, 10f)]
        public float comedownTime = 2f;
        
        public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        public abstract MovementModifier GetMovementModifier(float intensity);
        
        public virtual string GetPreferredState(float intensity)
        {
            return "Moving";
        }
        
        public virtual void ApplyCustomEffects(Larva larva, float intensity) { }
    }
}
