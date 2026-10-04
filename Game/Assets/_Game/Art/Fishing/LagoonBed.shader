Shader "WhatTheFish/LagoonBed" {
 Properties {_BaseMap("Shared sand",2D)="white"{} _BaseColor("Submerged sand",Color)=(.32,.74,.71,1)}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass {Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;};struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float fog:TEXCOORD2;};
   V vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.normal=TransformObjectToWorldNormal(a.n);o.fog=ComputeFogFactor(p.positionCS.z);return o;}
   half4 frag(V i):SV_Target {
    float2 p=i.world.xz;float t=_Time.y;
    half3 sand=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p*.25).rgb*_BaseColor.rgb;
    float u=sin(p.x*2.2+sin(p.y*1.5+t*.6)+t*.5);
    float v=sin(p.y*2.6+sin(p.x*1.8-t*.5)-t*.4);
    float caustic=pow(saturate(1-abs(u+v)*.75),14)*.055;
    caustic*=exp(-distance(_WorldSpaceCameraPos,i.world)/45)*(.55+.45*sin(p.x*.39+p.y*.26+t*.2)*sin(p.x*.39+p.y*.26+t*.2));
    Light sun=GetMainLight();sand*=.65+.35*saturate(dot(normalize(i.normal),sun.direction));
    return half4(MixFog(sand+caustic*half3(.66,1,.87),i.fog),1);
   }
   ENDHLSL
  }
 }
}
