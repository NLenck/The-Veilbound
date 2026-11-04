Shader "Veilbound/TownGroundContact" {
 Properties { _MainTex ("Texture",2D)="white" {} }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off ZTest Always
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target {float2 p=(i.uv-.5)*2;float d=dot(p,p);float edge=1-smoothstep(.3,1,d);float core=exp(-d*4);return fixed4(.045,.038,.022,edge*(.16+.23*core));}
 ENDCG }
 }
}
