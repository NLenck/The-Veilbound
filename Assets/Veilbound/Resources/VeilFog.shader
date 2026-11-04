Shader "Veilbound/LivingFog"
{
    Properties { _VeilTime("Veil time",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _VeilTime;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float3 normal:NORMAL; };
            struct v2f { float4 pos:SV_POSITION; float3 world:TEXCOORD0; float2 uv:TEXCOORD1; float ground:TEXCOORD2; };
            v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.uv=v.uv;o.ground=step(.5,abs(v.normal.y));return o;}
            float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
            float noise(float3 p)
            {
                float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+float3(1,1,1)),f.x),f.y),f.z);
            }
            fixed4 frag(v2f i):SV_Target
            {
                float3 p=i.world*.38+float3(_VeilTime*.035,-_VeilTime*.09,_VeilTime*.02);
                float n=noise(p)*.6+noise(p*2.13)*.27+noise(p*4.17)*.13;
                float vertical=1-smoothstep(.35+n*.25,1,i.uv.y);
                float horizontal=smoothstep(0,.34,i.uv.y)*(1-smoothstep(.8,1,i.uv.y));
                float edge=lerp(vertical,horizontal,i.ground);
                float density=saturate((n-.1)*2.1)*edge;
                float3 colour=lerp(float3(.012,.013,.014),float3(.12,.115,.11),n);
                return fixed4(colour,density*.96);
            }
            ENDCG
        }
    }
}
