Shader "WhatTheFish/SkySailCable" {
 SubShader {
  Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
  Pass {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct Attributes {float4 positionOS:POSITION;float3 normalOS:NORMAL;};
   struct Varyings {float4 positionCS:SV_POSITION;half fog:TEXCOORD0;half shade:TEXCOORD1;};
   Varyings vert(Attributes input){
    Varyings output;
    float3 p=TransformObjectToWorld(input.positionOS.xyz);
    float3 n=TransformObjectToWorldNormal(input.normalOS);
    // Preserve a readable cable silhouette as the physical radius becomes subpixel.
    // The close cabin and pulley contact retain the authored 47 mm radius.
    float depth=max(0,-TransformWorldToView(p).z);
    // A small angular floor also works in offscreen cameras and batch reviews,
    // whose screen-size uniforms need not match the render target.
    float radius=max(.047,depth*.00038);
    p+=n*min(radius-.047,1.2);
    output.positionCS=TransformWorldToHClip(p);
    output.fog=ComputeFogFactor(output.positionCS.z);
    output.shade=.78+.22*saturate(dot(n,normalize(float3(-.3,.8,.4))));
    return output;
   }
   half4 frag(Varyings input):SV_Target{return half4(MixFog(half3(.16,.23,.24)*input.shade,input.fog),1);}
   ENDHLSL
  }
 }
}
