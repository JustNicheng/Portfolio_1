Shader "ShaderOfJSNi/OldMovieShader"
{
    //Material顯示的內容
    Properties
    {
        //程式內變數(Inspector顯示名稱，類型) = 預設值
        _MainTex ("原圖", 2D) = "white" {}
        //老電影效果範例 https://www.youtube.com/watch?v=1hGhgD_VUyM
        _ScratchTex ("刮痕圖片", 2D) = "white" {}
        _ScratchEffectValue("刮痕響值",Range(0, 1)) = 0.5
        _DustTex("灰塵圖片",2D) = "white"{}
        _DustEffectValue("灰塵響值",Range(0, 1)) = 0.5
        _OutermostShader("最外層陰影圖",2D) = "white"{}
        _OutermostEffectValue("最外層響值",Range(0, 1)) = 0.5
        _OldFilmColor ("老電影遮罩顏色", Color) = (1,1,1,1)
        _EffectValue("總影響值",Range(0, 1)) = 0.5
    }
    SubShader
    {
        
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vertShader
            #pragma fragment fragShader
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _ScratchTex;
            uniform float _ScratchEffectValue;
            uniform sampler2D _DustTex;
            uniform float _DustEffectValue;
            uniform sampler2D _OutermostShader;
            uniform float _OutermostEffectValue;
            uniform float4 _OldFilmColor;
            uniform float _EffectValue;
        
            struct p2v
            {
                float4 pos_MainTex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 pos : SV_POSITION; // 頂點
                float2 uv : TEXCOORD0; // 頂點
            };
            v2f vertShader(p2v v) 
            {
                v2f vertRes;
                vertRes.pos = UnityObjectToClipPos(v.pos_MainTex);
                vertRes.uv = v.uv; // 頂點換到Clip空間
                return vertRes;
            }
            float4 fragShader(v2f vertRes) : Color
            {
                half2 TexUV = vertRes.uv;
                //取得主圖的顏色
			    float4 renderTex = tex2D(_MainTex, TexUV);
                //取得三張遮罩指定位置的顏色
                float4 scratchTex = tex2D(_ScratchTex, TexUV);
				
				float4 dustTex = tex2D(_DustTex, TexUV);

				float4 outermostTex = tex2D(_OutermostShader, TexUV);
				
				// 取得亮度YIQ
				float _YIQ = dot(fixed3(0.299, 0.587, 0.114), renderTex.rgb);
				
				float4 finalColor = _YIQ * _OldFilmColor;
				
				// Create a constant white color we can use to adjust opacity of effects
				float3 whiteColor = fixed3(1, 1, 1);
				
				// Composite together the different layers to create final Screen Effect
				finalColor.rgb *= lerp(whiteColor, scratchTex, _ScratchEffectValue);
				finalColor.rgb *= lerp(whiteColor, dustTex, _DustEffectValue);
				finalColor.rgb *= lerp(whiteColor, outermostTex, _OutermostEffectValue);
				finalColor = lerp(finalColor, renderTex, 1.0f-_EffectValue);
				
				
				return finalColor;
            }
            ENDCG
        }
        
    }
    FallBack "Diffuse"
}
