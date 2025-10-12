// Runtime/ParamsUI/Debug/ParamUIDebugShaderDemo.cs
using ParamsUI;
using ParamsUI.UITK;
using UnityEngine;

public sealed class ParamUIDebugDemo : MonoBehaviour
{
    public ParamWindow window;
    public ShaderParamBinderMaterial binder;

    void Start()
    {
        var cat = new ParamCatalog();

        // ── Cámara ────────────────────────────────────────────────────────
        cat.Add(new ParamDef<float>("cam_fov", "Camera FOV",
            () => binder.GetFloat("cam_fov"),
            v  => binder.SetFloat("cam_fov", Mathf.Clamp(v, 1f, 160f)))
            .InGroup("Camera")
            .WithMeta(ParamMeta.Range(1, 160, 1)));

        cat.Add(new ParamDef<Vector4>("cam_pos", "Camera Pos (xyz)",
            () => binder.GetVector("cam_pos"),
            v  => binder.SetVector("cam_pos", v))
            .InGroup("Camera"));

        cat.Add(new ParamDef<Vector4>("cam_forward", "Camera Forward",
            () => binder.GetVector("cam_forward"),
            v  => binder.SetVector("cam_forward", v))
            .InGroup("Camera"));

        // ── Raymarch tuning ───────────────────────────────────────────────
        cat.Add(new ParamDef<float>("max_ray_distance", "Max Ray Distance",
            () => binder.GetFloat("max_ray_distance"),
            v  => binder.SetFloat("max_ray_distance", Mathf.Max(0f, v)))
            .InGroup("Raymarch")
            .WithMeta(ParamMeta.Range(1, 500, 1)));

        cat.Add(new ParamDef<float>("surface_epsilon", "Surface Epsilon",
            () => binder.GetFloat("surface_epsilon"),
            v  => binder.SetFloat("surface_epsilon", Mathf.Max(1e-6f, v)))
            .InGroup("Raymarch")
            .WithMeta(ParamMeta.Range(0.000001, 0.01, 0.000001)));

        cat.Add(new ParamDef<int>("max_steps", "Max Steps",
            () => binder.GetInt("max_steps"),
            v  => binder.SetInt("max_steps", Mathf.Clamp(v, 1, 2048)))
            .InGroup("Raymarch")
            .WithMeta(ParamMeta.Range(1, 2048, 1)));

        cat.Add(new ParamDef<float>("normal_delta", "Normal Delta",
            () => binder.GetFloat("normal_delta"),
            v  => binder.SetFloat("normal_delta", Mathf.Max(1e-6f, v)))
            .InGroup("Raymarch")
            .WithMeta(ParamMeta.Range(0.000001, 0.02, 0.000001)));

        cat.Add(new ParamDef<float>("surface_epsilon_far", "Surface Epsilon Far",
            () => binder.GetFloat("surface_epsilon_far"),
            v  => binder.SetFloat("surface_epsilon_far", Mathf.Max(1e-6f, v)))
            .InGroup("Raymarch/Far"));

        cat.Add(new ParamDef<float>("normal_delta_far", "Normal Delta Far",
            () => binder.GetFloat("normal_delta_far"),
            v  => binder.SetFloat("normal_delta_far", Mathf.Max(1e-6f, v)))
            .InGroup("Raymarch/Far"));

        cat.Add(new ParamDef<float>("lod_near_distance", "LOD Near",
            () => binder.GetFloat("lod_near_distance"),
            v  => binder.SetFloat("lod_near_distance", Mathf.Max(0f, v)))
            .InGroup("Raymarch/LOD"));

        cat.Add(new ParamDef<float>("lod_far_distance", "LOD Far",
            () => binder.GetFloat("lod_far_distance"),
            v  => binder.SetFloat("lod_far_distance", Mathf.Max(0f, v)))
            .InGroup("Raymarch/LOD"));

        // ── Fractal core ──────────────────────────────────────────────────
        cat.Add(new ParamDef<int>("iterations", "Iterations",
            () => binder.GetInt("iterations"),
            v  => binder.SetInt("iterations", Mathf.Clamp(v, 1, 500)))
            .InGroup("Fractal")
            .WithMeta(ParamMeta.Range(1, 500, 1)));

        cat.Add(new ParamDef<float>("power", "Power",
            () => binder.GetFloat("power"),
            v  => binder.SetFloat("power", Mathf.Clamp(v, 1f, 12f)))
            .InGroup("Fractal")
            .WithMeta(ParamMeta.Range(1, 12, 0.1)));

        cat.Add(new ParamDef<float>("g_Scale", "Scale",
            () => binder.GetFloat("g_Scale"),
            v  => binder.SetFloat("g_Scale", v))
            .InGroup("Fractal"));

        // ── Mandelbulb params (MB_*) ──────────────────────────────────────
        cat.Add(new ParamDef<float>("MB_SCALE", "MB Scale",
            () => binder.GetFloat("MB_SCALE"),
            v  => binder.SetFloat("MB_SCALE", v))
            .InGroup("Fractal/MB"));

        cat.Add(new ParamDef<float>("MB_MIN_RADIUS", "MB Min Radius",
            () => binder.GetFloat("MB_MIN_RADIUS"),
            v  => binder.SetFloat("MB_MIN_RADIUS", Mathf.Max(0f, v)))
            .InGroup("Fractal/MB"));

        cat.Add(new ParamDef<float>("MB_FIXED_RADIUS", "MB Fixed Radius",
            () => binder.GetFloat("MB_FIXED_RADIUS"),
            v  => binder.SetFloat("MB_FIXED_RADIUS", Mathf.Max(0f, v)))
            .InGroup("Fractal/MB"));

        cat.Add(new ParamDef<float>("MB_BOX_LIMIT", "MB Box Limit",
            () => binder.GetFloat("MB_BOX_LIMIT"),
            v  => binder.SetFloat("MB_BOX_LIMIT", Mathf.Max(0f, v)))
            .InGroup("Fractal/MB"));

        // ── Luz / Orbit ───────────────────────────────────────────────────
        cat.Add(new ParamDef<Vector4>("light_direction", "Light Dir (xyz)",
            () => binder.GetVector("light_direction"),
            v  => binder.SetVector("light_direction", v))
            .InGroup("Lighting"));

        cat.Add(new ParamDef<Color>("base_color", "Base Color",
            () => binder.GetColor("base_color"),
            v  => binder.SetColor("base_color", v))
            .InGroup("Look"));

        cat.Add(new ParamDef<Color>("orbit_color_0", "Orbit Color 0",
            () => binder.GetColor("orbit_color_0"),
            v  => binder.SetColor("orbit_color_0", v))
            .InGroup("Look/Orbit"));

        cat.Add(new ParamDef<Color>("orbit_color_1", "Orbit Color 1",
            () => binder.GetColor("orbit_color_1"),
            v  => binder.SetColor("orbit_color_1", v))
            .InGroup("Look/Orbit"));

        cat.Add(new ParamDef<Color>("orbit_color_2", "Orbit Color 2",
            () => binder.GetColor("orbit_color_2"),
            v  => binder.SetColor("orbit_color_2", v))
            .InGroup("Look/Orbit"));

        cat.Add(new ParamDef<Color>("orbit_color_3", "Orbit Color 3",
            () => binder.GetColor("orbit_color_3"),
            v  => binder.SetColor("orbit_color_3", v))
            .InGroup("Look/Orbit"));

        cat.Add(new ParamDef<Vector4>("orbit_thresholds", "Orbit Thresholds",
            () => binder.GetVector("orbit_thresholds"),
            v  => binder.SetVector("orbit_thresholds", v))
            .InGroup("Look/Orbit"));

        cat.Add(new ParamDef<float>("orbit_scale", "Orbit Scale",
            () => binder.GetFloat("orbit_scale"),
            v  => binder.SetFloat("orbit_scale", v))
            .InGroup("Look/Orbit"));

        // ── AO / Debug ────────────────────────────────────────────────────
        cat.Add(new ParamDef<float>("ao_brightness", "AO Brightness",
            () => binder.GetFloat("ao_brightness"),
            v  => binder.SetFloat("ao_brightness", Mathf.Max(0f, v)))
            .InGroup("Shading"));

        cat.Add(new ParamDef<float>("debug_steps", "Debug Ray Steps",
            () => binder.GetFloat("debug_steps"),
            v  => binder.SetFloat("debug_steps", Mathf.Max(0f, v)))
            .InGroup("Debug"));

        window.Catalog = cat;
        window.Rebuild();
    }
}
