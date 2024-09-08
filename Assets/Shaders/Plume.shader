Shader "Unlit/Plume"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 0, 1, 1)
        _ColorFlame ("Flame Color", Color) = (1, 0, 1, 1)
        _MaxY ("Max", float) = 0
        _MinY ("Min", float) = -5
        _Radius ("Radius", float) = 5
        _OriginOffset ("OriginOffset", float) = 0
        _Resolution ("Resolution", int) = 500
        _RayLength ("Ray Length", float) = 5
        _Flare ("Flare", float) = 0
        _Throttle ("Throttle", Range(0, 1)) = 1
        _Multiplier ("Multiplier", float) = 1
        _RadialMultiplier ("Radial Multiplier", float) = 1
        _RadialOffset ("Radial Offset", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100
        Cull Off
        Blend SrcAlpha One
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _ColorFlame;
            float _MaxY;
            float _MinY;
            float _Radius;
            float _OriginOffset;
            float _RayLength;
            float _Flare;
            float _Throttle;
            float _Multiplier;
            float _RadialMultiplier;
            float _RadialOffset;
            int _Resolution;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            float raycastAlpha(float3 position, fixed3 direction, fixed3 camPosition)
            {
                if (_Throttle == 0)
                {
                    return 0;
                }

                float hits = 0;
                float opaqueness = 0;
                bool inside = false;
                fixed3 local = mul(unity_WorldToObject, camPosition) + mul(unity_WorldToObject, float4(0, 0, 0, 1));
                // if (local.y < _MaxY && local.y > _MinY && distance(local.xz, float2(0, 0)) < 1)
                // {
                //     inside = true;
                // }

                for (int i = 0; i < _Resolution; i++)
                {
                    // float3 local = mul(unity_WorldToObject, position) + mul(unity_WorldToObject, float4(0, 0, 0, 1));
                    local = mul(unity_WorldToObject, position) + mul(unity_WorldToObject, float4(0, 0, 0, 1));
                    float local_radial = sqrt(local.x * local.x + local.z * local.z);
                    // local = mul(UNITY_MATRIX_P, position);
                    if (local.y < _MaxY && local.y > _MinY && distance(local.xz, float2(0, 0)) < (_Radius + local.y * _Flare))
                    {
                        opaqueness += max((_MinY) / (local.y - _OriginOffset) * ((local.y - (_MinY)) / abs(_MinY)) / ((local_radial + _RadialOffset) * _RadialMultiplier), 0);
                    }
                    else
                    {
                        break;
                    }
                    // if (inside)
                    // {
                        position -= direction * (_RayLength / _Resolution);
                    // }
                    // else
                    // {
                    //     position += direction * (_RayLength / _Resolution);
                    // }
                }
                opaqueness /= _Resolution;
                return min(opaqueness * _Throttle * _Multiplier, 1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewDirection = normalize(i.worldPos - _WorldSpaceCameraPos);
                float alpha = raycastAlpha(i.worldPos, viewDirection, _WorldSpaceCameraPos);
                fixed4 col = lerp(_Color, _ColorFlame, alpha);
                // col.a = alpha;
                // UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

            ENDHLSL
        }
    }
}
