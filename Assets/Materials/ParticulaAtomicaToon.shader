Shader "Custom/Atomo3DToon"
{
    Properties
    {
        [Header(Colores Estilo Toon)]
        [HDR] _ProtonColor ("Color Protones (Nucleo)", Color) = (1.0, 0.2, 0.1, 1.0)
        [HDR] _NeutronColor ("Color Neutrones (Nucleo)", Color) = (1.0, 0.75, 0.1, 1.0)
        [HDR] _RingColor ("Color Anillos Orbitales", Color) = (0.0, 0.85, 1.0, 1.0)
        [HDR] _ElectronColor ("Color Electrones", Color) = (0.7, 1.0, 1.0, 1.0)
        _AuraColor ("Color Aura Exterior", Color) = (0.1, 0.4, 0.9, 0.15)

        [Header(Dimensiones)]
        _CoreSize ("Tamano del Nucleo", Range(0.05, 0.22)) = 0.11
        _OrbitRadius ("Radio de Anillos", Range(0.25, 0.44)) = 0.37
        _RingThickness ("Grosor de Anillos", Range(0.004, 0.03)) = 0.011
        _ElectronSize ("Tamano de Electrones", Range(0.015, 0.06)) = 0.032

        [Header(Animacion)]
        _OrbitSpeed ("Velocidad Electrones", Float) = 4.0
        _WobbleSpeed ("Velocidad Giro Anillos", Float) = 1.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 localPos : TEXCOORD0;
                float3 camLocalPos : TEXCOORD1;
            };

            float4 _ProtonColor;
            float4 _NeutronColor;
            float4 _RingColor;
            float4 _ElectronColor;
            float4 _AuraColor;
            float _CoreSize;
            float _OrbitRadius;
            float _RingThickness;
            float _ElectronSize;
            float _OrbitSpeed;
            float _WobbleSpeed;

            // Rotaciones 3D
            float3 RotX(float3 p, float a) { float s = sin(a), c = cos(a); return float3(p.x, c*p.y - s*p.z, s*p.y + c*p.z); }
            float3 RotY(float3 p, float a) { float s = sin(a), c = cos(a); return float3(c*p.x + s*p.z, p.y, -s*p.x + c*p.z); }
            float3 RotZ(float3 p, float a) { float s = sin(a), c = cos(a); return float3(c*p.x - s*p.y, s*p.x + c*p.y, p.z); }

            // Distancia a un Toroide (anillo 3D)
            float sdTorus(float3 p, float2 t)
            {
                float2 q = float2(length(p.xz) - t.x, p.y);
                return length(q) - t.y;
            }

            // Evalua la escena 3D interna del atomo
            float2 MapAtom(float3 p)
            {
                float t = _Time.y;
                
                // 1. NUCLEO (Racimo de protones y neutrones girando)
                float3 coreP = RotY(RotX(p, t * 1.2), t * 1.7);
                float r = _CoreSize * 0.65;
                float off = _CoreSize * 0.48;

                // Protones
                float dP1 = length(coreP - float3( off,  off,  off)) - r;
                float dP2 = length(coreP - float3(-off, -off,  off)) - r;
                float dP3 = length(coreP - float3(-off,  off, -off)) - r;
                float dP4 = length(coreP - float3( off, -off, -off)) - r;
                float dProtons = min(min(dP1, dP2), min(dP3, dP4));

                // Neutrones
                float dN1 = length(coreP - float3(-off,  off,  off)) - r;
                float dN2 = length(coreP - float3( off, -off,  off)) - r;
                float dN3 = length(coreP - float3( off,  off, -off)) - r;
                float dN4 = length(coreP - float3(-off, -off, -off)) - r;
                float dNeutrons = min(min(dN1, dN2), min(dN3, dN4));

                float2 res = (dProtons < dNeutrons) ? float2(dProtons, 1.0) : float2(dNeutrons, 2.0);

                // 2. ANILLOS Y ELECTRONES EN 3D
                // Anillo 1
                float3 p1 = RotY(RotZ(p, 1.05 + sin(t * _WobbleSpeed) * 0.15), t * _WobbleSpeed * 0.7);
                float dRing1 = sdTorus(p1, float2(_OrbitRadius, _RingThickness));
                float eAngle1 = t * _OrbitSpeed;
                float3 ePos1 = float3(cos(eAngle1) * _OrbitRadius, 0, sin(eAngle1) * _OrbitRadius);
                float dElec1 = length(p1 - ePos1) - _ElectronSize;

                // Anillo 2
                float3 p2 = RotY(RotZ(p, -1.05 + cos(t * _WobbleSpeed) * 0.15), -t * _WobbleSpeed * 0.8);
                float dRing2 = sdTorus(p2, float2(_OrbitRadius, _RingThickness));
                float eAngle2 = t * _OrbitSpeed * 1.15 + 2.1;
                float3 ePos2 = float3(cos(eAngle2) * _OrbitRadius, 0, sin(eAngle2) * _OrbitRadius);
                float dElec2 = length(p2 - ePos2) - _ElectronSize;

                // Anillo 3
                float3 p3 = RotZ(RotX(p, 1.1 + sin(t * _WobbleSpeed * 0.9) * 0.15), t * _WobbleSpeed * 0.6);
                float dRing3 = sdTorus(p3, float2(_OrbitRadius, _RingThickness));
                float eAngle3 = -t * _OrbitSpeed * 0.9 + 4.2;
                float3 ePos3 = float3(cos(eAngle3) * _OrbitRadius, 0, sin(eAngle3) * _OrbitRadius);
                float dElec3 = length(p3 - ePos3) - _ElectronSize;

                float dRings = min(min(dRing1, dRing2), dRing3);
                float dElecs = min(min(dElec1, dElec2), dElec3);

                if (dRings < res.x) res = float2(dRings, 3.0);
                if (dElecs < res.x) res = float2(dElecs, 4.0);

                return res;
            }

            float3 GetNormal(float3 p)
            {
                float2 e = float2(0.002, 0.0);
                return normalize(float3(
                    MapAtom(p + e.xyy).x - MapAtom(p - e.xyy).x,
                    MapAtom(p + e.yxy).x - MapAtom(p - e.yxy).x,
                    MapAtom(p + e.yyx).x - MapAtom(p - e.yyx).x
                ));
            }

            float2 IntersectSphere(float3 ro, float3 rd, float rad)
            {
                float b = dot(ro, rd);
                float c = dot(ro, ro) - rad * rad;
                float h = b * b - c;
                if (h < 0.0) return float2(-1.0, -1.0);
                h = sqrt(h);
                return float2(-b - h, -b + h);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex.xyz;
                o.camLocalPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 ro = i.camLocalPos;
                float3 rd = normalize(i.localPos - ro);

                float2 bounds = IntersectSphere(ro, rd, 0.49);
                if (bounds.y < 0.0) discard;

                float t = max(0.0, bounds.x);
                float hitID = 0.0;
                float3 hitPos = float3(0,0,0);

                // Variable renombrada a 'idx' para no chocar con la funcion step()
                for (int idx = 0; idx < 56; idx++)
                {
                    float3 p = ro + rd * t;
                    float2 d = MapAtom(p);
                    if (d.x < 0.002)
                    {
                        hitID = d.y;
                        hitPos = p;
                        break;
                    }
                    t += d.x;
                    if (t > bounds.y) break;
                }

                float sphereThickness = bounds.y - max(0.0, bounds.x);
                float auraAlpha = saturate(sphereThickness * 1.2) * _AuraColor.a;

                if (hitID > 0.5)
                {
                    float3 n = GetNormal(hitPos);
                    float3 lightDir = normalize(float3(0.5, 1.0, 0.5));
                    float toonLight = step(0.1, dot(n, lightDir)) * 0.35 + 0.65;

                    float3 col = _ProtonColor.rgb;
                    if (hitID > 1.5 && hitID < 2.5) col = _NeutronColor.rgb;
                    else if (hitID > 2.5 && hitID < 3.5) { col = _RingColor.rgb; toonLight = 1.0; }
                    else if (hitID > 3.5) { col = _ElectronColor.rgb; toonLight = 1.0; }

                    return fixed4(col * toonLight, 1.0);
                }

                return fixed4(_AuraColor.rgb, auraAlpha);
            }
            ENDCG
        }
    }
}
