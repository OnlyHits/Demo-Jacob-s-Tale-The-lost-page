Shader "Custom/VignetteWholeSpriteByPosition"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _VignetteColor("Vignette Color", Color) = (0,0,0,1)
        _VignetteIntensity("Vignette Intensity", Range(0,1)) = 0.5
        _VignetteSmoothness("Vignette Smoothness", Range(0.01,1)) = 0.5
        _SpriteSize("Sprite Size (Width, Height)", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "LightMode" = "Universal2D" "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvWhole : TEXCOORD1;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _VignetteColor;
            float _VignetteIntensity;
            float _VignetteSmoothness;
            float2 _SpriteSize;

            TEXTURE2D(_ShapeLightTexture0); SAMPLER(sampler_ShapeLightTexture0);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.uvWhole = (IN.positionOS.xy + (_SpriteSize * 0.5)) / _SpriteSize;
                OUT.color = IN.color;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                float2 dist = IN.uvWhole - 0.5;
                float radius = length(dist) / 0.7071;

                // Vignette value: 0 at center, 1 at edges
                float vignette = smoothstep(_VignetteIntensity, _VignetteIntensity + _VignetteSmoothness, radius);

                // Apply color to the outer area (vignette color outside, original inside)
                float4 vignetteTint = lerp(float4(1,1,1,1), _VignetteColor, vignette);

                float2 lightUV = IN.positionCS.xy * 0.5 + 0.5;
                float4 lighting = SAMPLE_TEXTURE2D(_ShapeLightTexture0, sampler_ShapeLightTexture0, lightUV);

                float4 result = texColor * vignetteTint * lighting;
                result.rgb *= IN.color.rgb;
                result.a *= IN.color.a;

                return result;
            }
            ENDHLSL
        }
    }
}
