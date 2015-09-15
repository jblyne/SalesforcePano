Shader "Custom/SmoothSquare" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BorderTex("Border Texture", 2D) = "white" {}
		
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
		
		CGPROGRAM
		#pragma surface surf Lambert alpha

		sampler2D _MainTex;
		sampler2D _BorderTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BorderTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			half4 b = tex2D (_BorderTex, IN.uv_BorderTex);
			o.Albedo = c.rgb;
			if (b.r < 0.5f) {
				o.Alpha = 0.0f;
			}
			else {
				o.Alpha = c.a;
			}
		}
		ENDCG
	} 
	Fallback "Transparent/VertexLit"
}
