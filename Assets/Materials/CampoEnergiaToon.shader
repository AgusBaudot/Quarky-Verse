Shader "Custom/CampoEnergiaToon"
{
    Properties
    {
        [Header(Colores Estilo Toon)]
        [HDR] _EnergyColor ("Color Lineas de Energia", Color) = (0.0, 0.9, 1.0, 1.0)
        _GlassTint ("Color Fondo Transparente", Color) = (0.05, 0.35, 0.5, 0.12)
        
        [Header(Malla Hexagonal)]
        _HexSize ("Escala de Hexagonos", Float) = 1.2
        _BorderWidth ("Grosor de Lineas", Range(0.01, 0.25)) = 0.04
        _ToonCut ("Dureza de Borde Toon", Range(0.001, 0.05)) = 0.004
        
        [Header(Movimiento de Energia)]
        _Speed ("Velocidad de Ondas", Float) = 2.0
        _PulseScale ("Separacion de Pulsos", Float) = 0.6
        _RimPower ("Brillo en Bordes / Angulo", Range(0.5, 5.0)) = 2.2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            float4 _EnergyColor;
            float4 _GlassTint;
            float _HexSize;
            float _BorderWidth;
            float _ToonCut;
            float _Speed;
            float _PulseScale;
            float _RimPower;

            float HexDistance(float2 p)
            {
                p = abs(p);
                float c = dot(p, normalize(float2(1.0, 1.73205)));
                return max(c, p.x);
            }

            float GetHexGrid(float2 uv)
            {
                float2 r = float2(1.0, 1.73205);
                float2 h = r * 0.5;
                float2 a = fmod(abs(uv), r) - h;
                float2 b = fmod(abs(uv) - h, r) - h;
                float2 gv = dot(a, a) < dot(b, b) ? a : b;
                return 0.5 - HexDistance(gv);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Proyeccion automatica en paredes gigantes (sin deformarse al escalar)
                float2 wallPos = (abs(i.worldNormal.x) > abs(i.worldNormal.z)) 
                                 ? i.worldPos.zy 
                                 : i.worldPos.xy;
                
                wallPos *= max(0.05, _HexSize);

                // Lineas hexagonales con corte estilo caricatura/toon
                float hexVal = GetHexGrid(wallPos);
                float hexLines = 1.0 - smoothstep(_BorderWidth, _BorderWidth + _ToonCut, hexVal);

                // Ondas de energia que suben por la pared
                float scanWave = sin(i.worldPos.y * _PulseScale - _Time.y * _Speed);
                float toonWave = smoothstep(0.3, 0.45, scanWave) * 0.55 + 0.45;

                // Brillo extra al mirar la pared en angulo (Fresnel toon)
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1.0 - abs(dot(normalize(i.worldNormal), viewDir)), _RimPower);
                float toonRim = smoothstep(0.25, 0.35, fresnel) * 0.6;

                // Mascara final de energia
                float energy = saturate(hexLines * toonWave + toonRim);

                // Color final: fondo casi invisible + lineas brillantes
                float3 color = lerp(_GlassTint.rgb, _EnergyColor.rgb, energy);
                float alpha = saturate(_GlassTint.a + energy * 0.85);

                return fixed4(color, alpha);
            }
            ENDCG
        }
    }
}
