using UnityEngine;

[RequireComponent(typeof(Larva))]
public class LarvaRenderer : MonoBehaviour
{
    [Header("Visual Settings")]
    public Material larvaMaterial;

    public Color larvaColor = Color.green;
    public Color headColor = Color.red;
    public float bodyWidth = 0.3f;
    public int segmentResolution = 8;

    private Larva _larva;
    private LineRenderer _lineRenderer;
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _larva = GetComponent<Larva>();
        SetupLineRenderer();
    }

    private void Update()
    {
        if (_larva != null && _lineRenderer != null) UpdateLineRenderer();
    }

    private void OnDrawGizmos()
    {
        if (_larva == null) return;

        // Draw body points
        for (var i = 0; i < _larva.points.Length; i++)
        {
            Gizmos.color = i == 0 ? headColor : larvaColor;
            Gizmos.DrawWireSphere(_larva.points[i], bodyWidth * 0.2f);

            // Draw segment connections
            if (i >= _larva.points.Length - 1) continue;

            Gizmos.color = Color.white * 0.5f;
            Gizmos.DrawLine(_larva.points[i], _larva.points[i + 1]);
        }

        // Draw center of mass
        Gizmos.color = Color.cyan;
        var center = _larva.GetCenter();
        Gizmos.DrawWireCube(center, Vector3.one * 0.1f);
    }

    private void SetupLineRenderer()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Try to use provided material or create a default one
        _lineRenderer.material = larvaMaterial != null ? larvaMaterial : CreateDefaultMaterial();

        var gradient = new Gradient();

        var colors = new GradientColorKey[1];
        colors[0] = new GradientColorKey(larvaColor, 0.0f);

        var alphas = new GradientAlphaKey[1];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);

        gradient.SetKeys(colors, alphas);

        _lineRenderer.colorGradient = gradient;
        _lineRenderer.startWidth = bodyWidth;
        _lineRenderer.endWidth = bodyWidth * 0.7f; // Tapered tail
        _lineRenderer.positionCount = _larva.points.Length;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.sortingOrder = 1;

        // Make the line smoother
        _lineRenderer.numCapVertices = 5;
        _lineRenderer.numCornerVertices = 5;
    }

    private Material CreateDefaultMaterial()
    {
        // Try multiple shader options for compatibility
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
            if (shader != null)
            {
                mat = new Material(shader);
                break;
            }
        }

        // Fallback if no shader found
        if (mat == null) mat = new Material(Shader.Find("Standard"));

        mat.color = larvaColor;
        return mat;
    }

    private void UpdateLineRenderer()
    {
        // Update line renderer positions
        for (var i = 0; i < _larva.points.Length; i++)
        {
            var point = new Vector3(_larva.points[i].x, _larva.points[i].y, 0);
            _lineRenderer.SetPosition(i, point);
        }

        // Create color gradient from head to tail
        var gradient = new Gradient();
        var colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(headColor, 0.0f);
        colorKeys[1] = new GradientColorKey(larvaColor, 1.0f);

        var alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(0.8f, 1.0f);

        gradient.SetKeys(colorKeys, alphaKeys);
        _lineRenderer.colorGradient = gradient;
    }
}