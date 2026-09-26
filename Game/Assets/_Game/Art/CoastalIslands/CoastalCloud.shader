Shader "WhatTheFish/CoastalCloud" {
 Properties { _BaseColor("Cloud",Color)=(1,1,1,1) }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
 Pass {Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;
 CBUFFER_END
 struct A{float4 vertex:POSITION;float3 normal:NORMAL;};struct V{float4 pos:SV_POSITION;float3 n:TEXCOORD0;float3 world:TEXCOORD1;};
 V vert(A v){V o;o.world=TransformObjectToWorld(v.vertex.xyz);o.pos=TransformWorldToHClip(o.world);o.n=TransformObjectToWorldNormal(v.normal);return o;}
 half4 frag(V i):SV_Target{float3 n=normalize(i.n);float lit=saturate(dot(n,normalize(float3(-.4,.8,-.3)))*.5+.5);half3 c=lerp(half3(.57,.71,.84),half3(1,.975,.925),smoothstep(.05,.85,lit));return half4(c*_BaseColor.rgb,1);}
 ENDHLSL }
 }
}
