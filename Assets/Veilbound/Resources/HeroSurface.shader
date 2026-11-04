Shader "Veilbound/CharacterSurface"
{
    Properties
    {
        _ClothColor("Cloth",Color)=(.34,.045,.041,1)
        _MetalColor("Armour",Color)=(.36,.4,.46,1)
        _ArmourTier("Armour tier",Float)=0
        _VeilPulse("Occasional ember pulse",Range(0,1))=0
        _VeilTime("Paused character clock",Float)=0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0
        fixed4 _ClothColor,_MetalColor;
        float _ArmourTier,_VeilPulse,_VeilTime;
        struct Input { float3 rest;float3 viewDir; };
        void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.rest=v.texcoord1.xyz;}
        float band(float value,float centre,float halfWidth,float soft){return 1-smoothstep(halfWidth,halfWidth+soft,abs(value-centre));}
        float between(float x,float a,float b,float s){return smoothstep(a,a+s,x)*(1-smoothstep(b-s,b,x));}
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            float3 p=IN.rest;
            float3 dark=float3(.018,.021,.027),skin=float3(.53,.43,.35),shirt=float3(.28,.28,.26);
            float head=smoothstep(1.64,1.68,p.y);
            float arms=smoothstep(.263,.285,abs(p.x))*smoothstep(.72,.77,p.y)*(1-smoothstep(1.58,1.625,p.y));
            float exposed=max(head,arms);
            float3 colour=lerp(float3(.075,.082,.095),shirt,smoothstep(1.005,1.04,p.y));
            colour=lerp(colour,float3(.045,.039,.039),1-smoothstep(.32,.35,p.y));
            colour=lerp(colour,skin,exposed);
            float front=smoothstep(.083,.105,p.z)*head;
            float socket=band(abs(p.x),.052,.022,.006)*band(p.y,1.937,.007,.004)*front;
            colour=lerp(colour,dark,socket*.64);
            float eye=band(abs(p.x),.052,.017,.003)*band(p.y,1.936,.0042,.0015)*front;
            colour=lerp(colour,float3(.32,.065,.012),eye);
            float pupil=band(abs(p.x),.052,.004,.0015)*band(p.y,1.936,.004,.0015)*front;
            colour=lerp(colour,float3(.57,.15,.025),pupil);
            float browShape=saturate(1-pow((abs(p.x)-.055)/.033,2));
            float browY=1.957+.007*browShape-.03*(abs(p.x)-.052);
            float brow=band(p.y,browY,.0014+.0018*browShape,.0018)*browShape*front;
            colour=lerp(colour,dark,brow*.9);
            float lipHeight=1.825+.0025*pow(saturate(abs(p.x)/.034),1.6);
            float mouth=band(p.x,0,.030,.005)*band(p.y,lipHeight,.0016,.0015)*front;
            colour=lerp(colour,float3(.27,.16,.13),mouth*.78);
            float stubble=between(p.y,1.748,1.819,.016)*head*smoothstep(.065,.093,p.z);
            colour=lerp(colour,float3(.12,.095,.08),stubble*.12);
            float armPath=.425+.008*sin(p.y*41)+.003*sin(p.y*103);
            float armCrack=band(p.x,armPath,.0011,.0013)*between(p.y,1.005,1.19,.02)*smoothstep(.10,.13,p.z);
            float armBranch=band(p.x,armPath+.5*(p.y-1.115),.0009,.0012)*between(p.y,1.105,1.15,.009)*smoothstep(.10,.13,p.z);
            float facePath=-.086+.20*(p.y-1.94)+.002*sin(p.y*119);
            float faceCrack=band(p.x,facePath,.0008,.0012)*between(p.y,1.90,1.982,.008)*smoothstep(.066,.09,p.z);
            float neckCrack=band(p.x,-.039+.13*(p.y-1.71)+.002*sin(p.y*133),.0009,.0012)*between(p.y,1.668,1.75,.009)*smoothstep(.052,.07,p.z);
            float cracks=saturate(max(max(armCrack,armBranch),max(faceCrack,neckCrack)))*exposed;
            float pulse=_VeilPulse;
            colour=lerp(colour,float3(.24,.055,.035),cracks*(.07+pulse*.30));
            float rim=pow(1-saturate(dot(normalize(IN.viewDir),float3(0,0,1))),3.5)*exposed;
            float noise=sin(p.x*211+p.y*97)*sin(p.z*183-p.y*57);
            o.Albedo=colour*(1+noise*.015);o.Metallic=0;o.Smoothness=lerp(.17,.25,exposed);
            float eyeFlicker=.045*sin(_VeilTime*3.1)+.025*sin(_VeilTime*7.7+.8);
            o.Emission=float3(.9,.035,.012)*(cracks*pulse*2.1+rim*(.012+.065*pulse));
            o.Emission+=float3(.82,.12,.018)*eye*(.08+.08*pulse+eyeFlicker*.1);o.Alpha=1;
        }
        ENDCG
    }
    FallBack "Standard"
}
