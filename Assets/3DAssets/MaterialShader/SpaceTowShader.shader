Shader "SpaceTow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SpecualarMask ("SpecularMask", 2D) = "black" {}
        _LightVal0 ("Light Value 1", range(0,1)) = 1
        _LightVal1 ("Light Value 2", range(0,1)) = 1
        _SpecLightVal ("Specular Intensity", range(0,5)) = 3
        _RimIntensity("Rim Intensity", range (0,3)) = 1
        _MainColor("Main Color", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color", Color) = (0.3,0.33,0.54,1)
        _LightC ("Light Color", Color) = (0.77,0.70,0.19,1)
        _RimLight ("Rim Color", Color) = (0.77,0.70,0.19,1)

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags
			{
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Lighting.cginc"
            
            uniform sampler2D _MainTex;
            uniform sampler2D _SpecualarMask;
            uniform float _LightVal0;
            uniform float _LightVal1;
            uniform float4 _ShadowColor;
            uniform float4 _LightC;
            uniform float4 _RimLight;
            uniform float _SpecLightVal;
            uniform float _RimIntensity;
            uniform float4 _MainColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 stylisticLight : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = (v.uv.xy);

                float3 worldNormal = UnityObjectToWorldNormal(v.normal);

                float3 normal = normalize(UnityObjectToWorldNormal(v.normal));
                
                half NdotL = dot (_WorldSpaceLightPos0, normal) * _LightColor0;
                half rimDot = 1- dot(WorldSpaceViewDir(v.vertex), normal) * _LightColor0;

                o.stylisticLight.r =  NdotL;

                o.stylisticLight.g = rimDot;


                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {                
                float3 mainSample  = tex2D(_MainTex, i.uv);
                float specSample = tex2D(_SpecualarMask, i.uv);


                float3 diffuseLight =  lerp (_LightC, 0.89, clamp(i.stylisticLight.r*2 -_LightVal0,0,1));
                diffuseLight =  lerp ((_ShadowColor), diffuseLight,clamp(i.stylisticLight.r*2 -_LightVal1,0,1));

                float3 specularLight = clamp((i.stylisticLight.g * _LightC * specSample * _SpecLightVal),0,1);

                float3 rimLight = clamp (i.stylisticLight.g * _RimLight * _RimIntensity * (1 - i.stylisticLight.r),0,1);

                return float4 ((mainSample * _MainColor * diffuseLight) + specularLight + rimLight,1);
                

            }
            ENDCG
        }
    }
}
