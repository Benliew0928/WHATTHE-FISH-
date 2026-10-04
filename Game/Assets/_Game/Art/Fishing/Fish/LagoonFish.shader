Shader "WhatTheFish/LagoonFish" {
 Properties {_TailMotion("Gentle tail motion",Range(0,1))=1}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass {Tags {"LightMode"="UniversalForward"} Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float _TailMotion;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;half4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;half3 color:TEXCOORD2;float fog:TEXCOORD3;};
   V vert(A a){
    UNITY_SETUP_INSTANCE_ID(a);V o;
    float tail=saturate((-a.p.z+.07)*2);
    float phase=_Time.y*5+a.p.z*7+GetObjectToWorldMatrix()._m03*.7;
    a.p.x+=sin(phase)*tail*tail*.065*_TailMotion;
    a.n=normalize(float3(a.n.x,a.n.y,a.n.z-a.n.x*cos(phase)*tail*.18*_TailMotion));
    VertexPositionInputs p=GetVertexPositionInputs(a.p.xyz);o.p=p.positionCS;o.world=p.positionWS;
    o.normal=TransformObjectToWorldNormal(a.n);o.color=a.color.rgb;o.fog=ComputeFogFactor(p.positionCS.z);return o;
   }
   half4 frag(V i):SV_Target {
    float3 n=normalize(i.normal),v=GetWorldSpaceNormalizeViewDir(i.world);Light sun=GetMainLight();
    half3 lit=i.color*(half3(.48,.57,.61)+sun.color*saturate(dot(n,sun.direction))*.65);
    float highlight=pow(saturate(dot(n,normalize(v+sun.direction))),44)*.24;
    return half4(MixFog(lit+highlight*sun.color,i.fog),1);
   }
   ENDHLSL
  }
 }
}
