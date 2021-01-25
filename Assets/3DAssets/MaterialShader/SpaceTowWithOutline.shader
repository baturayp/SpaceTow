Shader "SpaceTowFresnel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SpecualarMask ("SpecularMask", 2D) = "black" {}
        _EmmiMask ("EmmisionMask", 2D) = "black" {}
        _LightVal0 ("Light Value 1", range(0,1)) = 1
        _LightVal1 ("Light Value 2", range(0,1)) = 1
        _SpecLightVal ("Specular Intensity", range(0,5)) = 3
        _RimIntensity("Rim Intensity", range (0,3)) = 1
        _MainColor("Main Color", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color", Color) = (0.3,0.33,0.54,1)
        _LightC ("Light Color", Color) = (0.77,0.70,0.19,1)
        _RimLight ("Rim Color", Color) = (0.77,0.70,0.19,1)
        
        [HDR] _Color ("Fresnel Color", Color) = (1,1,1,1)
        _FresnelPower("Fresnel Power", Range(0, 10)) = 3
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
            uniform sampler2D _EmmiMask;
            uniform fixed _LightVal0;
            uniform fixed _LightVal1;
            uniform fixed4 _ShadowColor;
            uniform fixed4 _LightC;
            uniform fixed4 _RimLight;
            uniform fixed _SpecLightVal;
            uniform fixed _RimIntensity;
            uniform fixed4 _MainColor;

            struct appdata
            {
                fixed4 vertex : POSITION;
                fixed2 uv : TEXCOORD0;
                fixed3 normal : NORMAL;
            };

            struct v2f
            {
                fixed4 vertex : SV_POSITION;
                fixed2 stylisticLight : TEXCOORD1;
                fixed rim : TEXCOORD2;
                fixed2 uv : TEXCOORD0;
            };

            fixed3 viewDir;
            float4 _MainTex_ST;
            fixed4 _Color;
            half _FresnelPower;

            v2f vert (appdata v)
            {
                v.vertex = mul(1.1,v.vertex);
                
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = (v.uv.xy);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
                fixed3 normal = normalize(UnityObjectToWorldNormal(v.normal));
                fixed NdotL = dot (_WorldSpaceLightPos0, normal) * _LightColor0;
                fixed rimDot = 1- dot(WorldSpaceViewDir(v.vertex), normal) * _LightColor0;
                
                viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.rim = 1.1 - saturate(dot(viewDir, v.normal));

                o.stylisticLight.r =  NdotL;
                o.stylisticLight.g = rimDot;


                return o;
            }

            fixed pixel;
            fixed4 frag (v2f i) : SV_Target
            {                
                fixed3 mainSample  = tex2D(_MainTex, i.uv);
                fixed specSample = tex2D(_SpecualarMask, i.uv);
                fixed emmisSample = tex2D(_EmmiMask, i.uv);
                fixed3 diffuseLight =  lerp (_LightC, 0.89, clamp(i.stylisticLight.r*2 -_LightVal0, 0, 1));
                diffuseLight =  lerp ((_ShadowColor), diffuseLight,clamp(i.stylisticLight.r*2 -_LightVal1, 0, 1));
                diffuseLight = lerp(diffuseLight , 1 , clamp(emmisSample, 0, 1));
                fixed3 specularLight = clamp((i.stylisticLight.g * _LightC * specSample * _SpecLightVal), 0, 1);
                fixed3 rimLight = clamp (i.stylisticLight.g * _RimLight * _RimIntensity * (1 - i.stylisticLight.r), 0, 1);
                //modify
                pixel = pow(_FresnelPower, i.rim);             
                pixel = lerp(0, pixel, i.rim);
                //modify

                fixed3 fres = pixel*_Color;
                //fixed3 fres = clamp(pixel, 0, _Color);
                //return fixed4 (fres,1);

                //return fixed4 ((mainSample * _MainColor * diffuseLight) + specularLight, 1);
                return fixed4 ((mainSample * _MainColor * diffuseLight) + specularLight + fres, 1);
            }
            ENDCG
        }
    }
}
