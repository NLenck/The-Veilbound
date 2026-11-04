Shader "Veilbound/TownSurface" {
 Properties {_Color("Colour",Color)=(.3,.3,.25,1) _Emission("Crystal emission",Color)=(0,0,0,1) _Terrain("Ground",Float)=0 _WallHeightScale("Interior cutaway",Float)=1}
 SubShader {Tags {"RenderType"="Opaque"}
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
 #pragma target 3.0
 fixed4 _Color,_Emission;float _Terrain,_WallHeightScale;
 struct Input {float3 worldPos;};
 void vert(inout appdata_full v){v.vertex.y*=_WallHeightScale;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 void surf(Input i,inout SurfaceOutputStandard o){float n=noise(i.worldPos.xz*.65+i.worldPos.y*.31),b=noise(i.worldPos.xz*.17);float3 material=_Color.rgb*lerp(.84,1.09,n*.65+b*.35);float3 ground=lerp(float3(.13,.135,.085),float3(.22,.19,.125),smoothstep(.2,.8,b*.8+n*.2));o.Albedo=lerp(material,ground,_Terrain);o.Emission=_Emission.rgb;o.Metallic=.02;o.Smoothness=.15;o.Alpha=1;}
 ENDCG
 }Fallback "Standard"
}

