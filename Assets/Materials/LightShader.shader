Shader "Falloff"
{
    Properties
    {
      _MainTex("Texture", 2D) = "white" {}
      _LightPosition("LightPosition", Vector) = (0,0,0,0)
      _Radius("LightRadius", Float) = 1
      _Falloff("LightFalloff", Range(0, 4)) = 1
      _Color("Light Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {

      Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }

    Pass
    {
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
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
      float3 pos : float3;
    };

    sampler2D _MainTex;
    float4 _MainTex_ST;
    float4 _LightPosition;
    float _Radius;
    float _Falloff;
    fixed4 _Color;

    v2f vert(appdata v)
    {
      v2f o;
      o.vertex = UnityObjectToClipPos(v.vertex);
      o.pos = mul(unity_ObjectToWorld, v.vertex).xyz;
      o.uv = TRANSFORM_TEX(v.uv, _MainTex);
      return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
      fixed4 col = tex2D(_MainTex, i.uv) * _Color;
      float distance = length(_LightPosition - i.pos.xy) / _Radius;
      col.a = max(0, min(1, 1 - pow(distance, _Falloff)));
      return col;
    }
      ENDCG
    }
  }
}