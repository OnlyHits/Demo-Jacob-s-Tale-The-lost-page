Shader "Custom/ScreenSpaceOutlineSprite"
{
    Properties
    {
        _MainTex ("Main Sprite", 2D) = "white" {}
        _OutlineTex ("Outline Sprite", 2D) = "white" {}
        _OutlineSize ("Outline Size (pixels)", Float) = 1
        _Threshold ("Alpha Threshold", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _OutlineTex;
            float4 _OutlineTex_ST;

            float _OutlineSize;
            float _Threshold;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.screenPos = ComputeScreenPos(o.pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 col = tex2D(_MainTex, uv);
                float alpha = col.a;

                if (alpha > _Threshold)
                    return col;

                // screen-space offset (in pixels to UVs)
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float2 pixelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
                float2 offset = _OutlineSize * pixelSize;

                float outlineAlpha = 0;

                // Sample 8 surrounding pixels
                float2 dirs[8] = {
                    float2( 1, 0), float2(-1, 0),
                    float2( 0, 1), float2( 0,-1),
                    float2( 1, 1), float2(-1, 1),
                    float2( 1,-1), float2(-1,-1)
                };

                for (int j = 0; j < 8; ++j)
                {
                    float2 offsetUV = uv + dirs[j] * offset;
                    outlineAlpha += tex2D(_MainTex, offsetUV).a;
                }

                if (outlineAlpha > 0)
                {
                    // Use animated outline sprite
                    float4 outlineCol = tex2D(_OutlineTex, uv);
                    outlineCol.a *= step(0.001, outlineCol.a); // avoid noise
                    return outlineCol;
                }

                return 0;
            }

            ENDHLSL
        }
    }
}
