Shader "Veilbound/EnvironmentCrystalGlow" {
 SubShader {
  Tags {"Queue"="Transparent-1" "RenderType"="Transparent"}
  Blend SrcAlpha One ZWrite Off Cull Off
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   float _VeilClock;
   struct Output {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;};
   Output vert(appdata_base v){Output o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord.xy;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
   fixed4 frag(Output i):SV_Target {float2 p=i.uv*2-1;float fade=saturate(1-dot(p,p));float pulse=.85+.15*sin(_VeilClock*1.3+i.world.y*.7);return fixed4(.38,.003,.018,fade*fade*.65*pulse);}
   ENDCG
  }
 }Fallback Off
}
