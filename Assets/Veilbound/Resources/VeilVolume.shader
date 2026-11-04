Shader "Hidden/Veilbound/Volume"
{
    Properties { _MainTex("Scene",2D)="white"{} _FogTex("Fog",2D)="black"{} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex,_CameraDepthTexture,_FogTex;
        float4 _MainTex_TexelSize,_View,_ArenaEdges,_InteriorMist;
        float3 _Origin,_Right,_Up,_Forward;
        float _VeilClock,_VillageCrimson;
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
            // The offset camera can sit outside the boundary while viewing playable ground.
            // Keep that foreground storm out of sightlines ending inside the arena.
            float3 scenePoint=origin+_Forward*distance;
            float stormSightline=smoothstep(_ArenaEdges.x-.6,_ArenaEdges.x+1.4,length(scenePoint.xz));
            float stride=distance/32,transmittance=1;float3 colour=0;
            for(int s=0;s<32;s++)
            {
                float3 world=origin+_Forward*((s+.5)*stride);
                float radial=smoothstep(_ArenaEdges.x,_ArenaEdges.y,length(world.xz))*stormSightline;
                float height=(1-smoothstep(9,21,world.y))*smoothstep(-.6,.15,world.y);
                if(radial*height<.001)continue;
                float3 p=world*.29+float3(_VeilClock*.018,-_VeilClock*.035,_VeilClock*.012);
                float n=noise(p+noise(p*.53)*1.7)*.7+noise(p*2.1)*.3;
                float density=radial*height*(.42+n*.95);
                float alpha=1-exp(-density*stride);
                float3 tint=lerp(float3(.012,.013,.014),float3(.19,.18,.17),smoothstep(.32,.72,n));
                float crimson=smoothstep(.44,.77,noise(p*.68+float3(1.3,_VeilClock*.012,2.1)));
                float3 villageTint=lerp(float3(.014,.009,.011),float3(.25,.033,.019),crimson);
                tint=lerp(tint,villageTint,_VillageCrimson*.78);
                colour+=transmittance*alpha*tint;transmittance*=1-alpha;
                if(transmittance<.012)break;
            }
            // Analytic low ground layer: opacity-capped and depth-clipped, without another raymarch.
            if(_InteriorMist.x>0 && abs(_Forward.y)>.001)
            {
                float a=(_InteriorMist.y-origin.y)/_Forward.y,b=(.06-origin.y)/_Forward.y;
                float start=max(0,min(a,b)),end=min(distance,max(a,b)),path=max(0,end-start);
                float3 mid=origin+_Forward*((start+end)*.5);
                float inner=1-smoothstep(_ArenaEdges.x-1.5,_ArenaEdges.x+1.5,length(mid.xz));
                if(path>0 && inner>0)
                {
                    float n=noise(float3(mid.x*.12+_VeilClock*.014,.47,mid.z*.12-_VeilClock*.009));
                    float broad=noise(float3(mid.x*.055-_VeilClock*.006,.21,mid.z*.055));
                    float wisps=.05+.95*smoothstep(.34,.66,n*.7+broad*.3);
                    float alpha=min(_InteriorMist.z,(1-exp(-_InteriorMist.x*path))*wisps*inner);
                    colour+=transmittance*alpha*float3(.52,.56,.55);transmittance*=1-alpha;
                }
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
