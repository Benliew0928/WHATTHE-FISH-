Shader "WhatTheFish/RefinedFoliage" {
 Properties {
  _BaseMap("Albedo",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1)
  _BumpMap("Normal",2D)="bump"{} _BumpScale("Normal strength",Float)=.4
  _MetallicGlossMap("Metallic and smoothness",2D)="white"{} _Metallic("Metallic",Float)=0 _Smoothness("Smoothness scale",Float)=1
  _OcclusionMap("Occlusion",2D)="white"{} _OcclusionStrength("Occlusion strength",Float)=1
  _EmissionMap("Emission",2D)="black"{} _EmissionColor("Emission tint",Color)=(0,0,0,0)
  _SpecColor("Specular",Color)=(.2,.2,.2,1) _Cutoff("Cutoff",Float)=.5
  _Cull("Cull",Float)=0 _Surface("Surface",Float)=0 _AlphaClip("Clip",Float)=0
 }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Cull Off
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 float4 _GolfWind,_GolfWindArea,_GolfTornado0,_GolfTornado1;
 float2 TornadoPull(float2 world,float4 storm){float2 d=storm.xy-world;float f=saturate(1-dot(d,d)/max(.01,storm.z*storm.z))*storm.w;float2 n=d*rsqrt(max(.04,dot(d,d)));return (n*.7+float2(-n.y,n.x)*.45)*f*f;}
 struct WindInput {float4 p:POSITION;float3 n:NORMAL;float4 tangent:TANGENT;float2 uv:TEXCOORD0;float2 bend:TEXCOORD1;UNITY_VERTEX_INPUT_INSTANCE_ID};
 float3 IslandWind(float3 p,float2 uv,float2 bend){
  float3 world=TransformObjectToWorld(p);float event=saturate(_GolfWind.z)*step(distance(world.xz,_GolfWindArea.xy),_GolfWindArea.z);
  float grass=step(40,bend.y);float flex=lerp(smoothstep(.03,1,uv.y),saturate(bend.x),grass);
  float t=lerp(_Time.y,_GolfWind.w,event);float ambient=sin(world.x*.23+world.z*.17+t*1.1)*.6+sin(world.x*.79-world.z*.5+t*1.8)*.4;
  world.xz+=float2(.045,.023)*ambient*lerp(1,flex,event*grass);
  float phase=dot(world.xz,_GolfWind.xy)*.65-t*2.4;float gust=.78+.22*sin(phase);
  float2 across=float2(-_GolfWind.y,_GolfWind.x);
  world.xz+=event*flex*flex*(lerp(.42,.24,grass)*gust*_GolfWind.xy+across*sin(phase*2.3)*.045);
  world.xz+=(TornadoPull(world.xz,_GolfTornado0)+TornadoPull(world.xz,_GolfTornado1))*flex*flex*lerp(.55,.35,grass);
  return TransformWorldToObject(world);
 }

 ENDHLSL
 Pass {Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma target 3.0
 #pragma vertex WindVertex
 #pragma fragment LitPassFragment
 #pragma shader_feature_local _NORMALMAP
 #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
 #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
 #pragma multi_compile_fog
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
 Varyings WindVertex(WindInput input){UNITY_SETUP_INSTANCE_ID(input);Attributes a=(Attributes)0;UNITY_TRANSFER_INSTANCE_ID(input,a);a.positionOS=float4(IslandWind(input.p.xyz,input.uv,input.bend),1);a.normalOS=input.n;a.tangentOS=input.tangent;a.texcoord=input.uv;a.staticLightmapUV=input.bend;return LitPassVertex(a);}
 ENDHLSL }
 Pass {Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ZWrite On ZTest LEqual ColorMask 0
 HLSLPROGRAM
 #pragma target 3.0
 #pragma vertex WindShadow
 #pragma fragment ShadowPassFragment
 #pragma multi_compile_instancing
 #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
 #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
 Varyings WindShadow(WindInput input){UNITY_SETUP_INSTANCE_ID(input);Attributes a=(Attributes)0;UNITY_TRANSFER_INSTANCE_ID(input,a);a.positionOS=float4(IslandWind(input.p.xyz,input.uv,input.bend),1);a.normalOS=input.n;a.texcoord=input.uv;return ShadowPassVertex(a);}
 ENDHLSL }
 Pass {Name "DepthOnly" Tags {"LightMode"="DepthOnly"} ZWrite On ColorMask R
 HLSLPROGRAM
 #pragma target 3.0
 #pragma vertex WindDepth
 #pragma fragment DepthOnlyFragment
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
 Varyings WindDepth(WindInput input){UNITY_SETUP_INSTANCE_ID(input);Attributes a=(Attributes)0;UNITY_TRANSFER_INSTANCE_ID(input,a);a.position=float4(IslandWind(input.p.xyz,input.uv,input.bend),1);a.texcoord=input.uv;return DepthOnlyVertex(a);}
 ENDHLSL }
 Pass {Name "DepthNormals" Tags {"LightMode"="DepthNormals"} ZWrite On
 HLSLPROGRAM
 #pragma target 3.0
 #pragma vertex WindNormals
 #pragma fragment DepthNormalsFragment
 #pragma shader_feature_local _NORMALMAP
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
 Varyings WindNormals(WindInput input){UNITY_SETUP_INSTANCE_ID(input);Attributes a=(Attributes)0;UNITY_TRANSFER_INSTANCE_ID(input,a);a.positionOS=float4(IslandWind(input.p.xyz,input.uv,input.bend),1);a.normal=input.n;a.tangentOS=input.tangent;a.texcoord=input.uv;return DepthNormalsVertex(a);}
 ENDHLSL }
 }
}
