Shader "Custom/DistanceFade"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_DitherPattern("Dithering Pattern", 2D) = "White" {}
		_MinDistance("Minimum Fade Distance", Float) = 0
		_MaxDistance("Maximum Fade Distance", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"= "Geometry" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;

		//Dithering pattern
		sampler2D _DitherPattern;
		float4 _DitherPattern_TexelSize;

		//remapping of distance
		float _MinDistance;
		float _MaxDistance;

        struct Input
        {
            float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 screenPos;
        };

        fixed4 _Color;

		//the object data that's put into the vertex shader
		struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		//the data that's used to generate fragments and can be read by the fragment shader
		struct v2f {
			float4 position : SV_POSITION;
			float2 uv : TEXCOORD0;
			float4 screenPosition : TEXCOORD1;
		};
		v2f vert(appdata v) {
			v2f o;
			o.position.w;
			return o;
		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			//read dexture and write it to diffuse color
			float3 texColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = texColor.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			
			//value from the dither pattern
			float2 m_screenPos = IN.screenPos.xy / IN.screenPos.w;
			
			float2 m_ditherCoordinate = m_screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
			float m_ditherValue = tex2D(_DitherPattern, m_ditherCoordinate).r;

			//get relative distance from the camera
			float m_relDistance = IN.screenPos.w;
			m_relDistance = m_relDistance - _MinDistance;
			m_relDistance = m_relDistance / (_MaxDistance - _MinDistance);
			//discard pixels accordingly
			clip(m_relDistance - m_ditherValue);
			
        }
        ENDCG
    }
    FallBack "Standard"
}