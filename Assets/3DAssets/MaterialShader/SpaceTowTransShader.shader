Shader "SpaceTowTransparent"
{
    Properties
    {
        _RimIntensity("Rim Intensity", range (0,3)) = 1
        _MainColor("Main Color", Color) = (1,1,1,1)
        _RimLight ("Rim Color", Color) = (0.77,0.70,0.19,1)


    }
    SubShader
    {
        Tags { 

            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType"="Transparent" 
            
            }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

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

            uniform sampler2D _SpecualarMask;

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
                fixed2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = (v.uv.xy);

                fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);

                fixed3 normal = normalize(UnityObjectToWorldNormal(v.normal));
                
                fixed NdotL = dot (_WorldSpaceLightPos0, normal) * _LightColor0;
                fixed rimDot = 2 - dot(WorldSpaceViewDir(v.vertex), normal) * _LightColor0 ;

                o.stylisticLight.r =  NdotL;
                o.stylisticLight.g = rimDot;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {                
                fixed specSample = tex2D(_SpecualarMask, i.uv);

                fixed3 specularLight = clamp((i.stylisticLight.g * _LightC * specSample * _SpecLightVal), 0, 1);

                fixed3 rimLight = clamp (i.stylisticLight.g * _RimLight * _RimIntensity * (1 - i.stylisticLight.r), 0, 1);

                fixed lightVal = rimLight.r + rimLight.g + rimLight.b + specularLight.r + specularLight.g + specularLight.b /6;

                return fixed4 (_MainColor * (rimLight), lightVal/2);
                
            }
            ENDCG
        }
    }
}
