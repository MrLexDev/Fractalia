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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showUI = !showUI;
        }

        if (controller && controller.mainCamera)
        {
            Debug.DrawLine(controller.mainCamera.transform.position, Vector3.zero, Color.green);
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

