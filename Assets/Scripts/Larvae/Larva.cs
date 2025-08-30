using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Drugs;
using Larvae.States;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Larvae
{
    public class Larva : MonoBehaviour
    {
        private const float MinSpeedToChangeDirection = 0f;
        private const float AheadTargetAngleArc = 140f;
        private const float WideTargetAngleArc = 300f;

        private const float DefaultMovementSoftChangeTime = 1f;
        private const float DirectionTweakMultiplier = 0.05f;

        [Header("Larva Structure")] public Vector2[] points = new Vector2[5]; // Head, 2/5, Middle, 4/5, Back

        public float[] pointWidths = new float[5];

        public float segmentLength = 1.0f;

        [SerializeField] private float colliderWidthMultiplier = 0.5f;

        [Header("Movement Parameters")] public float dampening = 0.9f;

        public float restoreForce = 5.0f;
        public float headForwardForce = 3.0f;
        public float headDirectionInfluence = 0.8f;

        [Header("Curve Straightening")] public float maxAllowedCurveDegrees = 45.0f;

        public float curveStraighteningForce = 2.0f;

        [Header("Movement State")] public bool isMoving;

        // always normalized
        public Vector2 targetDirection = Vector2.right;
        [SerializeField] private float movementPhaseTime = 0.5f;

        [SerializeField] private MovementPhase movementPhase = MovementPhase.Rest;
        [SerializeField] private float headExtension = 2f;
        [SerializeField] private float tailRetraction = 0.5f;

        private readonly float[] _naturalLengths = new float[4];
        private readonly Segment[] _segments = new Segment[5];

        private readonly float[] _segmentTargetLengths = new float[4];
        private readonly Vector2[] _velocities = new Vector2[5];
        private DrugSystem _drugSystem;
        private float _movementModifier = 1f;
        private Rigidbody2D _rb;

        private CancellationTokenSource _softChangeCts;

        private LarvaStateMachine _stateMachine;
        private float _timeInPhase;

        private CancellationTokenSource _updateTargetDirectionCts;

        private MovementModifier CurrentMovementModifier => _drugSystem?.CurrentModifier ?? MovementModifier.Normal;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            _stateMachine = GetComponent<LarvaStateMachine>() ?? gameObject.AddComponent<LarvaStateMachine>();
            _drugSystem = GetComponent<DrugSystem>() ?? gameObject.AddComponent<DrugSystem>();

            SetupStateMachine();
        }

        private void Start()
        {
            _stateMachine.StartStateMachine();
            UpdateMovementWave().Forget();
        }

        private void FixedUpdate()
        {
            DebugDrawLarva();

            ApplySegmentConstraints();
            UpdatePositions();
            TweakTargetDirection();
        }

        private void OnDestroy()
        {
            _updateTargetDirectionCts?.Cancel();
            _updateTargetDirectionCts?.Dispose();
        }

        private void TweakTargetDirection()
        {
            targetDirection += DirectionTweakMultiplier * CurrentMovementModifier.randomnessMultiplier *
                               Random.insideUnitCircle;
            targetDirection.Normalize();
        }

        private void SetupStateMachine()
        {
            _stateMachine.RegisterState(new MovingState());
            _stateMachine.RegisterState(new LayingDownState());
            _stateMachine.RegisterState(new LookingAtEnvironmentState());
            _stateMachine.RegisterState(new LayingNearWallState());
            _stateMachine.RegisterState(new DeadState());
        }

        public float GetSegmentWidth(int i)
        {
            return _segments[i].Width;
        }

        public Vector2 GetHeadPosition()
        {
            return points[0];
        }

        public void Initialize(Transform segmentParent)
        {
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = transform.position + new Vector3(i * segmentLength, 0, 0);
                _segments[i] = SpawnColliderForSegment(i, segmentParent);
            }

            for (var i = 0; i < _naturalLengths.Length; i++)
            {
                _naturalLengths[i] = Vector2.Distance(points[i], points[i + 1]);
                _segmentTargetLengths[i] = _naturalLengths[i];
            }

            for (var i = 0; i < _velocities.Length; i++) _velocities[i] = Vector2.zero;
        }

        private Segment SpawnColliderForSegment(int i, Transform segmentParent)
        {
            var newGameObject = new GameObject($"Segment_{i}")
            {
                transform =
                {
                    position = points[i],
                    parent = segmentParent
                },
                layer = LayerMask.NameToLayer("Larva")
            };

            var rb = newGameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;

            var width = pointWidths[i];

            var newCollider = newGameObject.AddComponent<CircleCollider2D>();
            newCollider.radius = width * colliderWidthMultiplier;

            var segment = newGameObject.AddComponent<Segment>();
            segment.Initialize(i, width);
            segment.OnSegmentCollision += HandleSegmentCollision;

            return segment;
        }

        public event Action<int, float, Vector2> OnSegmentCollision;

        private void HandleSegmentCollision(int segmentIndex, float speed, Vector2 point)
        {
            OnSegmentCollision?.Invoke(segmentIndex, speed, point);

            if (segmentIndex != 0) return;

            if (speed < MinSpeedToChangeDirection) return;

            var oppositeDirection = (points[segmentIndex] - point).normalized + Random.insideUnitCircle / 2;

            SetMovementDirection(oppositeDirection);
        }

        private float GetRandomCoordinationMultiplier()
        {
            return Random.Range(CurrentMovementModifier.coordinationMultiplier,
                2 - CurrentMovementModifier.coordinationMultiplier);
        }

        private async UniTask UpdateMovementWave()
        {
            while (_stateMachine.CurrentState.StateName != "Dead")
            {
                var modifier = CurrentMovementModifier;
                var adjustedPhaseTime = movementPhaseTime / (modifier.speedMultiplier + 0.1f);

                await UniTask.Delay(TimeSpan.FromSeconds(adjustedPhaseTime));

                if (!isMoving || !modifier.canMove) continue;

                // Apply segment synchronization effects
                var shouldContinueNormalPhase = modifier.segmentSyncMultiplier > Random.value;

                if (shouldContinueNormalPhase)
                {
                    movementPhase = movementPhase switch
                    {
                        MovementPhase.ExtendingHead => MovementPhase.DraggingTail,
                        MovementPhase.Rest => MovementPhase.ExtendingHead,
                        MovementPhase.DraggingTail => MovementPhase.Rest,
                        _ => movementPhase
                    };
                }
                else
                {
                    // Randomize phase for desynchronization
                    var phases = new[] { MovementPhase.ExtendingHead, MovementPhase.Rest, MovementPhase.DraggingTail };
                    movementPhase = phases[Random.Range(0, phases.Length)];
                }

                ResetTargetLengths();

                switch (movementPhase)
                {
                    case MovementPhase.DraggingTail:
                        var retractionMultiplier = tailRetraction * GetRandomCoordinationMultiplier();
                        _segmentTargetLengths[3] = _naturalLengths[3] * retractionMultiplier;
                        break;
                    case MovementPhase.ExtendingHead:
                        var extensionMultiplier = headExtension * GetRandomCoordinationMultiplier();
                        _segmentTargetLengths[0] = _naturalLengths[0] * extensionMultiplier;
                        break;
                    case MovementPhase.Rest:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ResetTargetLengths()
        {
            for (var i = 0; i < _segmentTargetLengths.Length; i++)
                _segmentTargetLengths[i] = _naturalLengths[i];
        }

        private void ApplySegmentConstraints()
        {
            var modifier = CurrentMovementModifier;
            var headDirectionalForce = headForwardForce * modifier.headForceMultiplier * targetDirection;

            var randomForce = modifier.randomnessMultiplier * Time.fixedDeltaTime * Random.insideUnitCircle;
            headDirectionalForce += randomForce;

            if (!modifier.canMove || !isMoving) headDirectionalForce = Vector2.zero;

            if (movementPhase != MovementPhase.DraggingTail)
                ApplySegmentConstraint(0, 1, _segmentTargetLengths[0], false, headDirectionalForce);

            for (var i = 1; i < points.Length; i++)
                ApplySegmentConstraint(i, i - 1, _segmentTargetLengths[i - 1], true);

            ApplyCurveStraighteningForces();
        }

        private void ApplyCurveStraighteningForces()
        {
            for (var i = 1; i < points.Length - 1; i++)
            {
                var angle = CalculateAngleBetweenThreePoints(i - 1, i, i + 1);
                var curvature = 180f - angle;

                if (!(curvature > maxAllowedCurveDegrees)) continue;

                var prevPoint = points[i - 1];
                var currentPoint = points[i];
                var nextPoint = points[i + 1];

                var straightLineDirection = (nextPoint - prevPoint).normalized;
                var currentToLine =
                    Vector2.Dot(currentPoint - prevPoint, straightLineDirection) * straightLineDirection +
                    prevPoint;
                var straighteningDirection = (currentToLine - currentPoint).normalized;

                var excessCurvature = curvature - maxAllowedCurveDegrees;
                var forceMultiplier = excessCurvature / maxAllowedCurveDegrees;
                var straighteningForceVector = curveStraighteningForce * forceMultiplier * straighteningDirection;

                _velocities[i] += straighteningForceVector * Time.fixedDeltaTime;
            }
        }

        private void ApplySegmentConstraint(int i, int otherPointIndex, float targetDistance, bool applyRepelFromPoints)
        {
            ApplySegmentConstraint(i, otherPointIndex, targetDistance, applyRepelFromPoints, Vector2.zero);
        }

        private void ApplySegmentConstraint(int i, int otherPointIndex, float targetDistance, bool applyRepelFromPoints,
            Vector2 targetPositionOffset)
        {
            var previousPoint = points[otherPointIndex];
            var currentPoint = points[i];

            var direction = currentPoint - previousPoint;
            var currentDistance = direction.magnitude;

            if (!(currentDistance > 0)) return;

            var normalizedDirection = direction / currentDistance;
            var targetPosition = previousPoint + normalizedDirection * targetDistance + targetPositionOffset;

            var correction = targetPosition - currentPoint;

            if (applyRepelFromPoints) correction += CalculateRepelFromPoints(i);

            var modifier = CurrentMovementModifier;
            var modifiedRestoreForce = restoreForce * modifier.restoreForceMultiplier * _movementModifier;

            if (modifier.segmentSyncMultiplier < 1f && Random.value > modifier.segmentSyncMultiplier)
            {
                correction *= Random.Range(0.1f, 1.5f);
                correction += modifier.randomnessMultiplier * 0.5f * Random.insideUnitCircle;
            }

            _velocities[i] += modifiedRestoreForce * Time.fixedDeltaTime * correction;
        }

        private Vector2 CalculateRepelFromPoints(int i)
        {
            var correction = Vector2.zero;
            if (i == 0) return correction;

            for (var j = 0; j < points.Length; j++)
            {
                if (i == j || AreNeighbours(i, j)) continue;

                var direction = points[i] - points[j];
                var distance = direction.magnitude;

                var minDistance = segmentLength;

                if (!(distance < minDistance)) continue;

                var forceMagnitude = (minDistance - distance) * 0.5f;
                correction += direction.normalized * forceMagnitude;
            }

            return correction;
        }

        private static bool AreNeighbours(int i, int j)
        {
            return Mathf.Abs(i - j) == 1;
        }

        private float CalculateAngleBetweenThreePoints(int prevIndex, int currentIndex, int nextIndex)
        {
            if (prevIndex < 0 || nextIndex >= points.Length) return 0f;

            var prevPoint = points[prevIndex];
            var currentPoint = points[currentIndex];
            var nextPoint = points[nextIndex];

            var vector1 = (prevPoint - currentPoint).normalized;
            var vector2 = (nextPoint - currentPoint).normalized;

            var dot = Vector2.Dot(vector1, vector2);
            dot = Mathf.Clamp(dot, -1f, 1f);

            var angleRad = Mathf.Acos(dot);
            var angleDeg = angleRad * Mathf.Rad2Deg;

            return angleDeg;
        }

        private void UpdatePositions()
        {
            var modifier = CurrentMovementModifier;

            for (var i = 0; i < points.Length; i++)
            {
                points[i] = _segments[i].transform.position;

                var velocity = _velocities[i] * modifier.speedMultiplier;

                if (modifier.coordinationMultiplier < 1f)
                    velocity += (1f - modifier.coordinationMultiplier) * Time.fixedDeltaTime * Random.insideUnitCircle;

                points[i] += velocity * Time.fixedDeltaTime;
                _velocities[i] *= dampening;
                _segments[i].Rigidbody.MovePosition(points[i]);
            }

            var center = GetCenter();
            _rb.MovePosition(new Vector3(center.x, center.y, transform.position.z));
        }

        public Vector2 GetCenter()
        {
            var center = Vector2.zero;
            foreach (var t in points)
                center += t;

            return center / points.Length;
        }

        private void DebugDrawLarva()
        {
            for (var i = 0; i < points.Length - 1; i++) Debug.DrawLine(points[i], points[i + 1], Color.green, 0.1f);

            for (var i = 0; i < points.Length; i++)
            {
                var pointColor = i == 0 ? Color.red : Color.blue;
                Debug.DrawRay(points[i], Vector2.up * 0.1f, pointColor, 0.1f);
                Debug.DrawRay(points[i], Vector2.right * 0.1f, pointColor, 0.1f);
            }

            Debug.DrawRay(points[0], targetDirection, Color.yellow, 0.1f);
        }

        public void StartMoving(Vector2 direction)
        {
            isMoving = true;
            SetMovementDirection(direction.normalized);
        }

        public void StopMoving()
        {
            isMoving = false;
            movementPhase = MovementPhase.Rest;
            ResetTargetLengths();
        }

        public void SetMovementMultiplier(float modifier)
        {
            _movementModifier = modifier;
        }

        public async UniTask SoftChangeMovementMultiplier(float target)
        {
            await SoftChangeMovementMultiplier(DefaultMovementSoftChangeTime, target);
        }

        public async UniTask SoftChangeMovementMultiplier(float time, float target)
        {
            _softChangeCts?.Cancel();
            _softChangeCts?.Dispose();
            _softChangeCts = new CancellationTokenSource();
            var token = _softChangeCts.Token;

            var start = _movementModifier;
            var elapsed = 0f;

            try
            {
                if (Mathf.Approximately(target, _movementModifier))
                {
                    _movementModifier = target;
                    return;
                }

                while (elapsed < time)
                {
                    token.ThrowIfCancellationRequested();
                    elapsed += Time.deltaTime;
                    var t = Mathf.Clamp01(elapsed / time);
                    _movementModifier = Mathf.Lerp(start, target, t);
                    await UniTask.Yield(token);
                }

                _movementModifier = target;
            }
            catch (OperationCanceledException)
            {
            }
        }

        public Vector2 GetDesiredDirection()
        {
            var modifier = CurrentMovementModifier;
            var angleArc = Random.value < headDirectionInfluence ? AheadTargetAngleArc : WideTargetAngleArc;

            if (Random.value > modifier.directionStability) angleArc = WideTargetAngleArc;

            var halfArc = angleArc / 2f;
            var randomAngle = Random.Range(-halfArc, halfArc);

            var angleRad = randomAngle * Mathf.Deg2Rad;
            var rotatedDirection = new Vector2(
                targetDirection.x * Mathf.Cos(angleRad) - targetDirection.y * Mathf.Sin(angleRad),
                targetDirection.x * Mathf.Sin(angleRad) + targetDirection.y * Mathf.Cos(angleRad)
            );

            return rotatedDirection.normalized;
        }

        public void SetMovementDirection(Vector2 direction)
        {
            var modifier = CurrentMovementModifier;

            if (!modifier.canChangeDirection && Random.value > 0.1f) return;

            if (modifier.directionStability < 1f)
            {
                var randomInfluence = 1f - modifier.directionStability;
                var randomDirection = Random.insideUnitCircle.normalized;
                direction = Vector2.Lerp(direction, randomDirection, randomInfluence);
            }

            targetDirection = direction.normalized;
        }

        public void AddDrugEffect(DrugEffect drugEffect, float dosage = 1f)
        {
            _drugSystem.AddDrug(drugEffect, dosage);
        }

        public void ClearAllDrugEffects()
        {
            _drugSystem.ClearAllDrugs();
        }

        public void Die()
        {
            StopMoving();
            SetMovementMultiplier(0f);
            ClearAllDrugEffects();
            _stateMachine.ForceTransitionToState("Dead");
        }

        private enum MovementPhase
        {
            ExtendingHead,
            Rest,
            DraggingTail
        }
    }
}