using UnityEngine;

[RequireComponent(typeof(Larva))]
public class LarvaRenderer : MonoBehaviour
{
    private const float StripeZOffset = -0.01f;
    private const float HeadWidth = 1.2f;
    private const float TailWidth = 0.6f;
    private static readonly int Color1 = Shader.PropertyToID("_Color");

    public Material larvaMaterial;

    public Color larvaColor = Color.green;
    public Color headColor = Color.red;
    public float bodyWidth = 0.3f;

    public int segmentResolution = 16;

    public float smoothingFactor = 0.5f;

    public bool useSplineInterpolation = true;

    [Header("Stripe Settings")]
    public bool showStripes = true;

    public Color stripeColor = Color.black;
    public float stripeWidth = 0.02f;
    public int stripesPerSegment = 3;

    private Mesh _bodyMesh;

    private Larva _larva;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _stripesMesh;
    private GameObject _stripesObject;

    private void Start()
    {
        _larva = GetComponent<Larva>();
        SetupMeshRenderer();
        CreateBodyMesh();
        if (showStripes) CreateStripesObject();
    }

    private void Update()
    {
        UpdateBodyMesh();
        if (showStripes && _stripesObject) UpdateStripes();
    }

    private void OnDrawGizmos()
    {
        DrawPointsGizmos();

        DrawCenterOfMassGizmo();
    }

    private void DrawPointsGizmos()
    {
        for (var i = 0; i < _larva.points.Length; i++)
        {
            Gizmos.color = i == 0 ? headColor : larvaColor;
            Gizmos.DrawWireSphere(_larva.points[i], bodyWidth * 0.5f);
        }
    }

    private void DrawCenterOfMassGizmo()
    {
        Gizmos.color = Color.cyan;
        var center = _larva.GetCenter();
        Gizmos.DrawWireCube(center, Vector3.one * 0.1f);
    }

    private void SetupMeshRenderer()
    {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();

        _meshRenderer.material = larvaMaterial != null ? larvaMaterial : CreateDefaultMaterial();
        _meshRenderer.sortingOrder = 1;
    }

    private void CreateBodyMesh()
    {
        _bodyMesh = new Mesh
        {
            name = "LarvaBody"
        };
        _meshFilter.mesh = _bodyMesh;
    }

    private void UpdateBodyMesh()
    {
        var renderPoints = useSplineInterpolation ? GetSmoothedPoints(_larva.points) : _larva.points;

        GenerateBodyMesh(renderPoints);
    }

    private Vector2[] GetSmoothedPoints(Vector2[] originalPoints)
    {
        if (originalPoints.Length < 3) return originalPoints;

        var smoothedPoints = new Vector2[originalPoints.Length];

        smoothedPoints[0] = originalPoints[0];
        smoothedPoints[^1] = originalPoints[^1];

        // Smooth middle points using Catmull-Rom spline-like smoothing
        for (var i = 1; i < originalPoints.Length - 1; i++)
        {
            var prev = originalPoints[i - 1];
            var curr = originalPoints[i];
            var next = originalPoints[i + 1];

            // Simple smoothing: average with neighbors weighted by smoothing factor
            var smoothed = Vector2.Lerp(curr, (prev + next) * 0.5f, smoothingFactor);
            smoothedPoints[i] = smoothed;
        }

        return smoothedPoints;
    }

    private void GenerateBodyMesh(Vector2[] points)
    {
        if (points.Length < 2) return;

        var totalVertices = points.Length * segmentResolution;
        var vertices = new Vector3[totalVertices];
        var uvs = new Vector2[totalVertices];
        var colors = new Color[totalVertices];

        // Calculate triangles for the body segments
        // 2 triangles per quad, 3 vertices per triangle
        var triangleCount = (points.Length - 1) * segmentResolution * 6;
        var triangles = new int[triangleCount];

        // Convert world positions to local positions relative to transform
        var transformPosition = transform.position;

        // Generate circular vertices around each point
        for (var i = 0; i < points.Length; i++)
        {
            var center = points[i];

            // Convert to local space by subtracting transform position
            var localCenter = new Vector2(center.x - transformPosition.x, center.y - transformPosition.y);

            var widthMultiplier = 1.0f;
            if (i == 0) widthMultiplier = HeadWidth;
            else if (i == points.Length - 1) widthMultiplier = TailWidth;

            var currentWidth = bodyWidth * widthMultiplier;

            // Color interpolation from head to tail
            var segmentColor = Color.Lerp(headColor, larvaColor, (float)i / (points.Length - 1));

            for (var j = 0; j < segmentResolution; j++)
            {
                var angle = (float)j / segmentResolution * 2 * Mathf.PI;
                var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * currentWidth;

                var vertexIndex = i * segmentResolution + j;
                vertices[vertexIndex] = new Vector3(localCenter.x + offset.x, localCenter.y + offset.y, 0);

                // UV mapping
                uvs[vertexIndex] = new Vector2((float)j / segmentResolution, (float)i / (points.Length - 1));
                colors[vertexIndex] = segmentColor;
            }
        }

        // Generate triangles to connect segments
        var triangleIndex = 0;
        for (var i = 0; i < points.Length - 1; i++)
        {
            var currentRingStart = i * segmentResolution;
            var nextRingStart = (i + 1) * segmentResolution;

            for (var j = 0; j < segmentResolution; j++)
            {
                var current = currentRingStart + j;
                var next = currentRingStart + (j + 1) % segmentResolution;
                var currentNext = nextRingStart + j;
                var nextNext = nextRingStart + (j + 1) % segmentResolution;

                // First triangle
                triangles[triangleIndex++] = current;
                triangles[triangleIndex++] = currentNext;
                triangles[triangleIndex++] = next;

                // Second triangle
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = currentNext;
                triangles[triangleIndex++] = nextNext;
            }
        }

        _bodyMesh.Clear();
        _bodyMesh.vertices = vertices;
        _bodyMesh.triangles = triangles;
        _bodyMesh.uv = uvs;
        _bodyMesh.colors = colors;
        _bodyMesh.RecalculateNormals();
        _bodyMesh.RecalculateBounds();
    }

    private Material CreateDefaultMaterial()
    {
        Material mat = null;

        string[] shaderNames =
        {
            "Sprites/Default",
            "Legacy Shaders/Particles/Alpha Blended Premultiply",
            "UI/Default",
            "Standard"
        };

        foreach (var shaderName in shaderNames)
        {
            var shader = Shader.Find(shaderName);

            if (shader == null) continue;

            mat = new Material(shader);
            break;
        }

        if (mat == null) mat = new Material(Shader.Find("Standard"));

        mat.color = larvaColor;

        if (mat.HasProperty(Color1))
            mat.SetColor(Color1, Color.white);

        return mat;
    }

    private void CreateStripesObject()
    {
        _stripesObject = new GameObject("LarvaStripes");
        _stripesObject.transform.SetParent(transform);
        _stripesObject.transform.localPosition = Vector3.zero;

        var stripeMeshFilter = _stripesObject.AddComponent<MeshFilter>();
        var stripeMeshRenderer = _stripesObject.AddComponent<MeshRenderer>();

        _stripesMesh = new Mesh
        {
            name = "LarvaStripes"
        };
        stripeMeshFilter.mesh = _stripesMesh;

        var stripeMaterial = new Material(Shader.Find("Sprites/Default"))
        {
            color = stripeColor
        };
        stripeMeshRenderer.material = stripeMaterial;
        stripeMeshRenderer.sortingOrder = 2; // Render on top of body
    }

    private void UpdateStripes()
    {
        if (_larva.points.Length < 2) return;

        var renderPoints = useSplineInterpolation ? GetSmoothedPoints(_larva.points) : _larva.points;
        GenerateStripesMesh(renderPoints);
    }

    private void GenerateStripesMesh(Vector2[] points)
    {
        if (points.Length < 2) return;

        var totalStripes = (points.Length - 1) * stripesPerSegment;
        var vertices = new Vector3[totalStripes * 4];
        var triangles = new int[totalStripes * 6];
        var colors = new Color[totalStripes * 4];

        var transformPosition = transform.position;
        int vertexIndex = 0, triangleIndex = 0;

        for (var i = 0; i < points.Length - 1; i++)
        {
            var startPoint = points[i];
            var endPoint = points[i + 1];

            for (var stripeIdx = 0; stripeIdx < stripesPerSegment; stripeIdx++)
                AddStripeQuad(
                    startPoint, endPoint, transformPosition, i, points.Length, stripeIdx,
                    ref vertexIndex, ref triangleIndex, vertices, triangles, colors
                );
        }

        _stripesMesh.Clear();
        _stripesMesh.vertices = vertices;
        _stripesMesh.triangles = triangles;
        _stripesMesh.colors = colors;
        _stripesMesh.RecalculateNormals();
        _stripesMesh.RecalculateBounds();
    }

    private void AddStripeQuad(
        Vector2 start, Vector2 end, Vector3 transformPos, int segmentIdx, int pointsCount, int stripeIdx,
        ref int vertexIndex, ref int triangleIndex, Vector3[] vertices, int[] triangles, Color[] colors)
    {
        var t = (stripeIdx + 1f) / (stripesPerSegment + 1f);
        var center = Vector2.Lerp(start, end, t);
        var localCenter = center - (Vector2)transformPos;

        var segmentT = segmentIdx / (float)(pointsCount - 1) + t / (pointsCount - 1);
        var widthMultiplier = segmentT < 0.1f ? HeadWidth : segmentT > 0.9f ? TailWidth : 1f;
        var currentWidth = bodyWidth * widthMultiplier * 0.9f;

        var dir = (end - start).normalized;
        var perp = new Vector2(-dir.y, dir.x);
        var lineStart = localCenter - perp * currentWidth;
        var lineEnd = localCenter + perp * currentWidth;
        var lineDir = dir * (stripeWidth * 0.5f);

        var baseIdx = vertexIndex;

        vertices[vertexIndex] = new Vector3(lineStart.x - lineDir.x, lineStart.y - lineDir.y, StripeZOffset);
        colors[vertexIndex++] = stripeColor;
        vertices[vertexIndex] = new Vector3(lineStart.x + lineDir.x, lineStart.y + lineDir.y, StripeZOffset);
        colors[vertexIndex++] = stripeColor;
        vertices[vertexIndex] = new Vector3(lineEnd.x + lineDir.x, lineEnd.y + lineDir.y, StripeZOffset);
        colors[vertexIndex++] = stripeColor;
        vertices[vertexIndex] = new Vector3(lineEnd.x - lineDir.x, lineEnd.y - lineDir.y, StripeZOffset);
        colors[vertexIndex++] = stripeColor;

        triangles[triangleIndex++] = baseIdx;
        triangles[triangleIndex++] = baseIdx + 1;
        triangles[triangleIndex++] = baseIdx + 2;
        triangles[triangleIndex++] = baseIdx;
        triangles[triangleIndex++] = baseIdx + 2;
        triangles[triangleIndex++] = baseIdx + 3;
    }
}