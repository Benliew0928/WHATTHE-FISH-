Shader "WhatTheFish/CoastalWater" {
 Properties { _Radii("Island radii",Vector)=(132,160,0,0) }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
 Pass { Tags {"LightMode"="UniversalForward"} Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _Radii;
 CBUFFER_END
 struct A {float4 positionOS:POSITION;};
 struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;};
 V vert(A v){V o;VertexPositionInputs p=GetVertexPositionInputs(v.positionOS.xyz);o.positionCS=p.positionCS;o.world=p.positionWS;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
 half4 frag(V i):SV_Target {
  float2 p=i.world.xz;float2 q=p/_Radii.xy;float angle=atan2(q.y,q.x);
  float radius=1+.035*sin(3*angle+.4)+.023*cos(7*angle);
  float shore=(length(q)/radius-.944)*min(_Radii.x,_Radii.y);
  float depth=saturate(shore/33);depth=smoothstep(0,1,depth);
  half3 color=lerp(half3(.20,.64,.57),half3(.013,.15,.29),depth);
  color=lerp(half3(.57,.62,.43),color,smoothstep(-.4,2.6,shore));
  float t=_Time.y;float a=sin(p.x*.47+p.y*.28+t*.58+noise(p*.055)*2);
  float swell=noise(p*.027+float2(t*.008,0))*.6+noise(p*.067-float2(0,t*.012))*.4;
  color*=.965+swell*.07;
  float b=sin(p.y*.78-p.x*.16-t*.45+sin(p.x*.13)*1.7);
  float closeFade=exp(-distance(_WorldSpaceCameraPos.xz,p)/95);
  float3 normal=normalize(float3(a*.045*closeFade,1,b*.040*closeFade));float3 view=SafeNormalize(_WorldSpaceCameraPos-i.world);
  float fresnel=pow(1-saturate(dot(view,normal)),4);
  color=lerp(color,half3(.42,.71,.84),fresnel*.34);
  float light=pow(saturate(dot(normal,normalize(view+normalize(float3(-.4,.8,-.3))))),180);
  float glints=pow(saturate(a*b),15)*.07*closeFade;
  float ripple=sin(p.x*1.2+sin(p.y*.78+t*.3)*2+t*.48)*sin(p.y*1.0-p.x*.31-t*.35);
  float caustic=pow(saturate(ripple),10)*.13*(1-depth);
  color+=light*.11*closeFade+glints+caustic*closeFade;
  // Irregular shallow shore wash follows exactly the terrain generator's coastline.
  float breakup=noise(p*.62+t*.08);float wash=shore+sin(angle*29+t*.72)*.30+breakup*.55;
  float band=1-smoothstep(.07,.38,abs(wash-(.9+sin(t*.7)*.5)));
  float second=(1-smoothstep(.04,.20,abs(wash-(2.8+sin(t*.55)*.5))))*.34;
  float foam=saturate(max(band,second))*(.64+.36*breakup);
  color=lerp(color,half3(.86,.97,.9),foam*.88);
  half fog=ComputeFogFactor(TransformWorldToHClip(i.world).z);
  return half4(MixFog(color,fog),1);
 }
 ENDHLSL
 }}
}
