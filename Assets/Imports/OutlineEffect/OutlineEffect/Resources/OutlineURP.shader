Shader "Outline/URP Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Range(0.0, 0.5)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineThickness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                float3 expandedPosition = IN.positionOS.xyz + normalize(IN.normalOS) * _OutlineThickness;
                OUT.positionHCS = TransformObjectToHClip(expandedPosition);
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
