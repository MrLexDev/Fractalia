// Runtime/ParamsUI/Debug/ParamUIDebugShaderDemo.cs
using System;
using ParamsUI;
using ParamsUI.UITK;
using UnityEngine;

public sealed class ParamUIDebugDemo : MonoBehaviour
{
    public ParamWindow window;
    public ShaderParamBinderMaterial binder;

    SphereFieldCameraController _controller;

    void Start()
    {
        var cat = new ParamCatalog();
        int fractalCount = Enum.GetValues(typeof(FractalType)).Length;
        _controller = binder != null ? binder.targetMaterialHolder : null;

        if (_controller != null)
        {
            cat.Add(new ParamDef<SphereFieldCameraController.CameraMode>("camera_mode", "Camera Mode",
                    () => _controller.Mode,
                    v => _controller.Mode = v)
                .InGroup("Camera"));

            var freeSpeedMeta = ParamMeta.Range(0.1, 100, 0.1);
            freeSpeedMeta.Unit = "m/s";

            cat.Add(new ParamDef<float>("camera_free_move_speed", "Free Move Speed",
                    () => _controller.MoveSpeed,
                    v => _controller.MoveSpeed = v)
                .InGroup("Camera")
                .WithMeta(freeSpeedMeta));

            var orbitDistanceMeta = ParamMeta.Range(0.1, 10, 0.1);

            cat.Add(new ParamDef<float>("camera_orbit_distance", "Orbit Distance",
                    () => _controller.OrbitDistance,
                    v => _controller.OrbitDistance = v)
                .InGroup("Camera")
                .WithMeta(orbitDistanceMeta));

            var orbitSpeedMeta = ParamMeta.Range(-360, 360, 1);
            orbitSpeedMeta.Unit = "deg/s";

            cat.Add(new ParamDef<float>("camera_orbit_speed", "Orbit Speed",
                    () => _controller.OrbitSpeed,
                    v => _controller.OrbitSpeed = v)
                .InGroup("Camera")
                .WithMeta(orbitSpeedMeta));
        }

        // ── Cámara ────────────────────────────────────────────────────────
        cat.Add(new ParamDef<float>("cam_fov", "Camera FOV",
            () => binder.targetMaterialHolder.mainCamera.fieldOfView,
            v  => binder.targetMaterialHolder.mainCamera.fieldOfView = v)
            .InGroup("Camera")
            .WithMeta(ParamMeta.Range(1, 160, 1)));

        cat.Add(new ParamDef<FractalType>("fractal_type", "Fractal Type",
            () => (FractalType)Mathf.Clamp(binder.GetInt("fractal_type"), 0, fractalCount - 1),
            v  => binder.SetInt("fractal_type", (int)v))
            .InGroup("Fractal"));

        cat.Add(new ParamDef<float>("power", "Power",
            () => binder.GetFloat("power"),
            v  => binder.SetFloat("power", Mathf.Clamp(v, -12f, 12f)))
            .InGroup("Fractal")
            .WithMeta(ParamMeta.Range(-12, 12, 0.1)));

        // ── Mandelbulb params (MB_*) ──────────────────────────────────────
        cat.Add(new ParamDef<float>("MB_MIN_RADIUS", "MB Min Radius",
            () => binder.GetFloat("MB_MIN_RADIUS"),
            v  => binder.SetFloat("MB_MIN_RADIUS", Mathf.Max(0f, v)))
            .InGroup("Fractal")
            .WithMeta(ParamMeta.Range(0, 12, 0.1)));

        cat.Add(new ParamDef<float>("MB_FIXED_RADIUS", "MB Fixed Radius",
            () => binder.GetFloat("MB_FIXED_RADIUS"),
            v  => binder.SetFloat("MB_FIXED_RADIUS", Mathf.Max(0f, v)))
            .InGroup("Fractal")
            .WithMeta(ParamMeta.Range(0, 12, 0.1)));

        cat.Add(new ParamDef<float>("MB_BOX_LIMIT", "MB Box Limit",
            () => binder.GetFloat("MB_BOX_LIMIT"),
            v  => binder.SetFloat("MB_BOX_LIMIT", Mathf.Max(0f, v)))
            .InGroup("Fractal")
            .WithMeta(ParamMeta.Range(0, 12, 0.1)));

        // ── Luz / Orbit ───────────────────────────────────────────────────
        var lightSliderMeta = new ParamMeta()
            .WithVectorSlider(-1, 1, 0.01, "X", "Y", "Z");

        cat.Add(new ParamDef<Vector3>("light_direction", "Light Dir (xyz)",
            () => binder.GetVector("light_direction"),
            v  => binder.SetVector("light_direction", v))
            .InGroup("Lighting")
            .WithMeta(lightSliderMeta));

        var colorSliderMeta = new ParamMeta().WithColorSlider(0, 1, 0.01);

        cat.Add(new ParamDef<Color>("base_color", "Base Color",
            () => binder.GetColor("base_color"),
            v  => binder.SetColor("base_color", v))
            .InGroup("Look")
            .WithMeta(colorSliderMeta));

        cat.Add(new ParamDef<Color>("orbit_color_0", "Orbit Color 0",
            () => binder.GetColor("orbit_color_0"),
            v  => binder.SetColor("orbit_color_0", v))
            .InGroup("Look")
            .WithMeta(colorSliderMeta));

        cat.Add(new ParamDef<Color>("orbit_color_1", "Orbit Color 1",
            () => binder.GetColor("orbit_color_1"),
            v  => binder.SetColor("orbit_color_1", v))
            .InGroup("Look")
            .WithMeta(colorSliderMeta));

        cat.Add(new ParamDef<Color>("orbit_color_2", "Orbit Color 2",
            () => binder.GetColor("orbit_color_2"),
            v  => binder.SetColor("orbit_color_2", v))
            .InGroup("Look")
            .WithMeta(colorSliderMeta));

        cat.Add(new ParamDef<Color>("orbit_color_3", "Orbit Color 3",
            () => binder.GetColor("orbit_color_3"),
            v  => binder.SetColor("orbit_color_3", v))
            .InGroup("Look")
            .WithMeta(colorSliderMeta));

        var orbitSliderMeta = new ParamMeta()
            .WithVectorSlider(0, 1, 0.01, "X", "Y", "Z");
        
        cat.Add(new ParamDef<Vector4>("orbit_thresholds", "Orbit Thresholds",
            () => binder.GetVector("orbit_thresholds"),
            v  => binder.SetVector("orbit_thresholds", v))
            .InGroup("Look")
            .WithMeta(orbitSliderMeta));

        cat.Add(new ParamDef<float>("orbit_scale", "Orbit Scale",
            () => binder.GetFloat("orbit_scale"),
            v  => binder.SetFloat("orbit_scale", v))
            .InGroup("Look")
            .WithMeta(ParamMeta.Range(0, 3, 0.1)));

        // ── AO / Debug ────────────────────────────────────────────────────
        cat.Add(new ParamDef<float>("ao_brightness", "AO Brightness",
            () => binder.GetFloat("ao_brightness"),
            v  => binder.SetFloat("ao_brightness", Mathf.Max(0f, v)))
            .InGroup("Shading")
            .WithMeta(ParamMeta.Range(0, 30, 0.1)));

        window.Catalog = cat;
        window.Rebuild();

        if (_controller != null)
        {
            _controller.ModeChanged += OnCameraModeChanged;
            OnCameraModeChanged(_controller.Mode);
        }
    }

    void OnDestroy()
    {
        if (_controller != null)
        {
            _controller.ModeChanged -= OnCameraModeChanged;
        }
    }

    void OnCameraModeChanged(SphereFieldCameraController.CameraMode mode)
    {
        if (window != null)
        {
            window.Visible = mode != SphereFieldCameraController.CameraMode.Free;
        }
    }
}
