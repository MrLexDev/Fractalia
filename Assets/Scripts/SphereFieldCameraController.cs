using UnityEngine;
using UnityEngine.Serialization;

public class SphereFieldCameraController : MonoBehaviour
{
    private static readonly int CamPos = Shader.PropertyToID("cam_pos");
    private static readonly int CamForward = Shader.PropertyToID("cam_forward");
    private static readonly int CamRight = Shader.PropertyToID("cam_right");
    private static readonly int CamUp = Shader.PropertyToID("cam_up");
    private static readonly int CamFov = Shader.PropertyToID("cam_fov");

    [Header("References")]
    public Camera mainCamera;
    [FormerlySerializedAs("raymarchMaterial")] public Material rayMarchMaterial;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Mouse rotation")]
    public float mouseSensitivity = 2.0f;
    public float minPitch = -85f;  // límite de rotación vertical
    public float maxPitch = 85f;

    private float _yaw;   // rotación horizontal (Y)
    private float _pitch; // rotación vertical (X)

    private void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;

        Vector3 angles = mainCamera!.transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;
        

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!rayMarchMaterial)
        {
            Debug.LogError("Assign correct shader material to the sphere field camera controller");
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleMouseRotation();
        UpdateShaderCameraParams();
    }

    // --------------------------------------------------------
    // Mueve la cámara con WASD + QE (ejes locales)
    // --------------------------------------------------------
    private void HandleMovement()
    {
        // 1) Lectura de ejes
        float horizontal = Input.GetAxis("Horizontal"); // A/D o flechas izq/der
        float vertical   = Input.GetAxis("Vertical");   // W/S o flechas arriba/abajo

        Vector3 forwardFlat = mainCamera.transform.forward;
        forwardFlat.Normalize();

        Vector3 rightFlat = mainCamera.transform.right;
        rightFlat.Normalize();

        Vector3 move = rightFlat * horizontal + forwardFlat * vertical;

        mainCamera.transform.position += move.normalized * (moveSpeed * Time.deltaTime);

        // 3) Subir/bajar con Q/E
        if (Input.GetKey(KeyCode.E))
            mainCamera.transform.position += Vector3.up * (moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.Q))
            mainCamera.transform.position += Vector3.down * (moveSpeed * Time.deltaTime);
    }

    private void HandleMouseRotation()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _yaw += mouseX;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        // Aplicar rotación
        mainCamera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    // --------------------------------------------------------
    // Envía posición y vectores de la cámara al shader cada frame
    // --------------------------------------------------------
    private void UpdateShaderCameraParams()
    {
        if (!rayMarchMaterial) return;

        // 1) Posición de la cámara
        Vector3 camPos = mainCamera.transform.position;
        rayMarchMaterial.SetVector(CamPos, new Vector4(camPos.x, camPos.y, camPos.z, 0));

        // 2) Vector “forward” en world space
        Vector3 forward = mainCamera.transform.forward.normalized;
        rayMarchMaterial.SetVector(CamForward, new Vector4(forward.x, forward.y, forward.z, 0));

        // 3) Vector “right” en world space
        Vector3 right = mainCamera.transform.right.normalized;
        rayMarchMaterial.SetVector(CamRight, new Vector4(right.x, right.y, right.z, 0));

        // 4) Vector “up” en world space
        Vector3 up = -mainCamera.transform.up.normalized;
        rayMarchMaterial.SetVector(CamUp, new Vector4(up.x, up.y, up.z, 0));

        // 5) Field of view
        float fov = mainCamera.fieldOfView;
        rayMarchMaterial.SetFloat(CamFov, fov);
    }
}
