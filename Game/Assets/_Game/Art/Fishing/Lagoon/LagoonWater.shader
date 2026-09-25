Shader "WhatTheFish/LagoonWater" {
 Properties { _BaseMap("Painted water",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1) }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
  Pass {
   Tags { "LightMode"="UniversalForward" }
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseMap_ST;half4 _BaseColor;
   CBUFFER_END
   struct Attributes { float4 positionOS:POSITION;float2 uv:TEXCOORD0; };
   struct Varyings { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;half fog:TEXCOORD2; };
   Varyings vert(Attributes v){Varyings o;VertexPositionInputs p=GetVertexPositionInputs(v.positionOS.xyz);o.positionCS=p.positionCS;o.world=p.positionWS;o.uv=v.uv;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
   half4 frag(Varyings i):SV_Target {
    // Sample all water modules in one coordinate space, including the large ocean
    // quad, so FBX UV packing cannot create a visible seam at module boundaries.
    half3 color=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,(i.world.xz+180)/360).rgb*_BaseColor.rgb;
    float2 p=i.world.xz;
    float wave=sin(p.x*.85+sin(p.y*.42)+_Time.y*.65)*sin(p.y*1.15+_Time.y*.46);
    half glint=pow(saturate(wave),18)*.055;
    color=color*(.98+wave*.022)+glint;
    // The ocean has very distant corner vertices; interpolate world position and
    // evaluate fog per pixel, otherwise its whole quad receives horizon fog.
    half fog=ComputeFogFactor(TransformWorldToHClip(i.world).z);
    return half4(MixFog(color,fog),1);
   }
   ENDHLSL
  }
 }
}
