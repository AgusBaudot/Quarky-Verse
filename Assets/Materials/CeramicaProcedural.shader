Shader "Custom/CeramicaProceduralUniversal"
{
    Properties
    {
        [Header(Colores)]
        _TileColor1 ("Color Azulejo A", Color) = (0.85, 0.88, 0.9, 1)
        _TileColor2 ("Color Azulejo B (Variacion)", Color) = (0.72, 0.78, 0.84, 1)
        _GroutColor ("Color Junta / Boquilla", Color) = (0.25, 0.25, 0.28, 1)
        
        [Header(Patron y Forma)]
        _Scale ("Tamano de Azulejos", Float) = 1.0
        _Offset ("Desfase de Fila (Ladrillo)", Range(0, 1)) = 0.5
        _GroutWidth ("Grosor de Junta", Range(0.005, 0.15)) = 0.03
        _EdgeSoftness ("Suavizado de Borde", Range(0.005, 0.15)) = 0.02
        
        [Header(Iluminacion y Relieve)]
        _BumpStrength ("Fuerza del Relieve", Range(0, 5)) = 1.5
        _SpecularPower ("Intensidad Brillo", Range(0, 2)) = 0.8
        _Shininess ("Nitidez Reflejo", Range(1, 128)) = 32.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 worldTangent : TEXCOORD3;
                float3 worldBitangent : TEXCOORD4;
            };

            float4 _TileColor1;
            float4 _TileColor2;
            float4 _GroutColor;
            float _Scale;
            float _Offset;
            float _GroutWidth;
            float _EdgeSoftness;
            float _BumpStrength;
            float _SpecularPower;
            float _Shininess;

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float GetTileHeight(float2 pos, out float2 tileID)
            {
                pos *= max(0.05, _Scale);
                pos.x += step(1.0, fmod(abs(pos.y), 2.0)) * _Offset;
                
                tileID = floor(pos);
                float2 gridUV = frac(pos) - 0.5;
                
                float2 d = abs(gridUV);
                float maxDist = max(d.x, d.y);
                
                float edgeStart = 0.5 - _GroutWidth - _EdgeSoftness;
                float edgeEnd = 0.5 - _GroutWidth;
                
                return 1.0 - smoothstep(edgeStart, edgeEnd, maxDist);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBitangent = cross(o.worldNormal, o.worldTangent) * v.tangent.w;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Usa coordenadas del mundo (XZ) para suelos y plataformas
                float2 worldUV = i.worldPos.xz;
                
                float2 tileID;
                float height = GetTileHeight(worldUV, tileID);
                
                // Relieve simulado
                float2 dummyID;
                float eps = 0.01;
                float hX = GetTileHeight(worldUV + float2(eps, 0), dummyID);
                float hY = GetTileHeight(worldUV + float2(0, eps), dummyID);
                float3 normalTS = normalize(float3((height - hX) * _BumpStrength * 5.0, (height - hY) * _BumpStrength * 5.0, 1.0));

                float3 worldNormal = normalize(
                    normalTS.x * i.worldTangent +
                    normalTS.y * i.worldBitangent +
                    normalTS.z * i.worldNormal
                );

                // Color por pieza
                float randVal = Hash21(tileID);
                float3 tileAlbedo = lerp(_TileColor1.rgb, _TileColor2.rgb, randVal);
                
                float isTile = smoothstep(0.05, 0.2, height);
                float3 baseColor = lerp(_GroutColor.rgb, tileAlbedo, isTile);

                // Iluminación
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                if (length(_WorldSpaceLightPos0.xyz) < 0.001) lightDir = normalize(float3(0.5, 1.0, 0.3));
                
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfVector = normalize(lightDir + viewDir);

                float NdotL = max(0.0, dot(worldNormal, lightDir));
                float NdotH = max(0.0, dot(worldNormal, halfVector));
                
                float specular = pow(NdotH, _Shininess) * _SpecularPower * isTile;
                float3 ambient = baseColor * 0.4;
                float3 finalColor = ambient + (baseColor * NdotL * 0.7) + specular;

                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}
