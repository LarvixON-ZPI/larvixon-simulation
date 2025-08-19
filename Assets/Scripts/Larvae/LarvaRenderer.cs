using UnityEngine;

namespace Larvae
{
    [RequireComponent(typeof(Larva))]
    public class LarvaRenderer : MonoBehaviour
    {
        private const float StripeZOffset = -0.01f;
        private static readonly int Color1 = Shader.PropertyToID("_Color");

        public Material larvaMaterial;

        [Header("Body Settings")]
        public Color larvaColor = Color.green;

        public Color headColor = Color.red;
        public Color tailColor = Color.blue;
        public float bodyWidth = 0.3f;
        public int segmentResolution = 16;
        public float smoothingFactor = 0.5f;
        public bool useSplineInterpolation = true;

        [Header("Stripe Settings")]
        public bool showStripes = true;

        public Color stripeColor = Color.black;
        public float stripeWidth = 0.02f;
        public int stripesPerSegment = 3;

        [Tooltip("How much the stripes extend beyond the body's radius.")]
        public float stripeExtension = 0.05f;

        [Tooltip("The resolution of the circular ends of the stripes. Higher is smoother.")]
        public int stripeEndCapResolution = 6;

        [Tooltip("Blends stripe color with the body color. 0 = Body Color, 1 = Stripe Color.")] [Range(0, 1)]
        public float stripeColorBlend = 1f;

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

        private Color GetBodyColorAt(float normalizedPosition)
        {
            return normalizedPosition < 0.5f
                ? Color.Lerp(headColor, larvaColor, normalizedPosition * 2)
                : Color.Lerp(larvaColor, tailColor, (normalizedPosition - 0.5f) * 2);
        }

        private void DrawPointsGizmos()
        {
            if (_larva == null || _larva.points == null) return;
            for (var i = 0; i < _larva.points.Length; i++)
            {
                Gizmos.color = GetBodyColorAt((float)i / (_larva.points.Length - 1));
                Gizmos.DrawWireSphere(_larva.points[i], _larva.GetSegmentWidth(i) * bodyWidth * 0.5f);
            }
        }

        private void DrawCenterOfMassGizmo()
        {
            if (_larva == null) return;
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
            _bodyMesh = new Mesh { name = "LarvaBody" };
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

            for (var i = 1; i < originalPoints.Length - 1; i++)
            {
                var prev = originalPoints[i - 1];
                var curr = originalPoints[i];
                var next = originalPoints[i + 1];

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

            var triangleCount = (points.Length - 1) * segmentResolution * 6;
            var triangles = new int[triangleCount];

            var transformPosition = transform.position;

            for (var i = 0; i < points.Length; i++)
            {
                var center = points[i];
                var localCenter = new Vector2(center.x - transformPosition.x, center.y - transformPosition.y);
                var currentWidth = _larva.GetSegmentWidth(i) * bodyWidth * 0.5f;

                var segmentColor = GetBodyColorAt((float)i / (points.Length - 1));

                for (var j = 0; j < segmentResolution; j++)
                {
                    var angle = (float)j / segmentResolution * 2 * Mathf.PI;
                    var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * currentWidth;

                    var vertexIndex = i * segmentResolution + j;
                    vertices[vertexIndex] = new Vector3(localCenter.x + offset.x, localCenter.y + offset.y, 0);
                    uvs[vertexIndex] = new Vector2((float)j / segmentResolution, (float)i / (points.Length - 1));
                    colors[vertexIndex] = segmentColor;
                }
            }

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

                    triangles[triangleIndex++] = current;
                    triangles[triangleIndex++] = currentNext;
                    triangles[triangleIndex++] = next;

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
                { "Sprites/Default", "Legacy Shaders/Particles/Alpha Blended Premultiply", "UI/Default", "Standard" };

            foreach (var shaderName in shaderNames)
            {
                var shader = Shader.Find(shaderName);
                if (shader == null) continue;
                mat = new Material(shader);
                break;
            }

            if (mat == null) mat = new Material(Shader.Find("Standard"));
            mat.color = larvaColor;
            if (mat.HasProperty(Color1)) mat.SetColor(Color1, Color.white);
            return mat;
        }

        private void CreateStripesObject()
        {
            _stripesObject = new GameObject("LarvaStripes");
            _stripesObject.transform.SetParent(transform);
            _stripesObject.transform.localPosition = Vector3.zero;

            var stripeMeshFilter = _stripesObject.AddComponent<MeshFilter>();
            var stripeMeshRenderer = _stripesObject.AddComponent<MeshRenderer>();

            _stripesMesh = new Mesh { name = "LarvaStripes" };
            stripeMeshFilter.mesh = _stripesMesh;

            var stripeMaterial = new Material(Shader.Find("Sprites/Default")) { color = stripeColor };
            stripeMeshRenderer.material = stripeMaterial;
            stripeMeshRenderer.sortingOrder = 2;
        }

        private void UpdateStripes()
        {
            if (_larva.points.Length < 2) return;

            var renderPoints = useSplineInterpolation ? GetSmoothedPoints(_larva.points) : _larva.points;
            GenerateStripesMesh(renderPoints);
        }

        private void GenerateStripesMesh(Vector2[] points)
        {
            if (points.Length < 2 || stripesPerSegment <= 0 || stripeEndCapResolution < 1)
            {
                _stripesMesh.Clear();
                return;
            }

            var capVerts = stripeEndCapResolution + 1;
            var vertsPerStripe = capVerts * 2;
            var trisPerStripe = stripeEndCapResolution * 6;

            var totalStripes = (points.Length - 1) * stripesPerSegment;
            var vertices = new Vector3[totalStripes * vertsPerStripe];
            var triangles = new int[totalStripes * trisPerStripe];
            var colors = new Color[totalStripes * vertsPerStripe];

            var transformPosition = transform.position;
            int vertexIndex = 0, triangleIndex = 0;

            for (var i = 0; i < points.Length - 1; i++)
            for (var stripeIdx = 0; stripeIdx < stripesPerSegment; stripeIdx++)
                GenerateSingleStripe(
                    points[i], points[i + 1], transformPosition, i, points.Length, stripeIdx,
                    ref vertexIndex, ref triangleIndex, vertices, triangles, colors
                );

            _stripesMesh.Clear();
            _stripesMesh.vertices = vertices;
            _stripesMesh.triangles = triangles;
            _stripesMesh.colors = colors;
            _stripesMesh.RecalculateNormals();
            _stripesMesh.RecalculateBounds();
        }

        private void GenerateSingleStripe(
            Vector2 start, Vector2 end, Vector3 transformPos, int segmentIdx, float totalSegments, int stripeIdx,
            ref int vertexIndex, ref int triangleIndex, Vector3[] vertices, int[] triangles, Color[] colors)
        {
            // --- Basic Properties ---
            var t = (stripeIdx + 1f) / (stripesPerSegment + 1f);
            var center = Vector2.Lerp(start, end, t);
            var localCenter = center - (Vector2)transformPos;

            var dir = (end - start).normalized;
            var perp = new Vector2(-dir.y, dir.x);

            // --- Color Blending ---
            var normalizedGlobalPos = (segmentIdx + t) / (totalSegments - 1);
            var bodyColor = GetBodyColorAt(normalizedGlobalPos);
            var finalStripeColor = Color.Lerp(bodyColor, stripeColor, stripeColorBlend);

            // --- Size and Extension ---
            var startRadius = _larva.GetSegmentWidth(segmentIdx) * bodyWidth * 0.5f;
            var endRadius = _larva.GetSegmentWidth(segmentIdx + 1) * bodyWidth * 0.5f;
            var bodyRadius = Mathf.Lerp(startRadius, endRadius, t);

            var stripeHalfLength = bodyRadius + stripeExtension;
            var stripeRadius = stripeWidth * 0.5f;

            // --- Geometry Calculation ---
            var capCenter1 = localCenter - perp * stripeHalfLength;
            var capCenter2 = localCenter + perp * stripeHalfLength;

            var baseIdx = vertexIndex;
            var capVerts = stripeEndCapResolution + 1;

            // Generate vertices for the two semicircle caps
            for (var j = 0; j < capVerts; j++)
            {
                var p = (float)j / stripeEndCapResolution; // 0 to 1
                var angle = p * Mathf.PI - Mathf.PI / 2f; // -90 to +90 degrees

                var u = Mathf.Sin(angle) * stripeRadius; // Displacement along the stripe's direction
                var v = Mathf.Cos(angle) * stripeRadius; // Displacement perpendicular to the stripe (outward bulge)

                // First cap strip (on the "-perp" side)
                var offset1 = dir * u - perp * v;
                vertices[vertexIndex] = new Vector3(capCenter1.x + offset1.x, capCenter1.y + offset1.y, StripeZOffset);
                colors[vertexIndex++] = finalStripeColor;

                // Second cap strip (on the "+perp" side)
                var offset2 = dir * u + perp * v;
                vertices[vertexIndex] = new Vector3(capCenter2.x + offset2.x, capCenter2.y + offset2.y, StripeZOffset);
                colors[vertexIndex++] = finalStripeColor;
            }

            // --- Triangulation ---
            // Connect the two strips of vertices with quads
            for (var j = 0; j < stripeEndCapResolution; j++)
            {
                var i1 = baseIdx + j * 2;
                var i2 = baseIdx + j * 2 + 1;
                var i3 = baseIdx + (j + 1) * 2;
                var i4 = baseIdx + (j + 1) * 2 + 1;

                // First triangle of the quad
                triangles[triangleIndex++] = i1;
                triangles[triangleIndex++] = i2;
                triangles[triangleIndex++] = i3;

                // Second triangle of the quad
                triangles[triangleIndex++] = i3;
                triangles[triangleIndex++] = i2;
                triangles[triangleIndex++] = i4;
            }
        }
    }
}