Shader "WhatTheFish/CoastalSky" {
 Properties { _Zenith("Zenith",Color)=(.14,.46,.80,1) _Horizon("Horizon",Color)=(.66,.84,.94,1) }
 SubShader { Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline"} Cull Off ZWrite Off
 Pass { HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Zenith,_Horizon;
 CBUFFER_END
 struct A{float4 vertex:POSITION;};struct V{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
 V vert(A v){V o;o.pos=TransformObjectToHClip(v.vertex.xyz);o.dir=v.vertex.xyz;return o;}
 half4 frag(V i):SV_Target{float3 d=normalize(i.dir);float h=saturate(d.y);half3 c=lerp(_Horizon.rgb,_Zenith.rgb,pow(h,.55));float sun=pow(saturate(dot(d,normalize(float3(-.35,.65,-.5)))),90);c+=sun*half3(.14,.10,.035);return half4(c,1);}
 ENDHLSL }
 }
}
