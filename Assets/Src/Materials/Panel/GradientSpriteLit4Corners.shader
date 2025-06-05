Shader "Custom/2D/GradientSpriteLit4Corners"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _ColorTL ("Top Left Color", Color) = (1, 0, 0, 1)
        _ColorTR ("Top Right Color", Color) = (0, 1, 0, 1)
        _ColorBL ("Bottom Left Color", Color) = (0, 0, 1, 1)
        _ColorBR ("Bottom Right Color", Color) = (1, 1, 0, 1)

        _GradientSize ("Edge Gradient Size", Float) = 0.1
        _GradientCurve ("Gradient Curve", Float) = 1.0

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
                float2 posOS : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color;
            float4 _ColorTL, _ColorTR, _ColorBL, _ColorBR;
            float _GradientSize;
            float _GradientCurve;
            float4 _RendererColor;

            TEXTURE2D(_ShapeLightTexture0); SAMPLER(sampler_ShapeLightTexture0);

            float2 _MinPosOS;
            float2 _MaxPosOS;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.posOS = IN.positionOS.xy;
                return OUT;
            }

            float4 EdgeGradient(float2 posOS)
            {
                float2 size = max(_MaxPosOS - _MinPosOS, float2(0.0001, 0.0001));
                float2 uv = saturate((posOS - _MinPosOS) / size); // 0–1 across the sprite

                // Distance to closest edge
                float2 edgeDist = min(uv, 1.0 - uv); // min distance to horizontal/vertical edge
                float edgeFactor = saturate(edgeDist.x / _GradientSize);
                edgeFactor = min(edgeFactor, saturate(edgeDist.y / _GradientSize));

                // Edge gradient blend factor
                float t = 1.0 - pow(edgeFactor, _GradientCurve); // 1 near edge, 0 in center

                float4 top = lerp(_ColorTL, _ColorTR, uv.x);
                float4 bottom = lerp(_ColorBL, _ColorBR, uv.x);
                float4 edgeColor = lerp(bottom, top, uv.y);

                return lerp(float4(1,1,1,1), edgeColor, t); // white = no gradient when t = 0
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 texColor = tex2D(_MainTex, IN.uv);
                float4 gradient = EdgeGradient(IN.posOS);

                float2 lightUV = IN.positionHCS.xy * 0.5 + 0.5;
                float4 lighting = SAMPLE_TEXTURE2D(_ShapeLightTexture0, sampler_ShapeLightTexture0, lightUV);

                float4 finalColor = texColor * _Color * gradient * _RendererColor * lighting;
                return finalColor;
            }
            ENDHLSL
        }
    }
}
