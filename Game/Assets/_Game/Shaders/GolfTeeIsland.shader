Shader "WhatTheFish/GolfTeeIsland" {
 Properties {
  _BaseMap("Macro color",2D)="white"{} _Control("Surface weights",2D)="black"{}
  _Turf("Turf detail",2D)="white"{} _Sand("Sand detail",2D)="white"{}
  _TurfNormal("Turf normal",2D)="bump"{} _SandNormal("Sand normal",2D)="bump"{}
  _Tile("Tile metres",Float)=4 _BaseColor("Tint",Color)=(1,1,1,1)
  _T1Data("T1 cup X/Z, angle, radius",Vector)=(0,0,0,0)
  _IslandExtent("Original terrain UV extent",Float)=220
 }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
 Pass {Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);TEXTURE2D(_Control);SAMPLER(sampler_Control);
 TEXTURE2D(_Turf);SAMPLER(sampler_Turf);TEXTURE2D(_Sand);SAMPLER(sampler_Sand);
 TEXTURE2D(_TurfNormal);SAMPLER(sampler_TurfNormal);TEXTURE2D(_SandNormal);SAMPLER(sampler_SandNormal);
 CBUFFER_START(UnityPerMaterial)
 float _Tile;float4 _BaseColor;float4 _T1Data;float _IslandExtent;
 CBUFFER_END
 struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;half4 c:COLOR;float2 surface:TEXCOORD1;};
 struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;float fog:TEXCOORD3;half4 c:COLOR;float surface:TEXCOORD4;};
 V vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.n=TransformObjectToWorldNormal(a.n);o.uv=a.uv;o.c=a.c;o.surface=a.surface.x;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
 // Evaluate the narrow path and planting at pixel resolution. Interpolating a
 // three-metre path only at the island's coarse vertices creates colour blocks.
 void TeePaint(float2 uv,out half4 paint,out half surface){
  float2 p=(uv-.5)*(_IslandExtent*2),d=p-_T1Data.xy;
  paint=0;surface=0;
  if(p.x < -86 || p.x > 26 || p.y < -170 || p.y > -74)return;
  float r=length(d),delta=atan2(d.y,d.x)-_T1Data.z;
  delta-=floor((delta+PI)/(2*PI))*(2*PI);
  float edge=1-smoothstep(max(1,r*.275-3),r*.275+2,abs(delta)*r);
  float opening=smoothstep(5,10,r)*(1-smoothstep(66,72,r))*edge;
  float reclaim=(1-smoothstep(14,19,abs(p.x+27)))*(1-smoothstep(3,7,abs(p.y+142)));
  float noise=.012*sin(p.x*.24)*sin(p.y*.31)+.007*cos(p.x*.77+p.y*.19);
  float stripe=sin((p.y+p.x*.18)*.3)>0?1:0;
  half3 lawn=SRGBToLinear(half3(.5255,.698,.2863)+noise+stripe*half3(.015,.025,.009));
  float bunker=smoothstep(38,40,r)*(1-smoothstep(43,45,r))*edge;
  float rough=smoothstep(22,24,r)*(1-smoothstep(26,28,r))*edge;
  half3 tint=lerp(lawn,SRGBToLinear(half3(103,149,68)/255),rough*.65);
  tint=lerp(tint,SRGBToLinear(half3(232,207,157)/255),bunker);
  float path=(1-smoothstep(1.1,2.1,abs(r-75.5)))*(1-smoothstep(.30,.36,abs(delta)));
  float bend=(p.x+64)/18;bend*=bend;
  float pathZ=p.y-(p.y<-100?6*exp(-bend*bend):0);
  path*=1-smoothstep(1.005,1.024,length(float2(p.x/145,pathZ/156)));
  tint=lerp(tint,SRGBToLinear(half3(214,194,154)/255),path);
  float restore=1-smoothstep(.94,1.04,length(float2((p.x+43)/23,(p.y+112.5)/18)));
  paint=half4(tint,max(opening,reclaim)*(1-restore));paint.a=max(paint.a,path*(1-restore));
  surface=lerp(bunker,path,path);
 }
 half4 frag(V i):SV_Target {
  half4 paint=i.c;half surface=i.surface;if(_T1Data.w>0)TeePaint(i.uv,paint,surface);
  float2 uv=i.world.xz/_Tile;half sand=SAMPLE_TEXTURE2D(_Control,sampler_Control,i.uv).r;
  sand=lerp(sand,surface,paint.a);
  half3 turf=SAMPLE_TEXTURE2D(_Turf,sampler_Turf,uv).rgb,grit=SAMPLE_TEXTURE2D(_Sand,sampler_Sand,uv).rgb;
  // Normalize by the measured PNG mean, decoded into Unity's linear colour space.
  half3 variation=lerp(turf/SRGBToLinear(half3(.409,.582,.236)),grit/SRGBToLinear(half3(.815,.711,.523)),sand);
  variation=1+(variation-1)*2.2;
  half3 macro=lerp(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb,paint.rgb,paint.a);
  half3 albedo=macro*clamp(variation,.7,1.3)*_BaseColor.rgb;
  half3 bump=normalize(lerp(UnpackNormal(SAMPLE_TEXTURE2D(_TurfNormal,sampler_TurfNormal,uv)),UnpackNormal(SAMPLE_TEXTURE2D(_SandNormal,sampler_SandNormal,uv)),sand));
  InputData d=(InputData)0;d.positionWS=i.world;d.normalWS=normalize(i.n+float3(bump.x,0,bump.y)*.38);d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);
  d.shadowCoord=TransformWorldToShadowCoord(i.world);d.bakedGI=SampleSH(d.normalWS);d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);d.shadowMask=1;
  SurfaceData s=(SurfaceData)0;s.albedo=albedo;s.alpha=1;s.occlusion=1;s.smoothness=lerp(.16,.07,sand);s.normalTS=half3(0,0,1);
  half4 color=UniversalFragmentPBR(d,s);color.rgb=MixFog(color.rgb,i.fog);return color;
 }
 ENDHLSL }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 UsePass "Universal Render Pipeline/Lit/DepthNormals"
 }
}
