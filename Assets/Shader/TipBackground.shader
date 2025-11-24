Shader "UI/TipBackground"
{
    Properties
    {
        _CenterColor ("Center Color", Color) = (1, 1, 1, 1)
        _SideColor ("Side Color", Color) = (0, 0, 0, 0)
        _MainTex ("Base (RGB)", 2D) = "white" { }
    }
    SubShader
    {
        Tags {"Queue"="Overlay" "RenderType"="Opaque" }
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

            uniform float4 _CenterColor;
            uniform float4 _SideColor;
            uniform float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // 计算渐变位置，中心部分是1，两侧逐渐变化
                float t = abs(i.uv.x - 0.5) * 2.0; // 计算从中心到两侧的距离，越远渐变越强
                half4 color = lerp(_CenterColor, _SideColor, t); // 从中心颜色到两侧颜色插值
                return color;
            }
            ENDCG
        }
    }
}