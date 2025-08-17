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

        private CancellationTokenSource _cts;
        private Rigidbody2D _rb;
        private float _timeInPhase;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            UpdateTargetDirection().Forget();
        }

        private void Update()
        {
            if (isMoving) UpdateMovementWave();

            ApplySegmentConstraints();
            UpdatePositions();

            DebugDrawLarva();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
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

            var oppositeDirection = points[segmentIndex] - point;

            SetMovementDirection(oppositeDirection);
        }

        private void UpdateMovementWave()
        {
            _timeInPhase += Time.deltaTime;
            if (!(_timeInPhase >= movementPhaseTime)) return;

            _timeInPhase = 0;
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


        private async UniTask UpdateTargetDirection()
        {
            _cts = new CancellationTokenSource();
            try
            {
                while (true)
                {
                    var timeToWait = timeToChangeDirection.Evaluate(Random.value);
                    await UniTask.Delay(TimeSpan.FromSeconds(timeToWait), cancellationToken: _cts.Token);
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

            var correction = (targetPosition - currentPoint) * 0.5f;

            if (applyRepelFromPoints) correction += CalculateRepelFromPoints(i);

            _velocities[i] += correction * (restoreForce * Time.deltaTime);
        }

        private Vector2 CalculateRepelFromPoints(int i)
        {
            var correction = Vector2.zero;
            if (i == 0) return correction;

            for (var j = 0; j < points.Length; j++)
            {
                if (i == j) continue;

                var minDistanceDivider =
                    AreNeighbours(i, j) ? NeighbourMinDistanceDivider : NotNeighbourMinDistanceDivider;
                var desiredDistance = _segmentTargetLengths[i - 1];
                var minDistanceToRepel = desiredDistance / minDistanceDivider;

                var distance = (points[i] - points[j]).magnitude;

                if (!(distance < minDistanceToRepel)) continue;

                var multiplier = desiredDistance / (minDistanceDivider * distance);
                correction += (points[i] - points[j]).normalized * multiplier;
            }

            return correction;
        }

        private static bool AreNeighbours(int i, int j)
        {
            return Mathf.Abs(i - j) == 1;
        }

        private void UpdatePositions()
        {
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = _segmentColliders[i].transform.position;
                points[i] += _velocities[i] * Time.deltaTime;
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