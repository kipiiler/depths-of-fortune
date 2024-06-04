Shader "Hidden/MapRevealShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _MaskTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_MaskTex, i.uv);
                // just invert the colors
                // col.rgb = 1 - col.rgb;

                if(mask.a == 0.0)
                {
                    return mask;
                } else
                {
                    if(col.r < 0.05 && col.g < 0.05 && col.b < 0.05)
                    {
                        // Set color to white
                        col.rgb = fixed3(1.0, 1.0, 1.0);
                    } else if (col.r > 0.1) {
                        // Set color to red
                        col.rgb = fixed3(1.0, 0.0, 0.0);
                    } else {
                        // Set color to black
                        col.rgb = fixed3(0.0, 0.0, 0.0);
                    }
                    return col;
                }
            }
            ENDCG
        }
    }
}
