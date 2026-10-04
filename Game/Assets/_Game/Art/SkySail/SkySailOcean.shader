Shader "WhatTheFish/SkySailOcean" {
 Properties {_GolfColor("Golf shore color",2D)="white"{} _GolfDepth("Golf depth",2D)="white"{} _FishColor("Lagoon shore color",2D)="white"{} _FishDepth("Lagoon depth",2D)="white"{} }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Pass { Tags {"LightMode"="UniversalForward"} Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_GolfColor);SAMPLER(sampler_GolfColor);TEXTURE2D(_GolfDepth);SAMPLER(sampler_GolfDepth);
 TEXTURE2D(_FishColor);SAMPLER(sampler_FishColor);TEXTURE2D(_FishDepth);SAMPLER(sampler_FishDepth);
 float4 _SkySailOrigin,_FishingLagoonCutout;
 struct A {float4 p:POSITION;};struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float fog:TEXCOORD1;};
 V vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
 half4 frag(V i):SV_Target {
  if(_FishingLagoonCutout.w>.5)clip(distance(i.world.xz,_FishingLagoonCutout.xy)-_FishingLagoonCutout.z);
  float2 p=i.world.xz+_SkySailOrigin.xz;float t=_Time.y;
  float2 f=p-float2(-430,0),b=p-float2(430,0);
  float shore=min((length(f/float2(132,160))-1)*132,(length(b/float2(70,83))-1)*70);
  half3 color=lerp(half3(.018,.12,.22),half3(.035,.48,.47),exp(-max(0,shore)/38));
  float gdist=length(p-float2(0,500)),fdist=length(p-float2(0,-340));
  if(gdist<360){float2 uv=(p-float2(0,500))/1000+.5;half3 d=SAMPLE_TEXTURE2D(_GolfDepth,sampler_GolfDepth,uv).rgb;float weight=1-smoothstep(260,350,gdist);color=lerp(color,SAMPLE_TEXTURE2D(_GolfColor,sampler_GolfColor,uv).rgb,weight);shore=lerp(shore,(d.r-.5)*64,weight);}
  if(fdist<145){float2 uv=(p-float2(0,-340))/360+.5;half3 d=SAMPLE_TEXTURE2D(_FishDepth,sampler_FishDepth,uv).rgb;float weight=1-smoothstep(90,140,fdist);color=lerp(color,SAMPLE_TEXTURE2D(_FishColor,sampler_FishColor,uv).rgb,weight);shore=lerp(shore,(d.r-.5)*64,weight);}
  float fade=exp(-distance(_WorldSpaceCameraPos.xz,i.world.xz)/360);
  float a=sin(p.x*.73+p.y*.4+t*.7+noise(p*.08)*3),c=sin(p.y*.86-p.x*.22-t*.6+sin(p.x*.13)*2);
  float3 n=normalize(float3(a*.065*fade,1,c*.06*fade)),v=GetWorldSpaceNormalizeViewDir(i.world);
  float fresnel=pow(1-saturate(dot(v,n)),4);color=lerp(color,half3(.35,.60,.72),fresnel*.3);
  float glint=pow(saturate(dot(n,normalize(v+normalize(float3(-.4,.8,-.3))))),140)*.22;
  color+=(glint+pow(saturate(a*c),12)*.018)*fade;color*=.97+.06*noise(p*.05+t*.004);
  float wash=shore+noise(p*.5+t*.06)*.6;float foam=(1-smoothstep(.06,.48,abs(wash-.6-sin(t*.6)*.3)))*.63;
  color=lerp(color,half3(.80,.91,.84),foam);return half4(MixFog(color,i.fog),1);
 }
 ENDHLSL
 }}
}
