Shader "WhatTheFish/RefinedWater" {
 Properties {_BaseMap("Depth color",2D)="white"{} _DepthMap("Shore distance and depth",2D)="white"{} _Extent("Map half extent",Float)=500 _Calm("Lagoon calmness",Float)=.35 _Cascade("Cascade",Float)=0}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
 Pass {Tags {"LightMode"="UniversalForward"} Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);TEXTURE2D(_DepthMap);SAMPLER(sampler_DepthMap);
 CBUFFER_START(UnityPerMaterial)
 float _Extent,_Calm,_Cascade;
 CBUFFER_END
 float4 _FishingLagoonCutout;
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;};struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float2 uv:TEXCOORD1;float fog:TEXCOORD2;};
 V vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.uv=a.uv;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
 half4 frag(V i):SV_Target {
  if(_FishingLagoonCutout.w>.5)clip(distance(i.world.xz,_FishingLagoonCutout.xy)-_FishingLagoonCutout.z);
  float2 p=i.world.xz,uv=p/(_Extent*2)+.5;half3 depth=SAMPLE_TEXTURE2D(_DepthMap,sampler_DepthMap,uv).rgb;
  float shore=(depth.r-.5)*64,calm=lerp(1,_Calm,depth.b),t=_Time.y;
  half3 color=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).rgb;
  float a=sin(p.x*.82+p.y*.41+t*.65*calm+noise(p*.085)*3),b=sin(p.y*.99-p.x*.18-t*.58*calm+sin(p.x*.14)*2);
  float nearFade=exp(-distance(_WorldSpaceCameraPos.xz,p)/145);
  float3 n=normalize(float3(a*.055*calm*nearFade,1,b*.05*calm*nearFade)),v=GetWorldSpaceNormalizeViewDir(i.world);
  color*=1+(-.04+.08*noise(p*.04+float2(t*.005*calm,0)))*nearFade;
  float fresnel=pow(1-saturate(dot(v,n)),4);color=lerp(color,half3(.42,.69,.82),fresnel*.34);
  float glint=pow(saturate(dot(n,normalize(v+normalize(float3(-.4,.8,-.3))))),180)*.1;
  float caustic=pow(saturate(sin(p.x*1.8+sin(p.y*.9+t*.3))*sin(p.y*1.4-p.x*.4-t*.35)),9)*.13;
  color+=(glint+caustic*(1-depth.g))*nearFade;
  float wash=shore+noise(p*.65+t*.06)*.55+sin(p.x*.2+p.y*.3+t*.7)*.2;
  float foam=(1-smoothstep(.08,.42,abs(wash-(.7+sin(t*.6)*.3))))*.7;
  foam*=.65+.35*noise(p*1.7+t*.15);color=lerp(color,half3(.85,.94,.88),foam);
  if(_Cascade>.5){
   float stream=noise(float2(i.uv.x*23,i.uv.y*5-t*1.25));float lace=pow(saturate(noise(float2(i.uv.x*51+stream*3,i.uv.y*2-t*2))),3);
   color=lerp(half3(.12,.49,.48),half3(.73,.91,.84),stream*.65+lace*.28);
   float splash=smoothstep(.78,1,frac(i.uv.y))*(.35+.35*noise(i.uv*43+t));
   color=lerp(color,half3(.9,.98,.96),splash);
  }
  return half4(MixFog(color,i.fog),1);
 }
 ENDHLSL }
 }
}
