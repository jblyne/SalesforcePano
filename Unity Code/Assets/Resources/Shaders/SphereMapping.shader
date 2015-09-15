Shader "Custom/ReverseCulling/TexturedTransparent" {
Properties {
	_Color ("Color (float)", color) = (0,0,0,1)
	_MainTex ("Base (RGB)", 2D) = "black" {}
	_BorderTex ("Border Tex(RGB)", 2D) = "white" {}
	_BorderAlpha ("Border Alpha (float)", Float) = 0.0
}

SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	LOD 200
	Cull front
	Blend SrcAlpha OneMinusSrcAlpha
	Pass {
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
			uniform sampler2D _BorderTex;
			uniform float _BorderAlpha;
			uniform float4 _Color;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				float4 border = tex2D(_BorderTex, i.uv);

				if (border.r < 0.5f) {
					col.a = _BorderAlpha;
				}

				return col * _Color;
			}
		ENDCG
		}
	}
}
