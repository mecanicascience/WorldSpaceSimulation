Shader "Custom/AtmosphereShader" {
    Properties {
        _ForceDisable ("Force Disable", Int) = 0
        _MainTex ("Texture", 2D) = "white" {}
        _AtmosphereDiameter ("Atmosphere Radius", Float) = 10

        _AtmosphereColor ("Atmosphere Color", Color) = (0, 0, 0, 1)
        _FirstAlphaBlend ("First Alpha Multiplier", Float) = 1
        _SecondAlphaBlend ("Second Alpha Multiplier", Float) = 1
    }
    SubShader {
        // No culling or depth
		Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };

            static const float maxFloat = 3.402823466e+38;

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            int _ForceDisable;
            float3 atmosphereCenter = float3(0, 0, 0);
            float _AtmosphereDiameter;
            float4 _AtmosphereColor;
            float _FirstAlphaBlend;
            float _SecondAlphaBlend;


            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1));
                o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));

                return o;
			}


            // Returns vector (dstToSphere, dstThroughSphere)
            // If ray origin is inside sphere, dstToSphere = 0
            // If ray misses sphere, dstToSphere = maxValue; dstThroughSphere = 0
            float2 raySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir) {
                float3 offset = rayOrigin - sphereCentre;
                float a = 1; // Set to dot(rayDir, rayDir) if rayDir might not be normalized
                float b = 2 * dot(offset, rayDir);
                float c = dot(offset, offset) - sphereRadius * sphereRadius;
                float d = b * b - 4 * a * c; // Discriminant from quadratic formula

                // Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
                if (d > 0) {
                    float s = sqrt(d);
                    float dstToSphereNear = max(0, (-b - s) / (2 * a));
                    float dstToSphereFar = (-b + s) / (2 * a);

                    // Ignore intersections that occur behind the ray
                    if (dstToSphereFar >= 0) {
                        return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
                    }
                }
                // Ray did not intersect sphere
                return float2(maxFloat, 0);
            }


            float4 frag (v2f i) : SV_Target {
                float4 originalCol = tex2D(_MainTex, i.uv);
                if (_ForceDisable == 1)
                    return originalCol;
                
                float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);

                float2 hitInfo = raySphere(atmosphereCenter, _AtmosphereDiameter / 2, rayOrigin, rayDir);
                float dstToAtmosphere = hitInfo.x;
                float dstThroughAtmosphere = hitInfo.y; // min(hitInfo.y, dstToAtmosphere - dstToAtmosphere);

                if (dstThroughAtmosphere == 0)
                    return originalCol;

                float f = dstThroughAtmosphere / _AtmosphereDiameter * 2;
                float4 col = lerp(_AtmosphereColor, float4(f, f, f, 1), _FirstAlphaBlend);
                return lerp(originalCol, col, _SecondAlphaBlend);
            }

            ENDCG
        }
    }
}
