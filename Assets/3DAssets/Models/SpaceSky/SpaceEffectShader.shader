Shader "Unlit/SpaceEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _BloomLight1 ("Bloom Light 1", Range (0,1)) = 0
        _BloomLight2 ("Bloom Light 2", Range (0,1)) = 0
        _BloomColor1 ("Bloom Color 1", Color) = (1,1,1,1)
        _BloomColor2 ("Bloom Color 2", Color) = (1,1,1,1)
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha



        Tags
        {
        "Queue" = "Transparent" 
        "RenderType"="Transparent" 
        }

        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"




            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RotationSpeed;
            half _BloomLight1;
            half _BloomLight2;
            fixed4 _BloomColor1;
            fixed4 _BloomColor2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                fixed a = (col.r*_BloomLight1 + col.b*_BloomLight2);

                col = col.r*_BloomColor1 + col.b*_BloomColor2;

                //a = 1-a;

                col.a = a;

                return col;
            }
            ENDCG
        }
    }
}
