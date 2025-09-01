namespace Drugs
{
    public struct MovementModifier
    {
        public float SpeedMultiplier;

        public float SegmentCoordinationMultiplier;

        public float RandomnessMultiplier;

        public float DirectionStability;

        public float SegmentSyncMultiplier;

        public float HeadForceMultiplier;

        public float RestoreForceMultiplier;

        public bool CanMove;
        public bool CanChangeDirection;

        public static MovementModifier Normal => new()
        {
            SpeedMultiplier = 1f,
            SegmentCoordinationMultiplier = 1f,
            RandomnessMultiplier = 1f,
            DirectionStability = 1f,
            SegmentSyncMultiplier = 1f,
            HeadForceMultiplier = 1f,
            RestoreForceMultiplier = 1f,
            CanMove = true,
            CanChangeDirection = true
        };
    }
}