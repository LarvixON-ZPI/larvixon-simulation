using UnityEngine;

public class Larva3 : MonoBehaviour
{
    [Header("Larva Structure")]
    public Vector2[] points = new Vector2[5]; // Head, 2/5, Middle, 4/5, Back

    public float segmentLength = 1.0f;
    public float bodyWidth = 0.3f;

    [Header("Movement Parameters")]
    public float contractionStrength = 2.0f;

    public float waveSpeed = 3.0f;
    public float dampening = 0.9f;
    public float restoreForce = 5.0f;
    public float headForwardForce = 3.0f;
    public float headDirectionInfluence = 0.8f;

    [Header("Movement State")]
    public bool isMoving;

    public Vector2 targetDirection = Vector2.right;
    private readonly float[] _naturalLengths = new float[4];

    // Internal state
    private readonly float[] _segmentTargetLengths = new float[4]; // Distance between adjacent points
    private readonly Vector2[] _velocities = new Vector2[5];
    private float _movementPhase;

    private void Start()
    {
        InitializeLarva();
    }

    private void Update()
    {
        if (isMoving) UpdateMovementWave();

        ApplySegmentConstraints();
        UpdatePositions();

        // Visual debug
        DrawLarva();
    }

    private void InitializeLarva()
    {
        // Initialize points in a straight line
        for (var i = 0; i < points.Length; i++) points[i] = transform.position + new Vector3(i * segmentLength, 0, 0);

        // Calculate natural segment lengths
        for (var i = 0; i < _naturalLengths.Length; i++)
        {
            _naturalLengths[i] = Vector2.Distance(points[i], points[i + 1]);
            _segmentTargetLengths[i] = _naturalLengths[i];
        }

        // Initialize velocities
        for (var i = 0; i < _velocities.Length; i++) _velocities[i] = Vector2.zero;
    }

    private void UpdateMovementWave()
    {
        _movementPhase += waveSpeed * Time.deltaTime;

        // Create a peristaltic wave along the body
        for (var i = 0; i < _segmentTargetLengths.Length; i++)
        {
            var segmentPhase = _movementPhase - i * 1.2f; // Wave propagates from head to tail

            // Create asymmetric wave for forward propulsion
            var contraction = Mathf.Sin(segmentPhase);

            // Make contractions stronger than extensions for net forward movement
            if (contraction > 0)
                contraction = Mathf.Pow(contraction, 0.7f) * 0.4f; // Stronger contraction
            else
                contraction = contraction * 0.2f; // Weaker extension

            _segmentTargetLengths[i] = _naturalLengths[i] * (1.0f + contraction);
        }
    }

    private void ApplySegmentConstraints()
    {
        // Apply forces to maintain segment lengths and create movement
        for (var i = 0; i < points.Length - 1; i++)
        {
            var segment = points[i + 1] - points[i];
            var currentLength = segment.magnitude;
            var targetLength = _segmentTargetLengths[i];

            if (!(currentLength > 0)) continue;

            var segmentDir = segment.normalized;
            var lengthDiff = targetLength - currentLength;

            var correction = segmentDir * (lengthDiff * 0.5f);

            // Apply correction (push/pull segments)
            if (i > 0) // Don't move the head as much
                _velocities[i] -= correction * (restoreForce * Time.deltaTime);
            _velocities[i + 1] += correction * (restoreForce * Time.deltaTime);

            // Add propulsion based on contraction dynamics
            if (isMoving && lengthDiff < 0) // When segment is contracting
            {
                // Create forward thrust by applying asymmetric forces
                var propulsionForce =
                    targetDirection * (contractionStrength * Mathf.Abs(lengthDiff) * Time.deltaTime);

                // Apply more force to rear segments during contraction (like real larvae)
                var propulsionFactor = (float)(i + 1) / (points.Length - 1);
                _velocities[i] += propulsionForce * (propulsionFactor * 0.3f);
                _velocities[i + 1] += propulsionForce * (propulsionFactor * 0.7f);
            }
        }

        // Add dedicated head forward movement
        if (isMoving)
        {
            var headForwardVelocity = targetDirection * headForwardForce * Time.deltaTime;
            _velocities[0] += headForwardVelocity;

            // Gradually influence subsequent segments to follow the head
            for (var i = 1; i < Mathf.Min(3, points.Length); i++)
            {
                var influence = headDirectionInfluence * (1.0f - i / 3.0f);
                _velocities[i] += headForwardVelocity * influence;
            }
        }
    }

    private void UpdatePositions()
    {
        // Apply velocities and dampening
        for (var i = 0; i < points.Length; i++)
        {
            points[i] += _velocities[i] * Time.deltaTime;
            _velocities[i] *= dampening;
        }

        // Update GameObject position to follow the center of mass
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

    private void DrawLarva()
    {
        // Draw body segments
        for (var i = 0; i < points.Length - 1; i++) Debug.DrawLine(points[i], points[i + 1], Color.green, 0.1f);

        // Draw points
        for (var i = 0; i < points.Length; i++)
        {
            var pointColor = i == 0 ? Color.red : Color.blue; // Head is red
            Debug.DrawRay(points[i], Vector2.up * 0.1f, pointColor, 0.1f);
            Debug.DrawRay(points[i], Vector2.right * 0.1f, pointColor, 0.1f);
        }
    }

    // Public methods to control the larva
    public void StartMoving(Vector2 direction)
    {
        isMoving = true;
        targetDirection = direction.normalized;
    }

    public void StopMoving()
    {
        isMoving = false;
        _movementPhase = 0f;
    }

    public void SetMovementDirection(Vector2 direction)
    {
        targetDirection = direction.normalized;
    }
}