Shader "WhatTheFish/GolfStormVolume" {
 Properties { _Phase("Phase",Float)=0 _Opacity("Opacity",Range(0,1))=.4 _Softness("Soft haze",Range(0,1))=0 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+8"}
  Pass {
   Tags {"LightMode"="SRPDefaultUnlit"}
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off Cull Back
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float _Phase,_Opacity,_Softness;
   CBUFFER_END
   struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;half4 color:COLOR;};
   struct V {float4 positionCS:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;half4 color:COLOR;};
   V vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.normal=TransformObjectToWorldNormal(i.normalOS);o.world=TransformObjectToWorld(i.positionOS.xyz);o.color=i.color;o.color.rgb=SRGBToLinear(i.color.rgb);return o;}
   half4 frag(V i):SV_Target {
    float light=saturate(dot(normalize(i.normal),normalize(float3(-.5,.85,-.6)))*.5+.5);
    light=lerp(light,floor(light*6)/6,.38);
    float facing=saturate(dot(normalize(i.normal),normalize(_WorldSpaceCameraPos-i.world)));
    float noise=.65+.35*sin(i.world.x*3.1+i.world.z*2.3-_Phase*.9)*sin(i.world.y*4.7+_Phase*.7);
    float haze=pow(facing,1.7)*noise;
    return half4(i.color.rgb*(.72+.34*light),_Opacity*i.color.a*lerp(1,haze,_Softness));
   }
   ENDHLSL
  }
 }
}
