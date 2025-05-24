Shader "Custom/RayMarchRenderer"
{
    Properties
    {
        cam_pos             ("Camera Position",     Vector) = (0,0,-5,0)
        cam_forward         ("Camera Forward",      Vector) = (0,0,1,0)
        cam_right           ("Camera Right",        Vector) = (1,0,0,0)
        cam_up              ("Camera Up",           Vector) = (0,1,0,0)

        sphere_radius       ("Sphere Radius",       Float)  = 1.0
        cell_size           ("Repeat Cell Size",    Float)  = 4.0
        base_color          ("Sphere Base Color",   Color)  = (0.2, 0.6, 1.0, 1.0)
        max_ray_distance    ("Max Ray Distance",    Float)  = 100.0
        
        iterations          ("Iterations",          int)    = 10
        power               ("Power",               Float)  = 4.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "InfiniteSphereFieldPass"
            Tags { "LightMode"="UniversalForward" } // O el LightMode apropiado

            HLSLPROGRAM
            #pragma vertex vertex_full_screen_triangle
            #pragma fragment fragment_render_infinite_sphere_field

            #include "FullScreenTriangle_VS.hlsl" 
            #include "RayMarch_FS.hlsl"
            
            ENDHLSL
        }
    }
    FallBack Off
}