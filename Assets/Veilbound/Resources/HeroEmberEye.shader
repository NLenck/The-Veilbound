Shader "Veilbound/EmberEye"
{
    Properties
    {
        _MainTex("Eye coordinates",2D)="white"{}
        _EmissionColor("Ember intensity",Color)=(.85,.16,.025,1)
        _VeilTime("Paused character clock",Float)=0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        float4 _EmissionColor;
        float _VeilTime;
        struct Input {float2 uv_MainTex;};
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            float2 uv=IN.uv_MainTex,q=(uv-.5)*2;
            float irisRadius=length(q*float2(2.15,1));
            float iris=1-smoothstep(.55,.78,irisRadius);
            float pupil=1-smoothstep(.15,.23,irisRadius);
            float edge=1-smoothstep(.68,1.01,length(q));
            float cells=frac(sin(dot(floor(uv*27),float2(127.1,311.7)))*43758.5453);
            float grain=.48+.52*smoothstep(.08,.88,cells);
            float striae=.8+.2*sin(atan2(q.y,q.x*2.15)*19+irisRadius*13);
            float coal=saturate(iris-pupil*.94)*edge*grain*striae;
            float heat=(1-smoothstep(.24,.5,irisRadius))*(1-pupil);
            float3 ember=lerp(_EmissionColor.rgb,float3(1,.32,.055)*_EmissionColor.r,heat*.72);
            o.Albedo=lerp(float3(.027,.018,.016),float3(.28,.055,.008),iris*edge);
            o.Emission=ember*(coal*1.55+.012*edge);o.Metallic=0;o.Smoothness=.31;o.Alpha=1;
        }
        ENDCG
    }
    FallBack "Standard"
}
