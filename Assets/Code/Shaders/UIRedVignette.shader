Shader "Custom/UI Red Vignette"
{
    Properties
    {
        _Color ("Color (at full)", Color) = (0.45, 0, 0, 1)
        _Progress ("Progress", Range(0, 1)) = 0
        _Phase ("Phase: 0=fill(淡->浓), 1=fade(浓->淡)", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float _Progress;
            float _Phase;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float alpha;
                if (_Phase < 0.5)
                    alpha = _Progress;   // Fill: 全屏从淡到浓，0=无 1=满
                else
                    alpha = 1.0 - _Progress; // Fade: 全屏从浓到淡，0=满 1=无
                return float4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
    Fallback "UI/Default"
}
