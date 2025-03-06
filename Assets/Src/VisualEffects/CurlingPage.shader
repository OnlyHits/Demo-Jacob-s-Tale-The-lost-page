Shader "Custom/PageCurlWithPivot"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CurlIntensity ("Curl Intensity", Range(0, 1)) = 0.5
        _CurlAngle ("Curl Angle", Range(0, 3.14)) = 1.0
        _Rotation ("Rotation", Range(0, 6.28)) = 0.0
        _Pivot ("Curl Pivot", Vector) = (-1, 0, 0, 0)  // Default pivot at left-center
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _CurlIntensity;
            float _CurlAngle;
            float _Rotation;
            float4 _Pivot;  // Pivot point for curling (passed from Unity)

            v2f vert (appdata_t v)
            {
                v2f o;

                // Shift the vertex to pivot space (use the passed pivot)
                float x = v.vertex.x - _Pivot.x;
                float y = v.vertex.y - _Pivot.y;

                // Compute curl deformation relative to pivot
                float R = sqrt(x * x + y * y);
                float r = R * sin(_CurlAngle);
                float beta = asin(x / R) / sin(_CurlAngle);

                float v1_x = r * sin(beta);
                float v1_y = R - r * (1 - cos(beta)) * sin(_CurlAngle);
                float v1_z = r * (1 - cos(beta)) * cos(_CurlAngle);

                // Rotate around Y-axis
                float new_x = v1_x * cos(_Rotation) - v1_z * sin(_Rotation);
                float new_z = v1_x * sin(_Rotation) + v1_z * cos(_Rotation);

                // Apply transformation and move back to world space
                v.vertex.x = lerp(v.vertex.x, new_x + _Pivot.x, _CurlIntensity);
                v.vertex.y = lerp(v.vertex.y, v1_y + _Pivot.y, _CurlIntensity);
                v.vertex.z = lerp(v.vertex.z, new_z, _CurlIntensity);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
