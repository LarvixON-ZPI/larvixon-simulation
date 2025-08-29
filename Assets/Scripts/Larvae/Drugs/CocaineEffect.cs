using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Larvae.Drugs
{
    [CreateAssetMenu(fileName = "CocaineEffect", menuName = "Drugs/Cocaine Effect")]
    public class CocaineEffect : DrugEffect
    {
        [Header("Additional Parameters")]
        [Range(0.1f, 5f)] public float directionChangeFrequency = 2f;

        [Range(0f, 1f)] public float lethalDosageThreshold = 1f;

        [Range(60f, 300f)] public float lethalTime = 150f; // 2.5 minutes in seconds

        [Range(0f, 2f)] public float maxSpeedMultiplier = 3f;

        [Range(0f, 5f)] public float maxRandomnessMultiplier = 2f;
        [Range(1f, 10f)] public float maxHeadForceMultiplier = 5f;
        [Range(1f, 5f)] public float maxRestoreForceMultiplier = 2f;

        [Range(0f, 1f)] public float minCoordinationMultiplier = 0.2f;

        [Range(0f, 1f)] public float minDirectionStability = 0.1f;
        [Range(0f, 1f)] public float minSegmentSyncMultiplier = 0.3f;

        private CancellationTokenSource _directionChangeCts;
        private CancellationTokenSource _lethalEffectCts;

        private void OnDestroy()
        {
            _directionChangeCts?.Cancel();
            _directionChangeCts?.Dispose();
            _lethalEffectCts?.Cancel();
            _lethalEffectCts?.Dispose();
        }

        public override MovementModifier GetMovementModifier(float intensity)
        {
            var speedMultiplier = Mathf.Lerp(1f, maxSpeedMultiplier, intensity);
            var coordinationMultiplier = Mathf.Lerp(1f, minCoordinationMultiplier, intensity);
            var randomnessMultiplier = Mathf.Lerp(1f, maxRandomnessMultiplier, intensity);
            var directionStability = Mathf.Lerp(1f, minDirectionStability, intensity);
            var segmentSyncMultiplier = Mathf.Lerp(1f, minSegmentSyncMultiplier, intensity);
            var headForceMultiplier = Mathf.Lerp(1f, maxHeadForceMultiplier, intensity);
            var restoreForceMultiplier = Mathf.Lerp(1f, maxRestoreForceMultiplier, intensity);

            return new MovementModifier
            {
                speedMultiplier = speedMultiplier,
                coordinationMultiplier = coordinationMultiplier,
                randomnessMultiplier = randomnessMultiplier,
                directionStability = directionStability,
                segmentSyncMultiplier = segmentSyncMultiplier,
                headForceMultiplier = headForceMultiplier,
                restoreForceMultiplier = restoreForceMultiplier,
                canMove = true,
                canChangeDirection = true
            };
        }

        public override string GetPreferredState(float intensity)
        {
            return "Moving";
        }

        public override void ApplyCustomEffects(Larva larva, float intensity)
        {
            StartErraticDirectionChanges(larva, intensity).Forget();
            if (intensity >= lethalDosageThreshold) StartLethalEffect(larva).Forget();
        }

        private async UniTaskVoid StartErraticDirectionChanges(Larva larva, float intensity)
        {
            _directionChangeCts?.Cancel();
            _directionChangeCts?.Dispose();
            _directionChangeCts = new CancellationTokenSource();
            var token = _directionChangeCts.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var changeInterval = directionChangeFrequency / (intensity + 0.5f);
                    await UniTask.Delay(TimeSpan.FromSeconds(changeInterval), cancellationToken: token);

                    if (token.IsCancellationRequested) break;

                    var randomVariation = Random.insideUnitCircle.normalized * intensity;
                    var currentDirection = larva.targetDirection;
                    var newDirection = (currentDirection + randomVariation).normalized;

                    larva.SetMovementDirection(newDirection);
                    if (!larva.isMoving) larva.StartMoving(newDirection);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTaskVoid StartLethalEffect(Larva larva)
        {
            _lethalEffectCts?.Cancel();
            _lethalEffectCts?.Dispose();
            _lethalEffectCts = new CancellationTokenSource();
            var token = _lethalEffectCts.Token;

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(lethalTime), cancellationToken: token);
                if (!token.IsCancellationRequested)
                    larva.Die();
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}