Shader "Veilbound/EnemyArrivalFog" {
 Properties {_Progress("Arrival progress",Float)=0}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;};float _Progress,_VeilClock;
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 fixed4 frag(v2f i):SV_Target {float n=noise(i.world.xz*2+i.uv.y*3-float2(_VeilClock*.2,_VeilClock*.35));float shape=saturate(1-length((i.uv-float2(.5,.35))*float2(2,1.5)));float alpha=shape*shape*(.35+.65*n)*(1-smoothstep(.25,1,_Progress))*.88;float3 colour=lerp(float3(.018,.02,.023),float3(.16,.018,.009),smoothstep(.55,.9,n));return fixed4(colour,alpha);}
 ENDCG}
 }
}
