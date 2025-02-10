Shader "Raytracing/Anyhit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {
            Name "Raytracing"
            Tags{ "LightMode" = "Raytracing" }

            HLSLPROGRAM

            #include "Common.hlsl"

            #pragma raytracing test

            #pragma enable_ray_tracing_shader_debug_symbols

            struct AttributeData
            {
                float2 barycentrics;
            };

            [shader("anyhit")]
            void AnyHit(inout RayPayload payload, in AttributeData attribs)
            {
                if (payload.count == 0)
                {
                    //payload.data[payload.count] = 0.0f;
                    payload.data[0] = 0.0f;
                    payload.count = 1;
                }

                IgnoreHit();
                //AcceptHitAndEndSearch();
            }

            ENDHLSL
        }
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 5.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
}
