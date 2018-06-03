
//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

// Template of a custom function for MegaSplat. Included is the definition of the structures you will use,
// though these may be out of date since they are just comments. The latest structure is alwasy stored in
// megasplat_structs.txt inside the fragment folder.
//
// A separate file is also written out which allows you to insert properties as well. Remember to declare
// the variables in this file as well!
//
//  The MegaSplatLayer structure is similar to a surface shader struct, but contains height for the given surface as well.
//
//         struct MegaSplatLayer
//         {
//            half3 Albedo;
//            half3 Normal;
//            half3 Emission;
//            half  Metallic;
//            half  Smoothness;
//            half  Occlusion;
//            half  Height;
//            half  Alpha;
//         };
//
//
//  The SplatInput structure contains useful information you may need, sich as the macro and splat UVs, viewDir, 
//  distance from camera, etc. Note that many values are compiled out of the structure when unused, so please
//  observer those same conditional compiles in your custom functions, or you may break your shader when changing
//  options.
//
//
//         struct SplatInput
//         {
//            float3 weights;
//            float2 splatUV;
//            float2 macroUV;
//            float3 valuesMain;
//            half3 viewDir;
//            float4 camDist;
//
//            #if _TWOLAYER || _ALPHALAYER
//            float3 valuesSecond;
//            half layerBlend;
//            #endif
//
//            #if _TRIPLANAR
//            float3 triplanarUVW;
//            #endif
//            half3 triplanarBlend; // passed to func, so always present
//
//            #if _FLOW || _FLOWREFRACTION || _PUDDLES || _PUDDLEFLOW || _PUDDLEREFRACT
//            half2 flowDir;
//            #endif
//
//            #if _PUDDLES || _PUDDLEFLOW || _PUDDLEREFRACT
//            half puddleHeight;
//            #endif
//
//            #if _WETNESS
//            half wetness;
//            #endif
//
//            #if _TESSDAMPENING
//            half displacementDampening;
//            #endif
//
//            #if _SNOW
//            half snowHeightFade;
//            #endif
//         };

sampler2D _Custom_MyTexture;


// this function is called in the vertex shader, before anything has been computed.
// You can do things like modify the local position of the vertex (before triplanar texturing), etc..
void CustomMegaSplatFunction_PreVertex(inout float4 localPosition, inout float3 normal, inout float4 tangent, inout float2 uv)
{
   localPosition.y += sin(_Time.x * 10 + localPosition.x);
}

// this function is called after the tesselated vertex displacement has been calculated, if tessellation is enabled.
// you must apply the offset if you want to see it, it's provided here so you can modify it further. 
void CustomMegaSplatFunction_PostDisplacement(inout float3 position, float3 offset, inout float3 normal, inout float4 tangent, inout float2 uv)
{
   // default implimentation is to apply the displacement offset unmodified
   position += offset;
}

// this function is called at the end of the pipeline, so you can modify the color, normal, smoothness, 
// etc, before lighting is applied.
void CustomMegaSplatFunction_Final(SplatInput si, inout MegaSplatLayer o)
{
   half4 tex = tex2D(_Custom_MyTexture, si.macroUV * 20);
   o.Albedo *= tex.rgb;
}