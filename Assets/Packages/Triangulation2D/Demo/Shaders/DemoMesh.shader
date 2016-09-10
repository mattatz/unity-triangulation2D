Shader "Triangulation2D/Demo/Mesh" {

	Properties {
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		CGINCLUDE

		#include "UnityCG.cginc"

		#pragma target 3.0

		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		v2f vert (appdata v) {
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.normal = mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz;
			o.uv = v.uv;
			return o;
		}

		fixed3 normal_color (fixed3 norm) {
			return (normalize(norm) + 1.0) * 0.5;
		}

		ENDCG

		Pass {
			Cull Front
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = fixed4(normal_color(i.normal), 1);
				return col;
			}
			ENDCG
		}

		Pass {
			Cull Back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = fixed4(normal_color(-i.normal), 1);
				return col;
			}
			ENDCG
		}

	}
}
