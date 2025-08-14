using UnityEngine;

[RequireComponent(typeof(Larva))]
public class LarvaEyes : MonoBehaviour
{
    private const float EyeZOffset = -0.02f;

    private const float PupilZOffset = -0.001f;

    [Header("Eye Settings")]
    public Material eyeMaterial;

    public Color eyeColor = Color.white;
    public Color pupilColor = Color.black;

    [Header("Eye Positioning")]
    public float eyeSize = 0.15f;

    public float eyeSpacing = 0.3f;
    public float eyeOffsetFromHead = 0.2f;

    [Header("Pupil Settings")]
    public float pupilSize = 0.08f;

    public float pupilMaxOffset = 0.04f;
    public float lookAheadDistance = 1.0f;
    public float eyeRotationSpeed = 5.0f;

    [Header("Eye Animation")]
    public bool enableBlinking = true;

    public float blinkInterval = 3.0f;
    public float blinkDuration = 0.2f;
    private float _blinkTimer;

    private Vector2 _currentLookDirection;
    private bool _isBlinking;

    private Larva _larva;
    private GameObject _leftEye, _rightEye;
    private GameObject _leftPupil, _rightPupil;

    private float _nextBlinkTime;
    private Vector2 _targetLookDirection;

    private void Start()
    {
        _larva = GetComponent<Larva>();
        CreateEyes();

        _nextBlinkTime = GetNextBlinkTime();
        _currentLookDirection = Vector2.right;
        _targetLookDirection = Vector2.right;
    }

    private void Update()
    {
        UpdateEyePositions();
        UpdateEyeLookDirection();

        if (enableBlinking) UpdateBlinking();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || _larva == null) return;

        if (_larva.points.Length <= 0) return;

        var headPos = _larva.points[0];
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(headPos, _currentLookDirection * lookAheadDistance);
    }

    private void CreateEyes()
    {
        _leftEye = CreateEyeObject("LeftEye", eyeColor);
        _leftPupil = CreatePupilObject("LeftPupil", _leftEye.transform, pupilColor);

        _rightEye = CreateEyeObject("RightEye", eyeColor);
        _rightPupil = CreatePupilObject("RightPupil", _rightEye.transform, pupilColor);
    }

    private GameObject CreateEyeObject(string objectName, Color color)
    {
        var eyeObj = new GameObject(objectName);
        eyeObj.transform.SetParent(transform);

        var meshFilter = eyeObj.AddComponent<MeshFilter>();
        var meshRenderer = eyeObj.AddComponent<MeshRenderer>();

        meshFilter.mesh = CreateCircleMesh(eyeSize, 16);

        var material = new Material(eyeMaterial)
        {
            color = color
        };
        meshRenderer.material = material;
        meshRenderer.sortingOrder = 3; // Render on top of stripes

        return eyeObj;
    }

    private GameObject CreatePupilObject(string objectName, Transform parent, Color color)
    {
        var pupilObj = new GameObject(objectName);
        pupilObj.transform.SetParent(parent);
        pupilObj.transform.localPosition = Vector3.zero;

        var meshFilter = pupilObj.AddComponent<MeshFilter>();
        var meshRenderer = pupilObj.AddComponent<MeshRenderer>();

        meshFilter.mesh = CreateCircleMesh(pupilSize, 12);

        var material = new Material(eyeMaterial)
        {
            color = color
        };
        meshRenderer.material = material;
        meshRenderer.sortingOrder = 4; // Render on top of the eye

        return pupilObj;
    }

    private static Mesh CreateCircleMesh(float radius, int segments)
    {
        var mesh = new Mesh();
        var vertices = new Vector3[segments + 1];
        var triangles = new int[segments * 3];
        var uvs = new Vector2[segments + 1];

        // Center vertex
        vertices[0] = Vector3.zero;
        uvs[0] = Vector2.one * 0.5f;

        // Circle vertices
        for (var i = 0; i < segments; i++)
        {
            var angle = (float)i / segments * 2f * Mathf.PI;
            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );
            uvs[i + 1] = new Vector2(
                Mathf.Cos(angle) * 0.5f + 0.5f,
                Mathf.Sin(angle) * 0.5f + 0.5f
            );
        }

        // Create triangles
        for (var i = 0; i < segments; i++)
        {
            var triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void UpdateEyePositions()
    {
        if (_larva.points.Length == 0) return;

        var headPosition = _larva.points[0];
        var localHeadPosition = headPosition - (Vector2)transform.position;

        var headDirection = Vector2.right;
        if (_larva.points.Length > 1) headDirection = (_larva.points[0] - _larva.points[1]).normalized;

        var perpendicular = new Vector2(-headDirection.y, headDirection.x);

        var eyeBasePosition = localHeadPosition + headDirection * eyeOffsetFromHead;

        var leftEyePos = eyeBasePosition + perpendicular * (eyeSpacing * 0.5f);
        var rightEyePos = eyeBasePosition - perpendicular * (eyeSpacing * 0.5f);

        _leftEye.transform.localPosition = new Vector3(leftEyePos.x, leftEyePos.y, EyeZOffset);
        _rightEye.transform.localPosition = new Vector3(rightEyePos.x, rightEyePos.y, EyeZOffset);
    }

    private void UpdateEyeLookDirection()
    {
        _targetLookDirection = _larva.targetDirection;

        _currentLookDirection = Vector2.Lerp(_currentLookDirection, _targetLookDirection,
            eyeRotationSpeed * Time.deltaTime);

        var pupilOffset = _currentLookDirection * pupilMaxOffset;

        _leftPupil.transform.localPosition = new Vector3(pupilOffset.x, pupilOffset.y, PupilZOffset);
        _rightPupil.transform.localPosition = new Vector3(pupilOffset.x, pupilOffset.y, PupilZOffset);
    }

    private void UpdateBlinking()
    {
        if (_isBlinking)
        {
            _blinkTimer += Time.deltaTime;

            var blinkProgress = _blinkTimer / blinkDuration;
            var scaleY = Mathf.Lerp(1f, 0.1f, Mathf.Sin(blinkProgress * Mathf.PI));

            _leftEye.transform.localScale = new Vector3(1f, scaleY, 1f);
            _rightEye.transform.localScale = new Vector3(1f, scaleY, 1f);

            if (!(_blinkTimer >= blinkDuration)) return;

            _isBlinking = false;
            _blinkTimer = 0f;
            _nextBlinkTime = GetNextBlinkTime();

            _leftEye.transform.localScale = Vector3.one;
            _rightEye.transform.localScale = Vector3.one;
        }
        else if (Time.time >= _nextBlinkTime)
        {
            _isBlinking = true;
            _blinkTimer = 0f;
        }
    }

    private float GetNextBlinkTime()
    {
        return Time.time + Random.Range(blinkInterval * 0.5f, blinkInterval * 1.5f);
    }
}