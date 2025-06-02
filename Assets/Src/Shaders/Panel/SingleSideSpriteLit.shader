Shader "Custom/SingleSideSpriteLit"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        LOD 200
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            // URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Color;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 normalOS = float3(0, 0, -1); // Sprite forward vector

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = posInputs.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color;

                OUT.normalWS = TransformObjectToWorldNormal(normalOS);
                OUT.positionWS = posInputs.positionWS;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                Light mainLight = GetMainLight();

                // Calculate diffuse light contribution
                float NdotL = saturate(dot(IN.normalWS, mainLight.direction));
                half3 diffuse = texColor.rgb * mainLight.color.rgb * NdotL;

                // Add ambient lighting from SH (spherical harmonics) ambient probe
                half3 ambient = SAMPLE_AMBIENT_PROBE(IN.normalWS) * texColor.rgb;

                half3 litColor = diffuse + ambient;

                return half4(litColor, texColor.a);
            }
            ENDHLSL
        }
    }
}
