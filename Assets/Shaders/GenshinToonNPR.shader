Shader "Custom/GenshinToonNPR"
{
    Properties
    {
        _Color("Base Color", Color) = (1, 1, 1, 1)
        _MainTex("Base Texture", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _ShadowColor("Shadow Tint", Color) = (0.55, 0.58, 0.70, 1)
        _RampThreshold("Ramp Threshold", Range(0.0, 1.0)) = 0.5
        _RampSmooth("Ramp Smoothness", Range(0.001, 0.5)) = 0.18

        _SpecularColor("Specular Tint", Color) = (1, 1, 1, 1)
        _Glossiness("Glossiness", Range(1.0, 128.0)) = 32.0

        _RimColor("Rim Light Color", Color) = (0.85, 0.92, 1.0, 1)
        _RimPower("Rim Power", Range(1.0, 10.0)) = 3.5

        _OutlineColor("Outline Color", Color) = (0.15, 0.15, 0.2, 1)
        _OutlineWidth("Outline Width", Range(0.0, 5.0)) = 1.2
        _EmissionColor("Emission Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }
        LOD 200

        CGPROGRAM
        #pragma surface surf ToonRamp fullforwardshadows alphatest:_Cutoff addshadow
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        sampler2D _MainTex;
        fixed4 _Color;
        half _Cutoff;
        fixed4 _ShadowColor;
        half _RampThreshold;
        half _RampSmooth;
        fixed4 _SpecularColor;
        half _Glossiness;
        fixed4 _RimColor;
        half _RimPower;
        fixed4 _EmissionColor;

        half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half NdotL = dot(s.Normal, lightDir);
            half halfLambert = NdotL * 0.5 + 0.5;
            half cel = smoothstep(_RampThreshold - _RampSmooth, _RampThreshold + _RampSmooth, halfLambert * atten);
            half3 ramp = lerp(_ShadowColor.rgb, half3(1.0, 1.0, 1.0), cel);

            // Specular anime hair/cloth sheen
            half3 halfDir = normalize(lightDir + viewDir);
            half NdotH = saturate(dot(s.Normal, halfDir));
            half spec = pow(NdotH, _Glossiness);
            half specStep = smoothstep(0.72 - 0.05, 0.72 + 0.05, spec) * cel;

            // Soft anime rim light
            half NdotV = 1.0 - saturate(dot(s.Normal, viewDir));
            half rim = pow(NdotV, _RimPower);
            half rimStep = smoothstep(0.50, 0.65, rim) * saturate(dot(lightDir, -s.Normal) * 0.5 + 0.5);

            // Cleanroom ambient fill to ensure vivid anime clarity in shadow
            half3 ambientFill = UNITY_LIGHTMODEL_AMBIENT.rgb * 0.65 + half3(0.22, 0.25, 0.32);
            half3 lightCol = min(_LightColor0.rgb, 1.15);

            half4 c;
            c.rgb = (s.Albedo * (lightCol * ramp + ambientFill) + 
                     _SpecularColor.rgb * specStep * 0.40 + 
                     _RimColor.rgb * rimStep * 0.30) + s.Emission;
            c.a = s.Alpha;
            return c;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Emission = _EmissionColor.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Transparent/Cutout/Diffuse"
}
