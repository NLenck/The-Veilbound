Shader "Veilbound/AmbientEmbers" {
 Properties { _Color("Ember tint",Color)=(1,.38,.10,.85) }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
  Blend SrcAlpha One ZWrite Off Cull Off
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   fixed4 _Color;
   struct Input {float4 vertex:POSITION;float2 uv:TEXCOORD0;float2 size:TEXCOORD1;fixed4 color:COLOR;};
   struct Output {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   Output vert(Input v){Output o;float3 world=mul(unity_ObjectToWorld,v.vertex).xyz;float3 right=UNITY_MATRIX_I_V._m00_m10_m20;float3 up=UNITY_MATRIX_I_V._m01_m11_m21;world+=right*((v.uv.x-.5)*v.size.x)+up*((v.uv.y-.5)*v.size.y);o.pos=UnityWorldToClipPos(world);o.uv=v.uv;o.color=v.color*_Color;return o;}
   fixed4 frag(Output i):SV_Target{float2 p=i.uv*2-1;float fade=saturate(1-dot(p,p));return fixed4(i.color.rgb,i.color.a*fade*fade);}
   ENDCG
  }
 }
}
