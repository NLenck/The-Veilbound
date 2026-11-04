Shader "Veilbound/VillageTextured" {
 Properties {_MainTex("Low-poly village colour",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _Emission("Crystal emission",Color)=(0,0,0,1) _Crystal("Environment crystal",Float)=0 _WallHeightScale("Interior cutaway",Float)=1}
 SubShader {Tags {"RenderType"="Opaque"}
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
 #pragma target 3.0
 sampler2D _MainTex;fixed4 _Color,_Emission;float _WallHeightScale,_Crystal,_VeilClock;
 struct Input {float2 uv_MainTex;float3 worldPos;float3 viewDir;};
 void vert(inout appdata_full v){v.vertex.y*=_WallHeightScale;}
 void surf(Input i,inout SurfaceOutputStandard o){float3 paint=tex2D(_MainTex,float2(i.uv_MainTex.x,1-i.uv_MainTex.y)).rgb;float brightness=dot(paint,float3(.2126,.7152,.0722));float3 hue=lerp(float3(1,1,1),paint/max(brightness,.04),.3);o.Albedo=_Color.rgb*hue*lerp(.78,1.22,saturate(brightness));float rim=pow(1-saturate(dot(normalize(i.viewDir),float3(0,0,1))),2);float pulse=.85+.15*sin(_VeilClock*1.3+i.worldPos.y*.7);o.Emission=_Emission.rgb*lerp(1,(.32+.68*rim)*pulse,_Crystal);o.Metallic=0;o.Smoothness=.08;o.Alpha=1;}
 ENDCG
 }Fallback "Standard"
}
