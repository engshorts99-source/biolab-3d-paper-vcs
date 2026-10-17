Shader "Custom/GenshinFace"
{
    Properties
    {
        _Color("Base Color", Color) = (1, 1, 1, 1)
        _MainTex("Face Texture", 2D) = "white" {}
        _ShadowTint("Soft Shadow Tint", Color) = (0.90, 0.82, 0.84, 1)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _Emission("Feature Emission Boost", Range(0.0, 2.0)) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }
        LOD 200

        CGPROGRAM
        #pragma surface surf AnimeFace fullforwardshadows alphatest:_Cutoff addshadow
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _ShadowTint;
        half _Cutoff;
        half _Emission;

        half4 LightingAnimeFace(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            // Soft, flattering anime cel lighting for face
            half NdotL = dot(s.Normal, lightDir);
            half halfLambert = NdotL * 0.5 + 0.5;
            half ramp = smoothstep(0.32, 0.68, halfLambert * atten);
            half3 shade = lerp(_ShadowTint.rgb, half3(1.0, 0.98, 0.97), ramp);

            half4 c;
            // 0.60 balances with cleanroom environment directional + ambient light
            // ensuring skin retains delicate peach tone and soft warmth
            c.rgb = s.Albedo * shade * 0.60 + s.Emission;
            c.a = s.Alpha;
            return c;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = tex.rgb;
            o.Emission = tex.rgb * _Emission;
            o.Alpha = tex.a;
        }
        ENDCG
    }
    FallBack "Transparent/Cutout/Diffuse"
}
