﻿Shader "Custom/PetesWater"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        // Physically based Standard lighting model, and enable shadows on all light types  
        //Tags { "RenderType"="Opaque" }
        //LOD 200
        //CGPROGRAM
        //#pragma surface surf Standard fullforwardshadows

        // Petes Shader
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard alpha vertex:vert addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
            float3 worldPos;
            float3 originalPos;
            half3 worldRefl;
            INTERNAL_DATA
        };

        struct SurfaceOutputCustom
        {
            float3 Albedo;
            float3 Normal;
            float Alpha;
            float Metallic;
            float Smoothness;
            float Emission;
            float4 foam;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;


        static const float pi = 3.141592653589793238462;
        static const float deg2Rad = 3.141592653589793238462 / 180.0;
        static const float one = 1.0;

        // Initialized via script
        static const float WAVES_CAPACITY = 8;
        int _WavesLength = 0;
        float4 _WavesData[WAVES_CAPACITY]; // steepness, amplitude, frequency, speed
        float4 _WavesDirection[WAVES_CAPACITY]; // directionXYZ, phase constant

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float3 GerstnerWave(float4 wave, float3 p, inout float3 tangent, inout float3 binormal) {

            float steepness = wave.z;
            float wavelength = wave.w;
            float k = 2 * UNITY_PI / wavelength;
            float c = sqrt(9.8 / k);
            float2 d = normalize(wave.xy);
            float f = k * (dot(d, p.xz) - c * _Time.y);
            float a = steepness / k;

            tangent += float3(
                -d.x * d.x * (steepness * sin(f)),
                d.x * (steepness * cos(f)),
                -d.x * d.y * (steepness * sin(f))
                );
            binormal += float3(
                -d.x * d.y * (steepness * sin(f)),
                d.y * (steepness * cos(f)),
                -d.y * d.y * (steepness * sin(f))
                );
            return float3(
                d.x * (a * cos(f)),
                a * sin(f),
                d.y * (a * cos(f))
                );
        }

        void vert(inout appdata_full v, out Input toSurf)
        {
            // Calculate reflection (this can be used for skybox reflections in the surface shader)
            float3 worldViewDir = normalize(UnityWorldSpaceViewDir(v.vertex));
            half3 worldRefl = reflect(-worldViewDir, v.normal);

            // Compute Gerstner Waves position
            float4 pos = mul(unity_ObjectToWorld, v.vertex);
            //			pos.y = 0;

            float3 xz = pos.xyz;
            //			xz.y = 0;

            for (int i = 0; i < _WavesLength; i++)
            {
                float4 data = _WavesData[i];
                float4 direction = _WavesDirection[i];
                float steepness = data[0];
                float amplitude = data[1];
                float frequency = data[2];
                float speed = data[3];
                float phaseConstant = direction[3];
                float time = _Time[1];

                pos.x += steepness * amplitude * direction.x * cos(dot(frequency * direction, xz) + phaseConstant * time);
                pos.z += steepness * amplitude * direction.z * cos(dot(frequency * direction, xz) + phaseConstant * time);
                pos.y += amplitude * sin(dot(frequency * direction, xz) + phaseConstant * time);
            }

            // Compute Gerstner Waves normal
            float3 normal = float3(0, 1, 0);
            for (i = 0; i < _WavesLength; i++)
            {
                float4 data = _WavesData[i];
                float4 direction = _WavesDirection[i];
                float steepness = data[0];
                float amplitude = data[1];
                float frequency = data[2];
                float speed = data[3];
                float phaseConstant = direction[3];
                float time = _Time[1];

                float s = sin(dot(frequency * direction, pos) + phaseConstant * time);
                float c = cos(dot(frequency * direction, pos) + phaseConstant * time);
                float wa = frequency * amplitude;

                normal.x -= direction.x * wa * c;
                normal.z -= direction.z * wa * c;
                normal.y -= steepness * wa * s;
            }

            // Apply vertex offset and normal
            v.vertex = mul(unity_WorldToObject, pos);
            v.normal = normal;

            // Send additional data to surface shader
            UNITY_INITIALIZE_OUTPUT(Input, toSurf);
            toSurf.originalPos = xz;
            toSurf.worldRefl = worldRefl;
        }

        // Standard Surface
        /*
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // from vert
            float3 objectPos = mul(unity_WorldToObject, i.worldPos);
         
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        */

        // Petes Water Surface
        //void surf(Input i, inout SurfaceOutputCustom o)
        void surf(Input i, inout SurfaceOutputStandard o)
        {
            //SurfaceOutputCustom
            //o.Albedo;
            //o.Normal;
            //o.Alpha;
            //o.Emission;
            //o.foam;


            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, i.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            
            // from vert
            //float3 objectPos = mul(unity_WorldToObject, i.worldPos);

            //float2 normalUv = 0.02 * (i.originalPos.xz + i.worldPos.xz) * 0.5;
            //normalUv = normalUv.yx;

            //float3 normalMap = tex2D(_NoiseMap, normalUv) - float3(0.5, 0, 0.5);
            //normalMap.y = 0;
            //normalMap = normalize(normalMap);
            //o.Normal += normalMap * 0.25;*/

            /*half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
            half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);*/

            //float3 base = lerp(_BaseColor, _EdgeColor, saturate(_EdgeStart + _EdgeIncrease * objectPos.y));
            
            //float fresnel = 1 - dot(i.worldNormal, i.viewDir);
            //base = lerp(_BaseColor, _EdgeColor, fresnel * 3);

            //float3 seaUvOffset = tex2D(_SeaDistortion, i.originalPos.xz * 0.005 + i.worldPos.xz * 0.005 + _Time[1] * 1.0 * float2(0.01, 0.02));

            //float2 voronoiUv1 = 0.4 * (i.originalPos.xz * 0.025 * 0.25 + i.worldPos.xz * 0.025 * 0.75);
            //float2 voronoiUv2 = 0.4 * (i.originalPos.xz * 0.004 + i.worldPos.xz * 0.028) + float2(0.5, 0.4) + seaUvOffset.rb * 0.05;
            //float3 voronoi1 = (greaterThan(tex2D(_Voronoi, voronoiUv1), 0.45)) * 0.4 * 2.6;
            //float3 voronoi2 = (greaterThan(tex2D(_Voronoi, voronoiUv2), 0.4)) * 0.4 * 2.6;

            //float foamAmount = max(0, 0.3 * objectPos.y + 0.4);
            //float foamAmount = (1 - i.worldNormal.y) * _FoamMultiplier;
            //float foamAmount = 1;
            //float texturedFoamAmount = greaterThan(foamAmount * voronoi1 + foamAmount * voronoi2, 1.05) * 0.8;
            //o.Albedo = lerp(base, 1.0, texturedFoamAmount);
            //o.Albedo = seaUvOffset;
            //o.Albedo = lerp(base, 1.0, voronoi2);
            //o.Albedo = voronoi2;
            //o.Albedo = foamAmount;
            //o.foam = texturedFoamAmount;
            
        }

        ENDCG
    }
    FallBack "Diffuse"
}
