using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Larvae.Drugs
{
    [CreateAssetMenu(fileName = "CocaineEffect", menuName = "Drugs/Cocaine Effect")]
    public class CocaineEffect : DrugEffect
    {
        [Header("Additional Parameters")]
        [Range(0.1f, 5f)] public float directionChangeFrequency = 2f;

        [CanBeNull] private CancellationTokenSource _directionChangeCts;
        [CanBeNull] private CancellationTokenSource _lethalEffectCts;

        private void OnDestroy()
        {
            _directionChangeCts?.Cancel();
            _directionChangeCts?.Dispose();
            _lethalEffectCts?.Cancel();
            _lethalEffectCts?.Dispose();
        }

        public override string GetPreferredState(float intensity)
        {
            return "Moving";
        }

        public override void ApplyCustomEffects(Larva larva, float intensity)
        {
            StartErraticDirectionChanges(larva, intensity).Forget();
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
    }
}