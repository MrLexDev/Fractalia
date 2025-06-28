Shader "Custom/RayMarchRenderer"
{
    Properties
    {
        cam_pos             ("Camera Position",     Vector) = (0,0,-5,0)
        cam_forward         ("Camera Forward",      Vector) = (0,0,1,0)
        cam_right           ("Camera Right",        Vector) = (1,0,0,0)
        cam_up              ("Camera Up",           Vector) = (0,1,0,0)
        cam_fov             ("Camera FOV",          Float)  = 60

        base_color          ("Sphere Base Color",   Color)  = (0.2, 0.6, 1.0, 1.0)
        max_ray_distance    ("Max Ray Distance",    Float)  = 100.0
        surface_epsilon     ("Surface Epsilon",     Float)  = 0.001
        max_steps           ("Max Steps",           Int)    = 64
        normal_delta        ("Normal Delta",        Float)  = 0.0005
        bailout             ("Bailout",             Int)    = 4
        
        MB_SCALE            ("MB_SCALE",            Float)    = 2
        MB_MIN_RADIUS       ("MB_MIN_RADIUS",       Float)    = 0.5
        MB_FIXED_RADIUS     ("MB_FIXED_RADIUS",     Float)    = 1
        MB_BOX_LIMIT        ("MB_BOX_LIMIT",        Float)    = 1
        
        iterations          ("Iterations",          Int)    = 10
        power               ("Power",               Float)  = 4.0
        g_Scale             ("Scale",               Float)  = 1.0
        
        light_direction     ("Light Direction",     Vector) = (0, 0, 0)

        orbit_color_0       ("Orbit Color 0",       Color)  = (0.2, 0.2, 0.8, 1)
        orbit_color_1       ("Orbit Color 1",       Color)  = (0.2, 0.8, 0.2, 1)
        orbit_color_2       ("Orbit Color 2",       Color)  = (0.8, 0.8, 0.2, 1)
        orbit_color_3       ("Orbit Color 3",       Color)  = (0.8, 0.2, 0.2, 1)
        orbit_thresholds    ("Orbit Thresholds",    Vector) = (0.3, 0.6, 0.9, 0)
        orbit_scale         ("Orbit Scale",         Float)  = 1.0
    }

    SubShader
    {
        //Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Name "InfiniteSphereFieldPass"
            Tags { "LightMode"="UniversalForward" } // O el LightMode apropiado

            HLSLPROGRAM
            #pragma vertex vertex_full_screen_triangle
            #pragma fragment fragment_render_fractal

            #include "FullScreenTriangle_VS.hlsl" 
            #include "RayMarch_FS.hlsl"
            
            ENDHLSL
        }
    }
    FallBack Off
}