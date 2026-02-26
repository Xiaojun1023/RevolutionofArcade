Shader "UI/CRT_UI_Overlay"
{
    Properties
    {
        _Intensity ("Overlay Intensity", Range(0,1)) = 0.6
        _GreenTint ("Green Tint", Range(0,2)) = 1.0
        _ScanlineDensity ("Scanline Density", Range(100,1200)) = 450
        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.35
        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.12
        _WaveStrength ("Wave Strength", Range(0,0.02)) = 0.006
        _WaveSpeed ("Wave Speed", Range(0,10)) = 2.0
        _Flicker ("Flicker", Range(0,1)) = 0.08
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Intensity, _GreenTint;
            float _ScanlineDensity, _ScanlineStrength;
            float _NoiseStrength;
            float _WaveStrength, _WaveSpeed;
            float _Flicker;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _Time.y;

                float wave = sin((i.uv.y * 10.0 + t * _WaveSpeed) * 6.28318) * _WaveStrength;
                float2 uv = i.uv;
                uv.x += wave;

                float scan = sin((uv.y * _ScanlineDensity) * 6.28318);
                scan = smoothstep(-0.2, 0.8, scan);
                float scanDarken = lerp(1.0, scan, _ScanlineStrength);

                float n = hash21(uv * (800 + t * 10.0));
                float noise = (n - 0.5) * 2.0; // -1..1

                float flick = 1.0 + (sin(t * 60.0) * 0.5 + 0.5) * _Flicker;

                float3 green = float3(0.1, 1.0, 0.18) * _GreenTint;

                float alpha = _Intensity;
                alpha *= (0.75 + 0.25 * scan);
                alpha *= (1.0 + noise * _NoiseStrength);
                alpha = saturate(alpha);

                float3 col = green * scanDarken * flick;

                return float4(col, alpha);
            }
            ENDCG
        }
    }
}
