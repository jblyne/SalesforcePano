Shader "Custom/Learning/FlatColor" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_SpecColor ("Specular", Color) = (1,1,1,1)
		_Shininess ( "Shininess", Float) = 10
		_RimColor ("Rim Color", Color) = (1,1,1,1)
		_RimPower ("RimPower", Range(0.1, 10.0)) = 3.0
	}
	SubShader {
		Tags { "LightMode" = "ForwardBase" }
		Pass {
			CGPROGRAM
			//Pragmas
			#pragma vertex vert
			#pragma fragment frag

			//User defined variables
			uniform float4 _Color;
			uniform float4 _SpecColor;
			uniform float4 _RimColor;
			uniform float _Shininess;
			uniform float _RimPower;
			
			//unity defined variables
			uniform float4 _LightColor0;
			
			//Base Import structs
			struct vertexInput {
				float4 vertex	: POSITION;
				float3 normal	: NORMAL;
				float2 texCoord : TEXCOORD;
			};
			struct vertexOutput {
				float4 pos		: SV_POSITION;
				float4 posWorld : TEXCOORD0;
				float3 normDir	: TEXCOORD1;
			};
			
			//vertex function
			vertexOutput vert(vertexInput v) {
				vertexOutput o;
				
				o.posWorld = mul(_Object2World, v.vertex);
				o.normDir = normalize(mul(float4(v.normal, 0.0), _World2Object).xyz);
								
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;				
			}

			//fragment function
			float4 frag(vertexOutput i) : COLOR
			{
				//Vectors
				float3 viewDirection = normalize( _WorldSpaceCameraPos.xyz - i.posWorld.xyz );
				float3 lightDirection;
				float atten = 1.0;

				//Lighting
				lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 lambert = max(0.0, dot(i.normDir, lightDirection));
				float3 diffuseReflection = atten * _LightColor0.xyz * _Color.rgb * lambert;
				float3 specularReflection = lambert * pow(max(0.0, dot(reflect(-lightDirection, i.normDir), viewDirection)), _Shininess);
				
				float3 lightFinal = diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;				

				return float4(lightFinal * _Color.rgb, 1.0);
			}
			ENDCG
		}
		//Fallback "Diffuse"
	}
}