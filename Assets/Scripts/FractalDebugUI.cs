using UnityEngine;

public class FractalDebugUI : MonoBehaviour
{
    private static readonly int LightDirId = Shader.PropertyToID("light_direction");
    private static readonly int MaxStepsId = Shader.PropertyToID("max_steps");
    private static readonly int BaseColorId = Shader.PropertyToID("base_color");

    [Header("References")]
    public SphereFieldCameraController controller;
    public Material rayMarchMaterial;

    public bool showUI = true;
    public bool showCenterLine = true;

    private LineRenderer _lineRenderer;
    private Vector3 _lightDirection;
    private int _maxSteps;
    private Color _baseColor;

    private void Start()
    {
        if (controller && !rayMarchMaterial)
        {
            rayMarchMaterial = controller.rayMarchMaterial;
        }

        if (rayMarchMaterial)
        {
            Vector4 ld = rayMarchMaterial.GetVector(LightDirId);
            _lightDirection = new Vector3(ld.x, ld.y, ld.z);
            _maxSteps = rayMarchMaterial.GetInt(MaxStepsId);
            _baseColor = rayMarchMaterial.GetColor(BaseColorId);
        }

        // create line renderer for center line
        _lineRenderer = new GameObject("CenterDebugLine").AddComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = Color.red;
        _lineRenderer.endColor = Color.red;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showUI = !showUI;
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            bool isLocked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = isLocked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !isLocked;
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            showCenterLine = !showCenterLine;
        }

        if (controller && controller.mainCamera)
        {
            _lineRenderer.enabled = showCenterLine;
            if (showCenterLine)
            {
                _lineRenderer.SetPosition(0, controller.mainCamera.transform.position);
                _lineRenderer.SetPosition(1, Vector3.zero);
            }
        }
    }

    private void OnGUI()
    {
        if (!showUI || !rayMarchMaterial) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 250), GUI.skin.box);

        if (controller && controller.mainCamera)
        {
            Vector3 pos = controller.mainCamera.transform.position;
            GUILayout.Label($"Camera Pos: {pos}");
        }

        GUILayout.Label($"Max Steps: {_maxSteps}");
        _maxSteps = (int)GUILayout.HorizontalSlider(_maxSteps, 1, 512);
        rayMarchMaterial.SetInt(MaxStepsId, _maxSteps);

        GUILayout.Label("Light Direction");
        _lightDirection.x = GUILayout.HorizontalSlider(_lightDirection.x, -1f, 1f);
        _lightDirection.y = GUILayout.HorizontalSlider(_lightDirection.y, -1f, 1f);
        _lightDirection.z = GUILayout.HorizontalSlider(_lightDirection.z, -1f, 1f);
        rayMarchMaterial.SetVector(LightDirId, _lightDirection.normalized);

        GUILayout.Label("Base Color");
        _baseColor.r = GUILayout.HorizontalSlider(_baseColor.r, 0f, 1f);
        _baseColor.g = GUILayout.HorizontalSlider(_baseColor.g, 0f, 1f);
        _baseColor.b = GUILayout.HorizontalSlider(_baseColor.b, 0f, 1f);
        rayMarchMaterial.SetColor(BaseColorId, _baseColor);

        GUILayout.EndArea();
    }
}

