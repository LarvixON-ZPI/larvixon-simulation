using UnityEngine;

[RequireComponent(typeof(Larva))]
public class LarvaRenderer : MonoBehaviour
{
    private static readonly int Color1 = Shader.PropertyToID("_Color");

    public Material larvaMaterial;

    public Color larvaColor = Color.green;
    public Color headColor = Color.red;
    public float bodyWidth = 0.3f;

    public int segmentResolution = 16;

    public float smoothingFactor = 0.5f;

    public bool useSplineInterpolation = true;
    private Mesh _bodyMesh;

    private Larva _larva;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _larva = GetComponent<Larva>();
        SetupMeshRenderer();
        CreateBodyMesh();
    }

    private void Update()
    {
        UpdateBodyMesh();
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
        var triangleCount =
            (points.Length - 1) * segmentResolution * 6; // 2 triangles per quad, 3 vertices per triangle
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
            if (i == 0) widthMultiplier = 1.2f; // Head larger
            else if (i == points.Length - 1) widthMultiplier = 0.6f; // Tail smaller

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
}