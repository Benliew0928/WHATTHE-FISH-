Shader "WhatTheFish/SkySailDistant" {
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Pass { Tags {"LightMode"="UniversalForward"} Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 struct A {float4 position:POSITION;float3 normal:NORMAL;half4 color:COLOR;};
 struct V {float4 position:SV_POSITION;half3 color:COLOR;float fog:TEXCOORD0;};
 V vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.position.xyz);o.position=p.positionCS;Light sun=GetMainLight();half lit=.68+.32*saturate(dot(TransformObjectToWorldNormal(a.normal),sun.direction));o.color=a.color.rgb*lit;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
 half4 frag(V i):SV_Target{return half4(MixFog(i.color,i.fog),1);}
 ENDHLSL
 }}
}
