#ifndef WTF_IDENTITY_INPUT
#define WTF_IDENTITY_INPUT
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
TEXTURE2D(_MetallicGlossMap); SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_Numbers); SAMPLER(sampler_Numbers);
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor,_JerseyColor,_SkinTint,_AccentColor,_HairColor;
half _BumpScale,_Metallic,_Smoothness,_IdentityNumber,_ShowOriginalHair,_Team,_HideFirstPersonHead;
CBUFFER_END
struct IdentityAttributes {float4 positionOS:POSITION;float3 normalOS:NORMAL;float4 tangentOS:TANGENT;float2 uv:TEXCOORD0;float4 number:TEXCOORD2;half4 regions:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
struct IdentityVaryings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float3 positionWS:TEXCOORD1;half3 normalWS:TEXCOORD2;half4 tangentWS:TEXCOORD3;float4 number:TEXCOORD4;half4 regions:COLOR;half fog:TEXCOORD5;half3 vertexLighting:TEXCOORD6;half firstPersonHead:TEXCOORD7;UNITY_VERTEX_OUTPUT_STEREO};
void IdentityClip(half hair,float2 uv,half sideHead){
 clip(_ShowOriginalHair+.49h-hair);
 // Color boundaries on the fused source mesh cross triangles around the ears.
 // Resolve these tiny teal/pink strands per pixel, keeping the detailed ear skin.
 if(_ShowOriginalHair<.5h&&sideHead>.5h){half3 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).rgb;clip((c.r>c.g*1.15h&&c.g>c.b*1.6h)?1:-1);}
}
IdentityVaryings IdentityVertex(IdentityAttributes v){
 IdentityVaryings o=(IdentityVaryings)0;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
 VertexPositionInputs p=GetVertexPositionInputs(v.positionOS.xyz);VertexNormalInputs n=GetVertexNormalInputs(v.normalOS,v.tangentOS);
 o.positionCS=p.positionCS;o.positionWS=p.positionWS;o.normalWS=n.normalWS;o.tangentWS=half4(n.tangentWS,v.tangentOS.w*GetOddNegativeScale());o.uv=v.uv;o.firstPersonHead=v.number.w;o.number=v.number;o.number.w=step(v.number.z,-.5);o.regions=v.regions;o.fog=ComputeFogFactor(p.positionCS.z);o.vertexLighting=VertexLighting(p.positionWS,n.normalWS);return o;
}
half NumberInk(float4 coordinates){
 float2 uv=coordinates.xy;if(coordinates.z<.5||any(uv<0)||any(uv>1))return 0;
 float halfIndex=step(.5,uv.x);float digit=halfIndex>0?fmod(_IdentityNumber,10):floor(_IdentityNumber/10);
 float2 digitUV=float2((digit+frac(uv.x*2))*.1,uv.y);
 return SAMPLE_TEXTURE2D(_Numbers,sampler_Numbers,digitUV).r;
}
void FirstPersonHeadClip(IdentityVaryings i){clip(.5h-_HideFirstPersonHead*i.firstPersonHead);}
half4 IdentityFragment(IdentityVaryings i):SV_Target {
 FirstPersonHeadClip(i);
 IdentityClip(i.regions.g,i.uv,i.number.w);
 half4 base=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*_BaseColor;
 half luminance=dot(base.rgb,half3(.2126,.7152,.0722));
 half3 jersey=_JerseyColor.rgb*(.55+.45*luminance);
 // One/two narrow chest bands give the team a redundant non-color cue.
 half band=(_Team>0&&i.number.z>.5)?step(.935,i.number.y)*step(i.number.y,.98):0;
 if(_Team>1)band=max(band,step(.85,i.number.y)*step(i.number.y,.895)*step(.5,i.number.z));
 jersey=lerp(jersey,half3(.95,.97,1),band*.8);
 // Albedo samples and material colors are linear; normalize in the same space.
 half3 skin=base.rgb*_SkinTint.rgb/half3(1,.515,.253);
 half3 albedo=lerp(base.rgb,jersey,i.regions.r);albedo=lerp(albedo,skin,i.regions.b);
 half fringe=step(.1,i.regions.g)*(1-step(.49,i.regions.g))*step(3.95,i.number.y)*step(base.r*1.15,base.g)*step(base.r*1.15,base.b)*(1-_ShowOriginalHair);albedo=lerp(albedo,_HairColor.rgb*(.65+.35*luminance),fringe);
 albedo=lerp(albedo,_AccentColor.rgb*(.65+.35*luminance),i.regions.a*.75);
 half ink=NumberInk(i.number)*i.regions.r;albedo=lerp(albedo,half3(1,1,1),ink);
 half4 metallic=SAMPLE_TEXTURE2D(_MetallicGlossMap,sampler_MetallicGlossMap,i.uv);
 half3 tangentNormal=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,i.uv),_BumpScale);
 half3 normal=normalize(i.normalWS);half3 tangent=normalize(i.tangentWS.xyz);half3 bitangent=cross(normal,tangent)*i.tangentWS.w;
 normal=NormalizeNormalPerPixel(TransformTangentToWorld(tangentNormal,half3x3(tangent,bitangent,normal)));
 InputData input=(InputData)0;input.positionWS=i.positionWS;input.normalWS=normal;input.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);input.shadowCoord=TransformWorldToShadowCoord(i.positionWS);input.bakedGI=SampleSH(normal);input.vertexLighting=i.vertexLighting;input.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);input.shadowMask=half4(1,1,1,1);
 SurfaceData surface=(SurfaceData)0;surface.albedo=albedo;surface.metallic=metallic.r*_Metallic;surface.smoothness=metallic.a*_Smoothness;surface.normalTS=tangentNormal;surface.occlusion=1;surface.alpha=1;
 half4 color=UniversalFragmentPBR(input,surface);color.rgb=MixFog(color.rgb,i.fog);return color;
}
#endif
