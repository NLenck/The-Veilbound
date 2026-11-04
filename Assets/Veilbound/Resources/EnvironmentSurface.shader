Shader "Veilbound/EnvironmentSurface" {
 Properties { _Color("Painted colour",Color)=(.25,.25,.22,1) _Glossiness("Smoothness",Range(0,1))=.16 _Terrain("Earth and moss",Float)=0 }
 SubShader {
  Tags { "RenderType"="Opaque" }
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows
  #pragma target 3.0
  fixed4 _Color;half _Glossiness,_Terrain;
  struct Input {float3 worldPos;};
  float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
  float softNoise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
  void surf(Input i,inout SurfaceOutputStandard o){float2 p=i.worldPos.xz+i.worldPos.y*.37;float fine=softNoise(p*.73),broad=softNoise(p*.19);float patina=fine*.65+broad*.35;float3 stone=_Color.rgb*lerp(.84,1.10,patina);float soil=smoothstep(.30,.78,broad*.8+fine*.2);float3 earth=lerp(float3(.105,.135,.065),float3(.205,.185,.12),soil)*lerp(.92,1.06,fine);o.Albedo=lerp(stone,earth,_Terrain);o.Smoothness=_Glossiness;o.Metallic=.025;o.Alpha=1;}
  ENDCG
 }
 Fallback "Standard"
}
