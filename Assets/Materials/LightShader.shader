Shader "Unlit/LightShader"
{
	Properties
	{
        _Color ("Light Color", Color) = (1, 1, 0, 1)
        _LightCenter ("Light Center", Vector) = (0, 0, 0, 0)
        _LightRadius ("Light Radius", float) = 10
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		LOD 100

        ZWrite Off
        Blend DstAlpha One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
                float4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
                float4 color : COLOR;
			};

            fixed4 _Color;
            fixed4 _LightCenter;
            float _LightRadius;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.color.a = 1 - (length(_LightCenter.xyz - v.vertex.xyz) / _LightRadius);
                //o.distance = length(v.vertex.xyz - _LightCenter.xyz);
				return o;
			}
			
            fixed4 frag (v2f i) : SV_Target
			{
                fixed4 col = _Color;
                
                float distance = length(_LightCenter.xyz - i.vertex.xyz);
                //col.a = 1 - (distance / _LightRadius);
				return i.color;
			}
			ENDCG
		}
	}
}

