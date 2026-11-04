Shader "Veilbound/BronzeCape" {
 Properties { _Color("Cloth", Color)=(.1,.1,.1,1) _MainTex("Texture",2D)="white"{} _Glossiness("Smoothness",Range(0,1))=.1 _Metallic("Metallic",Range(0,1))=0 }
 SubShader { Tags {"RenderType"="Opaque"} LOD 200 Cull Off
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert
 #pragma target 3.0
 sampler2D _MainTex;fixed4 _Color;half _Glossiness,_Metallic;
 struct Input {float2 uv_MainTex;};
 void vert(inout appdata_full v){float loose=saturate((1.50-v.vertex.y)/1.05);v.vertex.z+=(sin(_Time.y*2.5+v.vertex.y*4)-1)*.009*loose*loose;v.vertex.x+=sin(_Time.y*2.1+v.vertex.y*3)*.004*loose;}
 void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=tex2D(_MainTex,IN.uv_MainTex).rgb*_Color.rgb;o.Metallic=_Metallic;o.Smoothness=_Glossiness;o.Alpha=1;}
 ENDCG
 } FallBack "Diffuse"
}
