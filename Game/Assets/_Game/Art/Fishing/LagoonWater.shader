Shader "WhatTheFish/LagoonWater" {
 Properties {
  _ShallowColor("Water tint",Color)=(.07,.58,.59,1)
  _DeepColor("Deeper tint",Color)=(.025,.29,.38,1)
  _Opacity("Underwater visibility",Range(0,1))=.28
  _WaveSpeed("Ripple speed",Float)=1
  _ShoreMap("Existing ocean colours",2D)="white"{}
 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-20"}
  Pass {
   Tags {"LightMode"="UniversalForward"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_ShoreMap);SAMPLER(sampler_ShoreMap);
   CBUFFER_START(UnityPerMaterial)
   half4 _ShallowColor,_DeepColor;float _Opacity,_WaveSpeed;
   CBUFFER_END
   struct A {float4 p:POSITION;};
   struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float2 local:TEXCOORD1;float fog:TEXCOORD2;};
   V vert(A a){
    V o;o.local=a.p.xz;float t=_Time.y*_WaveSpeed;
    float fade=1-smoothstep(24,27.8,length(a.p.xz));
    a.p.y+=(sin(a.p.x*.92+a.p.z*.46+t*1.1)*.023+sin(a.p.z*1.27-a.p.x*.34-t*.85)*.013)*fade;
    VertexPositionInputs p=GetVertexPositionInputs(a.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.fog=ComputeFogFactor(p.positionCS.z);return o;
   }
   half4 frag(V i):SV_Target {
    float2 p=i.local;float t=_Time.y*_WaveSpeed,r=length(p);
    float a=p.x*1.7+p.y*.72+t*1.14+sin(p.y*.61+t*.35)*1.3+sin(p.x*.42-t*.13)*.7;
    float b=p.y*2.15-p.x*.48-t*.92+sin(p.x*.53+t*.22)*1.6;
    float c=p.x*4.8+p.y*3.2+t*1.56+sin(b)*.3;
    float nearFade=exp(-distance(_WorldSpaceCameraPos,i.world)/95);
    float3 n=normalize(float3((cos(a)*.12-cos(b)*.043+cos(c)*.032)*nearFade,1,
      (cos(a)*.052+cos(b)*.14+cos(c)*.023)*nearFade));
    float3 v=GetWorldSpaceNormalizeViewDir(i.world);
    float fresnel=.02+.98*pow(1-saturate(dot(v,n)),5);
    Light sun=GetMainLight();
    float spec=pow(saturate(dot(n,normalize(v+sun.direction))),180)*.45;
    float broad=pow(saturate(dot(n,normalize(v+sun.direction))),28)*.035;
    float depth=1-smoothstep(8,28,r);
    half3 tint=lerp(_ShallowColor.rgb,_DeepColor.rgb,depth*.45);
    // Soft sky reflection stays legible on mobile without a second scene render.
    half3 sky=lerp(half3(.36,.66,.8),half3(.8,.91,.94),pow(saturate(n.z*.8+n.x*.35+.38),3));
    half3 color=lerp(tint,sky,fresnel*.8)+(spec+broad)*sun.color;
    float edge=smoothstep(25.5,27.9,r);
    float foam=(1-smoothstep(.04,.22,abs(r-27.05-sin(p.x*.8+p.y*.7+t*.8)*.12)))*.13;
    color=lerp(color,half3(.8,.95,.91),foam);
    float alpha=saturate(_Opacity+depth*.12+fresnel*.48+spec*.25+foam);
    // The edge meets the opaque inlet water under the surrounding shoreline.
    alpha=lerp(alpha,1,edge);
    half3 shore=SAMPLE_TEXTURE2D(_ShoreMap,sampler_ShoreMap,p/360+.5).rgb;
    shore=lerp(shore,half3(.35,.60,.72),fresnel*.3);
    color=lerp(color,shore,edge);
    return half4(MixFog(color,i.fog),alpha);
   }
   ENDHLSL
  }
 }
}
