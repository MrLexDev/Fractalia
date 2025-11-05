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
        surface_epsilon_far ("Surface Epsilon Far", Float)  = 0.005
        normal_delta_far    ("Normal Delta Far",    Float)  = 0.003
        lod_near_distance   ("LOD Near Distance",   Float)  = 2
        lod_far_distance    ("LOD Far Distance",    Float)  = 25
        bailout             ("Bailout",             Int)    = 4
        ao_brightness       ("AO Brightness",       Float)  = 0.1
        
        MB_SCALE            ("MB_SCALE",            Float)    = 2
        MB_MIN_RADIUS       ("MB_MIN_RADIUS",       Float)    = 0.5
        MB_FIXED_RADIUS     ("MB_FIXED_RADIUS",     Float)    = 1
        MB_BOX_LIMIT        ("MB_BOX_LIMIT",        Float)    = 1

        julia_constant      ("Julia Constant",      Vector)   = (-0.745, 0.113, 0.274)

        menger_bounds       ("Menger Bounds",       Vector)   = (1, 1, 1, 0)
        menger_cross_thickness ("Menger Cross Thickness", Float) = 1

        iterations          ("Iterations",          Int)    = 10
        power               ("Power",               Float)  = 4.0
        fractal_type        ("Fractal Type",        Int)    = 0

        sierpinski_iterations    ("Sierpinski Iterations",    Int)   = 15
        sierpinski_scale         ("Sierpinski Scale",         Float) = 2
        sierpinski_offset_vec    ("Sierpinski Offset",        Vector)= (1, 1, 1, 0)
        sierpinski_strut_thickness("Sierpinski Strut Thickness", Float) = 1.2

        light_direction     ("Light Direction",     Vector) = (0, 0, 0)

        orbit_color_0       ("Orbit Color 0",       Color)  = (0.2, 0.2, 0.8, 1)
        orbit_color_1       ("Orbit Color 1",       Color)  = (0.2, 0.8, 0.2, 1)
        orbit_color_2       ("Orbit Color 2",       Color)  = (0.8, 0.8, 0.2, 1)
        orbit_color_3       ("Orbit Color 3",       Color)  = (0.8, 0.2, 0.2, 1)
        orbit_thresholds    ("Orbit Thresholds",    Vector) = (0.3, 0.6, 0.9, 0)
        orbit_scale         ("Orbit Scale",         Float)  = 1.0
        
        debug_steps         ("Debug Ray Steps",     Float)  = 0.0
    }

    SubShader
    {
        ////Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        //Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        //LOD 100
        Tags{ "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-1" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Name "RaymarchFS"
            Tags { "LightMode"="UniversalForward" } // O el LightMode apropiado

            HLSLPROGRAM
            #pragma target 3.0
            #pragma multi_compile _ _DEBUG_OVERLAY
            
            #pragma vertex vertex_full_screen_triangle
            #pragma fragment fragment_render_fractal

            #include "FullScreenTriangle_VS.hlsl" 
            #include "RayMarch_FS.hlsl"
            
            ENDHLSL
        }
    }
    FallBack Off
}