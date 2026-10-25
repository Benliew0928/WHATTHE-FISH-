Shader "WhatTheFish/UI/LetteringBackground" {
 Properties {
  [PerRendererData] _MainTex ("Artwork",2D)="white" {}
  _CleanTex ("Clean companion",2D)="white" {}
  _StencilComp ("Stencil Comparison",Float)=8
  _Stencil ("Stencil ID",Float)=0
  _StencilOp ("Stencil Operation",Float)=0
  _StencilWriteMask ("Stencil Write Mask",Float)=255
  _StencilReadMask ("Stencil Read Mask",Float)=255
  _ColorMask ("Color Mask",Float)=15
 }
 SubShader {
  Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"}
  Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
  Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
   #include "UnityCG.cginc"
   #include "UnityUI.cginc"
   struct input {float4 vertex:POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;};
   struct output {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;float4 world:TEXCOORD1;};
   sampler2D _MainTex,_CleanTex;float4 _MainTex_TexelSize,_ClipRect,_TextureSampleAdd,_CleanUV;
   float4 _Regions[12];int _RegionCount;
   output vert(input v){output o;o.world=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   fixed4 frag(output i):SV_Target {
    fixed4 original=tex2D(_MainTex,i.uv)+_TextureSampleAdd;
    float mask=0;
    for(int n=0;n<_RegionCount;n++){
     float4 r=_Regions[n];float2 d=min(i.uv-r.xy,r.xy+r.zw-i.uv)/_MainTex_TexelSize.xy;
     mask=max(mask,saturate(min(d.x,d.y)/2));
    }
    fixed4 color=lerp(original,tex2D(_CleanTex,i.uv*_CleanUV.zw+_CleanUV.xy),mask)*i.color;
    #ifdef UNITY_UI_CLIP_RECT
    color.a*=UnityGet2DClipping(i.world.xy,_ClipRect);
    #endif
    return color;
   }
   ENDCG
  }
 }
}
