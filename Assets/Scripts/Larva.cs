using System;
using UnityEngine;

public class Larva : MonoBehaviour
{
    [Header("Larva Structure")]
    public Vector2[] points = new Vector2[5]; // Head, 2/5, Middle, 4/5, Back

    public float segmentLength = 1.0f;

    [Header("Movement Parameters")]
    public float dampening = 0.9f;

    public float restoreForce = 5.0f;
    public float headForwardForce = 3.0f;
    public float headDirectionInfluence = 0.8f;

    [Header("Movement State")]
    public bool isMoving;

    public Vector2 targetDirection = Vector2.right;
    [SerializeField] private float movementPhaseTime = 0.5f;

    [SerializeField] private MovementPhase movementPhase = MovementPhase.Rest;

    private readonly float[] _naturalLengths = new float[4];

    // Internal state
    private readonly float[] _segmentTargetLengths = new float[4];
    private readonly Vector2[] _velocities = new Vector2[5];
    private float _timeInPhase;

    private void Start()
    {
        InitializeLarva();
    }

    private void Update()
    {
        if (isMoving) UpdateMovementWave();

        ApplySegmentConstraints();
        UpdatePositions();

        DebugDrawLarva();
    }

    private void InitializeLarva()
    {
        for (var i = 0; i < points.Length; i++) points[i] = transform.position + new Vector3(i * segmentLength, 0, 0);

        for (var i = 0; i < _naturalLengths.Length; i++)
        {
            _naturalLengths[i] = Vector2.Distance(points[i], points[i + 1]);
            _segmentTargetLengths[i] = _naturalLengths[i];
        }

        for (var i = 0; i < _velocities.Length; i++) _velocities[i] = Vector2.zero;
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
                _segmentTargetLengths[3] = _naturalLengths[3] * .5f;
                break;
            case MovementPhase.ExtendingHead:
                _segmentTargetLengths[0] = _naturalLengths[0] * 2f;
                break;
            case MovementPhase.Rest:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ApplySegmentConstraints()
    {
        if (movementPhase != MovementPhase.DraggingTail)
        {
            var previousPoint = points[1];
            var currentPoint = points[0];
            var targetDistance = _segmentTargetLengths[0];

            var direction = currentPoint - previousPoint;
            var currentDistance = direction.magnitude;

            var normalizedDirection = direction / currentDistance;
            var targetPosition = previousPoint + normalizedDirection * targetDistance +
                                 targetDirection * headForwardForce;

            var correction = (targetPosition - currentPoint) * 0.5f;
            _velocities[0] += correction * (restoreForce * Time.deltaTime);
        }

        for (var i = 1; i < points.Length; i++)
        {
            var previousPoint = points[i - 1];
            var currentPoint = points[i];
            var targetDistance = _segmentTargetLengths[i - 1];

            var direction = currentPoint - previousPoint;
            var currentDistance = direction.magnitude;

            if (!(currentDistance > 0)) continue;

            var normalizedDirection = direction / currentDistance;
            var targetPosition = previousPoint + normalizedDirection * targetDistance;

            var additionalForce = currentDistance < _segmentTargetLengths[i - 1] / 5
                ? _segmentTargetLengths[i - 1] / (5 * currentDistance)
                : 1;

            var correction = (targetPosition - currentPoint) * 0.5f;
            _velocities[i] += correction * (restoreForce * Time.deltaTime * additionalForce);
        }
    }

    private void UpdatePositions()
    {
        for (var i = 0; i < points.Length; i++)
        {
            points[i] += _velocities[i] * Time.deltaTime;
            _velocities[i] *= dampening;
        }

        var center = GetCenter();
        transform.position = new Vector3(center.x, center.y, transform.position.z);
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
        movementPhase = 0f;
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