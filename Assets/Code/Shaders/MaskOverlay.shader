Shader "Custom/MaskOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskColor ("Mask Color", Color) = (0, 0, 0, 1)
        _MaskHeightRatio ("Mask Height Ratio", Range(0, 1)) = 0.3
        _LeftEyePos ("Left Eye Position", Vector) = (0.3, 0.7, 0, 0)
        _RightEyePos ("Right Eye Position", Vector) = (0.7, 0.7, 0, 0)
        _EyeHoleRadius ("Eye Hole Radius", Range(0, 0.5)) = 0.08
        _EyeHoleSmoothness ("Eye Hole Smoothness", Range(0, 0.1)) = 0.02
        
        // 眨眼效果参数（为未来扩展预留）
        _LeftEyeBlink ("Left Eye Blink", Range(0, 1)) = 0.0
        _RightEyeBlink ("Right Eye Blink", Range(0, 1)) = 0.0
        _BlinkSpeed ("Blink Speed", Float) = 5.0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
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
            fixed4 _MaskColor;
            float _MaskHeightRatio;
            float2 _LeftEyePos;
            float2 _RightEyePos;
            float _EyeHoleRadius;
            float _EyeHoleSmoothness;
            float _LeftEyeBlink;
            float _RightEyeBlink;
            float _BlinkSpeed;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // 计算眼睛孔的alpha值（考虑眨眼效果）
            float GetEyeHoleAlpha(float2 uv, float2 eyePos, float blinkAmount)
            {
                float2 eyeUV = uv - eyePos;
                float dist = length(eyeUV);
                
                // 基础圆形孔
                float baseRadius = _EyeHoleRadius;
                
                // 眨眼效果：当blinkAmount > 0时，将圆形孔压缩成椭圆形
                float blinkRadius = baseRadius * (1.0 - blinkAmount * 0.9); // 眨眼时半径缩小到10%
                float blinkHeight = baseRadius * (1.0 - blinkAmount); // 垂直方向完全闭合
                
                // 计算到眼睛中心的距离（考虑眨眼变形）
                float2 eyeDist = eyeUV;
                eyeDist.y /= max(blinkHeight / blinkRadius, 0.01); // 垂直方向压缩
                float eyeDistScaled = length(eyeDist);
                
                // 使用smoothstep创建平滑的边缘
                float alpha = 1.0 - smoothstep(blinkRadius - _EyeHoleSmoothness, blinkRadius + _EyeHoleSmoothness, eyeDistScaled);
                
                return alpha;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // 计算眼睛孔的alpha（1.0 = 在孔内完全透明，0.0 = 在孔外）
                float leftEyeAlpha = GetEyeHoleAlpha(uv, _LeftEyePos, _LeftEyeBlink);
                float rightEyeAlpha = GetEyeHoleAlpha(uv, _RightEyePos, _RightEyeBlink);
                float eyeHoleAlpha = max(leftEyeAlpha, rightEyeAlpha);
                
                // 遮罩逻辑：
                // - 整个屏幕都有遮罩（纯黑色）
                // - 下半部分（y <= maskHeightRatio）：始终显示遮罩，不受眼睛孔影响
                // - 上半部分（y > maskHeightRatio）：只在非眼睛孔区域显示遮罩
                float maskAlpha = 1.0;
                
                if (uv.y <= _MaskHeightRatio)
                {
                    // 下半部分：始终有遮罩（纯黑色，完全不透明）
                    maskAlpha = 1.0;
                }
                else
                {
                    // 上半部分：只在非眼睛孔区域显示遮罩
                    // eyeHoleAlpha = 1.0 时（在孔内），maskAlpha = 0（完全透明）
                    // eyeHoleAlpha = 0.0 时（在孔外），maskAlpha = 1.0（显示纯黑色遮罩）
                    maskAlpha = 1.0 - eyeHoleAlpha;
                }
                
                // 使用纯黑色，alpha由maskAlpha控制
                fixed4 col = fixed4(0, 0, 0, maskAlpha);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "UI/Default"
}
