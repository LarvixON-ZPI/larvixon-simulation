using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Larvae
{
    public class Larva : MonoBehaviour
    {
        private const int NotNeighbourMinDistanceDivider = 10;
        private const int NeighbourMinDistanceDivider = 5;

        private const float MinSpeedToChangeDirection = 0f;
        private const float AheadTargetAngleArc = 140f;
        private const float WideTargetAngleArc = 300f;

        [Header("Larva Structure")]
        public Vector2[] points = new Vector2[5]; // Head, 2/5, Middle, 4/5, Back

        public float[] pointWidths = new float[5];

        public float segmentLength = 1.0f;

        [SerializeField] private float colliderWidthMultiplier = 0.5f;

        [Header("Movement Parameters")]
        public float dampening = 0.9f;

        public float restoreForce = 5.0f;
        public float headForwardForce = 3.0f;
        public float headDirectionInfluence = 0.8f;

        [Header("Curve Straightening")]
        public float maxAllowedCurveDegrees = 45.0f;

        public float curveStraighteningForce = 2.0f;

        [Header("Movement State")]
        public bool isMoving;

        // always normalized
        public Vector2 targetDirection = Vector2.right;
        [SerializeField] private float movementPhaseTime = 0.5f;
        public AnimationCurve timeToChangeDirection;

        [SerializeField] private MovementPhase movementPhase = MovementPhase.Rest;
        [SerializeField] private float headExtension = 2f;
        [SerializeField] private float tailRetraction = 0.5f;

        private readonly float[] _naturalLengths = new float[4];
        private readonly Collider2D[] _segmentColliders = new Collider2D[5];
        private readonly Rigidbody2D[] _segmentRigidbodies = new Rigidbody2D[5];

        private readonly float[] _segmentTargetLengths = new float[4];
        private readonly Vector2[] _velocities = new Vector2[5];
        private Rigidbody2D _rb;
        private float _timeInPhase;

        private CancellationTokenSource _updateTargetDirectionCts;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            UpdateTargetDirection().Forget();
            UpdateMovementWave().Forget();
        }

        private void FixedUpdate()
        {
            ApplySegmentConstraints();
            UpdatePositions();

            DebugDrawLarva();
        }

        private void OnDestroy()
        {
            _updateTargetDirectionCts?.Cancel();
            _updateTargetDirectionCts?.Dispose();
        }

        public void Initialize(Transform segmentParent)
        {
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = transform.position + new Vector3(i * segmentLength, 0, 0);
                _segmentColliders[i] = SpawnColliderForSegment(i, segmentParent);
                _segmentRigidbodies[i] = _segmentColliders[i].attachedRigidbody;
            }

            for (var i = 0; i < _naturalLengths.Length; i++)
            {
                _naturalLengths[i] = Vector2.Distance(points[i], points[i + 1]);
                _segmentTargetLengths[i] = _naturalLengths[i];
            }

            for (var i = 0; i < _velocities.Length; i++) _velocities[i] = Vector2.zero;
        }

        private Collider2D SpawnColliderForSegment(int i, Transform segmentParent)
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

            var newCollider = newGameObject.AddComponent<CircleCollider2D>();
            newCollider.radius = pointWidths[i] * colliderWidthMultiplier;

            var segment = newGameObject.AddComponent<Segment>();
            segment.Initialize(i, this);
            segment.OnSegmentCollision += HandleSegmentCollision;

            return newCollider;
        }

        private void HandleSegmentCollision(int segmentIndex, float speed, Vector2 point)
        {
            if (segmentIndex != 0) return;

            if (speed < MinSpeedToChangeDirection) return;

            var oppositeDirection = (points[segmentIndex] - point).normalized + Random.insideUnitCircle / 2;

            SetMovementDirection(oppositeDirection);
        }

        private async UniTask UpdateMovementWave()
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(movementPhaseTime));

                if (!isMoving) continue;

                movementPhase = movementPhase switch
                {
                    MovementPhase.ExtendingHead => MovementPhase.DraggingTail,
                    MovementPhase.Rest => MovementPhase.ExtendingHead,
                    MovementPhase.DraggingTail => MovementPhase.Rest,
                    _ => movementPhase
                };

                for (var i = 0; i < _naturalLengths.Length; i++) _segmentTargetLengths[i] = _naturalLengths[i];

                switch (movementPhase)
                {
                    case MovementPhase.DraggingTail:
                        _segmentTargetLengths[3] = _naturalLengths[3] * tailRetraction;
                        break;
                    case MovementPhase.ExtendingHead:
                        _segmentTargetLengths[0] = _naturalLengths[0] * headExtension;
                        break;
                    case MovementPhase.Rest:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        private async UniTask UpdateTargetDirection()
        {
            var cts = new CancellationTokenSource();
            try
            {
                while (true)
                {
                    var timeToWait = timeToChangeDirection.Evaluate(Random.value);
                    await UniTask.Delay(TimeSpan.FromSeconds(timeToWait), cancellationToken: cts.Token);
                    SetMovementDirection(GetDesiredDirection());
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void ApplySegmentConstraints()
        {
            var headDirectionalForce = headForwardForce * targetDirection;

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

            _velocities[i] += restoreForce * Time.fixedDeltaTime * correction;
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

                if (distance < minDistance)
                {
                    var forceMagnitude = (minDistance - distance) * 0.5f; // The 0.5f here softens the repulsion
                    correction += direction.normalized * forceMagnitude;
                }
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
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = _segmentColliders[i].transform.position;
                points[i] += _velocities[i] * Time.fixedDeltaTime;
                _velocities[i] *= dampening;
                _segmentRigidbodies[i].MovePosition(points[i]);
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
        }

        private Vector2 GetDesiredDirection()
        {
            var angleArc = Random.value < headDirectionInfluence ? AheadTargetAngleArc : WideTargetAngleArc;
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
            targetDirection = direction.normalized;
        }

        private enum MovementPhase
        {
            ExtendingHead,
            Rest,
            DraggingTail
        }
    }
}