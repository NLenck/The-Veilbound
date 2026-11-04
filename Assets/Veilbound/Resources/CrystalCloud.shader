Shader "Veilbound/CrystalCloud" {
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"}
  Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Back
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
   float _VeilClock;
   struct Input {float4 vertex:POSITION;float4 cluster:TEXCOORD0;};
   struct Output {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float4 cluster:TEXCOORD1;float4 screen:TEXCOORD2;};
   Output vert(Input v){Output o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.cluster=float4(mul(unity_ObjectToWorld,float4(v.cluster.xyz,1)).xyz,v.cluster.w);o.screen=ComputeScreenPos(o.pos);return o;}
   float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
   float noise(float3 p){float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(a),hash(a+float3(1,0,0)),f.x),lerp(hash(a+float3(0,1,0)),hash(a+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(a+float3(0,0,1)),hash(a+float3(1,0,1)),f.x),lerp(hash(a+float3(0,1,1)),hash(a+1),f.x),f.y),f.z);}
   fixed4 frag(Output i):SV_Target {
    float scale=i.cluster.w;float3 forward=-UNITY_MATRIX_V[2].xyz;
    float3 ray=normalize(lerp(i.world-_WorldSpaceCameraPos,forward,unity_OrthoParams.w));
    float3 halfSize=float3(1.8,.85,1.8)*scale;
    float3 origin=i.world+ray*.001,local=origin-i.cluster.xyz;
    float3 safeRay=ray+float3(.00001,.00001,.00001);
    float3 farPlane=max((-halfSize-local)/safeRay,(halfSize-local)/safeRay);
    float distance=max(0,min(farPlane.x,min(farPlane.y,farPlane.z)));
    float rawDepth=SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(i.screen));
    float orthoDepth=rawDepth;
    #if defined(UNITY_REVERSED_Z)
     orthoDepth=1-orthoDepth;
    #endif
    float sceneEye=lerp(LinearEyeDepth(rawDepth),lerp(_ProjectionParams.y,_ProjectionParams.z,orthoDepth),unity_OrthoParams.w);
    float surfaceEye=-mul(UNITY_MATRIX_V,float4(origin,1)).z;
    distance=min(distance,max(0,(sceneEye-surfaceEye)/max(.1,dot(ray,forward))));
    float stepSize=distance/18,opacity=0;float clock=_VeilClock;
    [unroll]for(int k=0;k<18;k++){
     float3 p=origin+ray*((k+.5)*stepSize),v=(p-i.cluster.xyz)/scale;
     float radial=length(v.xz)/1.65,height=v.y/.85;
     float envelope=saturate(1-radial*radial)*saturate(1-height*height);
     float3 drift=float3(clock*.14,-clock*.09,clock*.08);
     float cloud=noise(v*2.5+drift+i.cluster.xyz*.31)*.72+noise(v*5-drift*.6)*.28;
     float density=smoothstep(.27,.7,cloud)*envelope;
     opacity+=(1-opacity)*density*stepSize*1.05/scale;
    }
    return fixed4(.035,.03,.045,min(opacity,.52));
   }
   ENDCG
  }
 }Fallback Off
}
