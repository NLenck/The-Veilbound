Shader "Veilbound/MonsterSurface" {
 Properties {_Color("Surface tint",Color)=(1,1,1,1) _EmissionColor("Ember and hit flash",Color)=(0,0,0,1) _Glossiness("Smoothness",Range(0,1))=.12 _Metallic("Metallic",Range(0,1))=0}
 SubShader { Tags {"RenderType"="Opaque"} LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 fixed4 _Color;half4 _EmissionColor;half _Glossiness,_Metallic;
 struct Input {float4 color:COLOR;};
 void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=IN.color.rgb*_Color.rgb;o.Emission=_EmissionColor.rgb;o.Metallic=_Metallic;o.Smoothness=_Glossiness;o.Alpha=1;}
 ENDCG
 } FallBack "Diffuse"
}
