Shader "Hidden/Veilbound/Volume"
{
    Properties { _MainTex("Scene",2D)="white"{} _FogTex("Fog",2D)="black"{} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex,_CameraDepthTexture,_FogTex;
        float4 _MainTex_TexelSize,_View,_ArenaEdges;
        float3 _Origin,_Right,_Up,_Forward;
        float _VeilClock;
        float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
        float noise(float3 p)
        {
            float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);
            return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+float3(1,1,1)),f.x),f.y),f.z);
        }
        float4 volume(v2f_img i):SV_Target
        {
            float2 depthUV=i.uv;
            #if UNITY_UV_STARTS_AT_TOP
            if(_MainTex_TexelSize.y<0)depthUV.y=1-depthUV.y;
            #endif
            float depth=SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,depthUV);
            #if defined(UNITY_REVERSED_Z)
            depth=1-depth;
            #endif
            float distance=min(lerp(_View.z,_View.w,depth),200);
            float3 origin=_Origin+_Right*((i.uv.x-.5)*_View.x)+_Up*((i.uv.y-.5)*_View.y);
            float stride=distance/32,transmittance=1;float3 colour=0;
            for(int s=0;s<32;s++)
            {
                float3 world=origin+_Forward*((s+.5)*stride);
                float radial=smoothstep(_ArenaEdges.x,_ArenaEdges.y,length(world.xz));
                float height=(1-smoothstep(9,21,world.y))*smoothstep(-.6,.15,world.y);
                if(radial*height<.001)continue;
                float3 p=world*.29+float3(_VeilClock*.018,-_VeilClock*.035,_VeilClock*.012);
                float n=noise(p+noise(p*.53)*1.7)*.7+noise(p*2.1)*.3;
                float density=radial*height*(.42+n*.95);
                float alpha=1-exp(-density*stride);
                float3 tint=lerp(float3(.012,.013,.014),float3(.19,.18,.17),smoothstep(.32,.72,n));
                colour+=transmittance*alpha*tint;transmittance*=1-alpha;
                if(transmittance<.012)break;
            }
            return float4(colour,1-transmittance);
        }
        float4 composite(v2f_img i):SV_Target
        {
            float3 scene=tex2D(_MainTex,i.uv).rgb;float4 fog=tex2D(_FogTex,i.uv);
            return float4(scene*(1-fog.a)+fog.rgb,1);
        }
        ENDCG
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment volume
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment composite
            #pragma target 3.0
            ENDCG
        }
    }
}
