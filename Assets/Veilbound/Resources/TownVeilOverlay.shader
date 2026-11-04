Shader "Veilbound/TownVeilOverlay" {
 SubShader {Tags {"Queue"="Overlay"} Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
 Pass {CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 float _Clock;
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 fixed4 frag(v2f_img i):SV_Target{float2 uv=i.uv;float edge=max(abs(uv.x-.5)*2,abs(uv.y-.5)*2);float n=noise(uv*12+float2(_Clock*.012,-_Clock*.008))*.65+noise(uv*23-float2(_Clock*.007,0))*.35;float bank=.12+.88*smoothstep(.25,.72,n);float outer=smoothstep(.64,.98,edge)*bank;float distant=exp(-pow((uv.y-.74)/.10,2))*smoothstep(.06,.18,abs(uv.x-.56))*bank;float3 col=lerp(float3(.16,.17,.18),float3(.50,.51,.52),n);float alpha=max(outer*.86,distant*.70);float2 cell=floor(uv*float2(55,36));float spark=hash(cell);float2 centre=frac(uv*float2(55,36))-float2(.5,.5+.25*sin(_Clock*.4+spark*20));float glow=pow(saturate(1-length(centre)*5),2)*step(.985,spark)*smoothstep(.45,.85,edge)*(.65+.35*sin(_Clock*1.5+spark*30));col=lerp(col,float3(.9,.16,.025),glow);alpha=max(alpha,glow*.85);return fixed4(col,alpha);}
 ENDCG}
 }
}
