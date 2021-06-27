Shader "Holistic/ToonRamp" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _TexScale ("Texture Scale", Range(0.5, 10.0)) = 1.0
        _Normal ("Normal", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 10)) = 1
        _RampTex ("Ramp Texture", 2D) = "white" {}
        _EmissiveRim ("Emissive Rim", Color) = (1,1,1,1)
        _RimPow ("Rim Edginess", Range(1.0, 8.0)) = 3.0
    }
    
    SubShader {
        Tags {
            "Queue" = "Geometry"
        }
        
        CGPROGRAM
        #pragma surface surf ToonRamp
        
        float4 _Color;
        sampler2D _RampTex;
        sampler2D _MainTex;
        half _TexScale;
        sampler2D _Normal;
        half _NormalStrength;
        float4 _EmissiveRim;
        half _RimPow;

        half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
        {
            half diff = dot(s.Normal, lightDir);
            float h = diff * 0.5 + 0.5;
            float2 rh = h;
            float3 ramp = tex2D(_RampTex, rh).rgb;

            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * ramp;
            c.a = s.Alpha;
            return c; 
        }
        
        struct Input {
            float2 uv_MainTex;
            float2 uv_Normal;
            float3 viewDir;
        };
        
        void surf(Input IN, inout SurfaceOutput o) {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex / _TexScale).rgb * _Color.rgb;
            o.Normal =  UnpackNormal(tex2D(_Normal, IN.uv_Normal / _TexScale)) * float3(_NormalStrength, _NormalStrength, 1);

            float d = saturate(1 - dot(IN.viewDir, o.Normal));
            float p = pow(d, _RimPow);
            o.Emission.rgb = _EmissiveRim.rgb * float3(p, p, p);
        }
        ENDCG
    }
    Fallback "Diffuse"
}