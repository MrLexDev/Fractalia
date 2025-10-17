using UnityEngine;
using UnityEngine.Serialization;

public class SphereFieldCameraController : MonoBehaviour
{
    public enum CameraMode
    {
        Free,
        Orbit
    }
    
    private static readonly int CamPos = Shader.PropertyToID("cam_pos");
    private static readonly int CamForward = Shader.PropertyToID("cam_forward");
    private static readonly int CamRight = Shader.PropertyToID("cam_right");
    private static readonly int CamUp = Shader.PropertyToID("cam_up");
    private static readonly int CamFov = Shader.PropertyToID("cam_fov");

    [Header("References")]
    public Camera mainCamera;
    [FormerlySerializedAs("raymarchMaterial")] public Material rayMarchMaterial;

    [Header("Movement")]
    [SerializeField]
    private CameraMode cameraMode = CameraMode.Orbit;

    [SerializeField]
    private float moveSpeed = 5f;

    [Header("Orbit")]
    public Transform orbitTarget;

    [SerializeField]
    private Vector3 orbitCenter = Vector3.zero;

    [SerializeField]
    private float orbitDistance = 10f;

    [SerializeField]
    private float orbitSpeed = 20f;

    [Header("Mouse rotation")]
    public float mouseSensitivity = 2.0f;
    public float minPitch = -85f;  // límite de rotación vertical
    public float maxPitch = 85f;

    private float _yaw;   // rotación horizontal (Y)
    private float _pitch; // rotación vertical (X)
    private float _orbitAngle;

    private void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;

        Vector3 angles = mainCamera!.transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;
        

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        if (!rayMarchMaterial)
        {
            Debug.LogError("Assign correct shader material to the sphere field camera controller");
        }
        
        if (cameraMode == CameraMode.Orbit)
        {
            Vector3 center = GetOrbitCenter();
            Vector3 toCamera = mainCamera.transform.position - center;
            if (toCamera.sqrMagnitude > 0.0001f)
            {
                orbitDistance = toCamera.magnitude;
                _orbitAngle = Mathf.Atan2(toCamera.z, toCamera.x) * Mathf.Rad2Deg;
            }
        }
    }

    private void Update()
    {
        if (cameraMode == CameraMode.Free)
        {
            HandleMovement();
            HandleMouseRotation();
        }
        else
        {
            HandleOrbit();
        }
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

        if (move.sqrMagnitude > 0f)
        {
            mainCamera.transform.position += move.normalized * (moveSpeed * Time.deltaTime);
        }
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
        _pitch += mouseY * -1;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        // Aplicar rotación
        mainCamera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }
    
    private void HandleOrbit()
    {
        Vector3 center = GetOrbitCenter();
        if (orbitDistance < 0.01f)
        {
            orbitDistance = 0.01f;
        }

        _orbitAngle += orbitSpeed * Time.deltaTime;
        float angleRad = _orbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * orbitDistance;
        mainCamera.transform.position = center + offset;
        mainCamera.transform.LookAt(center, Vector3.up);
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
        Vector3 up = mainCamera.transform.up.normalized;
        if (SystemInfo.graphicsUVStartsAtTop)
        {
            up = -up;
        }
        rayMarchMaterial.SetVector(CamUp, new Vector4(up.x, up.y, up.z, 0));

        // 5) Field of view
        float fov = mainCamera.fieldOfView;
        rayMarchMaterial.SetFloat(CamFov, fov);
    }
    private Vector3 GetOrbitCenter()
    {
        if (orbitTarget != null)
        {
            return orbitTarget.position;
        }

        return orbitCenter;
    }

    public CameraMode Mode
    {
        get => cameraMode;
        set
        {
            if (cameraMode == value) return;

            cameraMode = value;

            if (cameraMode == CameraMode.Free)
            {
                Vector3 angles = mainCamera.transform.eulerAngles;
                _yaw = angles.y;
                _pitch = angles.x;
            }
            else
            {
                Vector3 center = GetOrbitCenter();
                Vector3 toCamera = mainCamera.transform.position - center;
                if (toCamera.sqrMagnitude > 0.0001f)
                {
                    orbitDistance = toCamera.magnitude;
                    _orbitAngle = Mathf.Atan2(toCamera.z, toCamera.x) * Mathf.Rad2Deg;
                }
            }
        }
    }

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    public float OrbitDistance
    {
        get => orbitDistance;
        set => orbitDistance = Mathf.Max(0.01f, value);
    }

    public float OrbitSpeed
    {
        get => orbitSpeed;
        set => orbitSpeed = value;
    }

    public Vector3 OrbitCenter
    {
        get => orbitCenter;
        set => orbitCenter = value;
    }
}
