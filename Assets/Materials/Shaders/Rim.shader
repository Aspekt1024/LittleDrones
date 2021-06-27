Shader "Aspekt/Rim" {
    Properties {
        _MainTex ("Main Texture", 2D) = "black" {}
        _Color ("Rim Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0, 10)) = 1
        
    }
    
    SubShader {
        CGPROGRAM
        #pragma surface surf Standard
        
        sampler2D _MainTex;
        float4 _Color;
        
        struct Input {
            float2 uv_MainTex;
            float3 viewDir;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
            float d = pow(1 - dot(IN.viewDir, o.Normal), 2);
            o.Emission = _Color * d;
            o.Metallic = 1;
        }
        
        ENDCG
    }
    Fallback "Diffuse"
}