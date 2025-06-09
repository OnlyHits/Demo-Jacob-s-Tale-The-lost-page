Shader "Custom/PageFoldShader_DoubleSided"
{
    Properties
    {
        _FrontTex ("Front Texture", 2D) = "white" {}
        _BackTex ("Back Texture", 2D) = "white" {}
        _FoldProgress ("Fold Progress", Range(0, 1)) = 0
        _CurveStrength ("Curve Strength", Float) = 1
        _FoldRightSide ("Fold Right Side", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        Cull Off // Important: Render both sides

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _FrontTex;
            sampler2D _BackTex;
            float4 _FrontTex_ST;
            float4 _BackTex_ST;
            float _FoldProgress;
            float _CurveStrength;
            float _FoldRightSide;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 basePos = IN.positionOS.xyz;
                float3 foldedPos = basePos;

                float progress = saturate(_FoldProgress);
                float foldAmount = sin(progress * 3.14159265);

                if (progress > 0)
                {
                    bool foldRight = (_FoldRightSide > 0.5);
                    float xNorm = IN.uv2.x;

                    float hingeX = foldRight ? 0.5 : -0.5;
                    float distFromHinge = foldRight ? (1.0 - xNorm) : xNorm;
                    float compressFactor = foldAmount * distFromHinge;
                    foldedPos.x = lerp(basePos.x, hingeX, compressFactor);

                    float bendZ = sin(xNorm * 3.14159265) * -foldAmount * _CurveStrength;
                    foldedPos.z += bendZ;
                }

                float3 finalPos = lerp(basePos, foldedPos, progress);

                OUT.positionHCS = TransformObjectToHClip(finalPos);
                OUT.uv = TRANSFORM_TEX(IN.uv, _FrontTex);

                return OUT;
            }

            float4 frag(Varyings IN, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                float4 frontCol = tex2D(_FrontTex, IN.uv);
                float4 backCol = tex2D(_BackTex, IN.uv);
                return isFrontFace ? frontCol : backCol;
            }

            ENDHLSL
        }
    }
}
