Shader "Custom/2DUnlitBurn_TiledSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _MainTex_ST ("Tiling/Offset", Vector) = (1,1,0,0)
        _BurnAmount ("Burn Amount", Range(0,1)) = 0
        _BurnColor ("Burn Color", Color) = (1,0.3,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float localY : TEXCOORD1; // normalized Y of vertex in object space
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BurnAmount;
            float4 _BurnColor;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Assuming the sprite quad spans roughly from -0.5 to +0.5 in Y in local space
                // Normalize Y from [minY, maxY] to [0,1]
                // Here assuming sprite quad is -0.5 to 0.5 Y:
                float normalizedY = saturate(v.positionOS.y + 0.5);

                o.localY = normalizedY;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv);

                // Burn based on normalized Y and burn amount
                if (i.localY < _BurnAmount)
                {
                    // burned area: show burn color
                    return _BurnColor;
                }
                else
                {
                    // fading alpha above burn line, smooth step for nicer transition
                    float alphaFade = smoothstep(_BurnAmount, _BurnAmount + 0.1, i.localY);
                    texColor.a *= alphaFade;
                    return texColor;
                }
            }
            ENDHLSL
        }
    }
}
