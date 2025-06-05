Shader "Custom/2D/GradientSpriteLit"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ColorA ("Start Color", Color) = (1, 0, 0, 1)
        _ColorB ("End Color", Color) = (0, 0, 1, 1)
        _GradientSize ("Gradient Size (Local Z Units)", Float) = 1.0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 300

        Pass
        {
            Name "Main"
            Tags { "LightMode" = "Universal2D" }
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color;
            float4 _ColorA;
            float4 _ColorB;
            float _GradientSize;
            float4 _RendererColor;

            // Lighting system inputs
            TEXTURE2D(_ShapeLightTexture0); SAMPLER(sampler_ShapeLightTexture0);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 texColor = tex2D(_MainTex, IN.uv);

                float t = saturate(IN.positionWS.z / _GradientSize);
                float4 gradient = lerp(_ColorA, _ColorB, t);

                // Sample the global 2D lighting
                float2 lightUV = IN.positionHCS.xy * 0.5 + 0.5;
                float4 lighting = SAMPLE_TEXTURE2D(_ShapeLightTexture0, sampler_ShapeLightTexture0, lightUV);

                float4 finalColor = texColor * _Color * gradient * _RendererColor * lighting;

                return finalColor;
            }
            ENDHLSL
        }
    }
}
