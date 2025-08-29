using System;
using UnityEngine;

namespace Larvae.Drugs
{
    [Serializable]
    public struct MovementModifier
    {
        [Range(0f, 2f)] public float speedMultiplier;

        [Range(0f, 2f)] public float coordinationMultiplier; // How well segments work together

        [Range(0f, 5f)] public float randomnessMultiplier; // Amount of random movement

        [Range(0f, 1f)] public float directionStability; // How well the larva maintains direction

        [Range(0f, 1f)] public float segmentSyncMultiplier; // How synchronized the segments are

        [Range(0f, 2f)] public float headForceMultiplier;

        [Range(0f, 2f)] public float restoreForceMultiplier;

        public bool canMove;
        public bool canChangeDirection;

        public static MovementModifier Normal => new()
        {
            speedMultiplier = 1f,
            coordinationMultiplier = 1f,
            randomnessMultiplier = 1f,
            directionStability = 1f,
            segmentSyncMultiplier = 1f,
            headForceMultiplier = 1f,
            restoreForceMultiplier = 1f,
            canMove = true,
            canChangeDirection = true
        };
    }
}