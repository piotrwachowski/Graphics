# Upgrading from HDRP 8.x to 10.x

In the High Definition Render Pipeline (HDRP), some features work differently between major versions. This document helps you upgrade HDRP from 8.x to 10.x.

## Constant Buffer API

From 10.x, HDRP uses a new constant buffer API that allows it to set up uniforms during the frame and send them to the shader in a single transfer instead of multiple transfers. To do this, the global variables that were declared individually are now all within the `ShaderVariablesGlobal` struct. The consequence of this is that it's no longer possible to set up any of the global values individually using `CommandBuffer.SetVectorXXX()` or its related functions. Instead, to change a global variable, you need to update the struct in its entirety.

Currently, the only publicly accessible variables in the `ShaderVariablesGlobal` struct are camera related and only available within [Custom Passes](Custom-Pass.md), using the following functions:

* `RenderFromCamera()`
* `RenderDepthFromCamera()`
* `RenderNormalFromCamera()`
* `RenderTangentFromCamera()`


## Frame Settings

From 10.x, if you create a new [HDRP Asset](HDRP-Asset.md), the **MSAA Within Forward** Frame Setting is enabled by default.

## Menu

From 10.x, several HDRP menu items in **Assets** > **Create** > **Shader** are renamed to **HD Render Pipeline** for consistency.

## Decal

From 10.x, decals no longer require a full Depth Prepass. HDRP only renders Materials with **Receive Decals** enabled during the Depth Prepass, unless other options force it.

From 10.x, you can use the Decal Layers system, which makes use of the **Rendering Layer Mask** property from a Mesh Renderer and Terrain. The default value of this property before 2020.2 doesn't include any Decal Layer flags. This means that when you enable this feature, no Meshes receive decals until you configure them correctly.

* To convert all Meshes, go to **Edit** > **Render Pipeline/HD Render Pipeline** > **Upgrade from Previous Version** > **Add HDRP Decal Layer Default to Loaded Mesh Renderers and Terrains**.

To convert selected Meshes, go to **Edit** > **Render Pipeline/HD Render Pipeline** > **Upgrade from Previous Version** > **Add HDRP Decal Layer Default to Selected Mesh Renderers and Terrains**.

Newly created Mesh Renderer or Terrain components have **Decal Layer Default** enabled by default.

## Lighting

From 10.x, when you create a Spot Light from the Editor menu, HDRP enables the **Reflector** property by default.

**Note**: If you create a Spot Light via a C# script, this property is disabled.

From 10.x, HDRP disables [Backplate](Override-HDRI-Sky.md) rendering for lighting cubemaps that aren't compatible.

From 10.x, [Screen Space Ambient Occlusion](Override-Ambient-Occlusion.md), [Screen Space Global Illumination](Override-Screen-Space-GI.md), [Screen Space Reflection](Override-Screen-Space-Reflection.md), [Ray Tracing Effects](Ray-Tracing-Getting-Started.md), and [Volumetric Reprojection](Override-Fog.md) don't interact with Reflection Probes as they don't work correctly.

From 10.x, if you disable the sky override used as the **Static Lighting Sky** in the **Lighting** window, the sky no longer affects the baked lighting. Previously, the sky affected the baked lighting even when it was disabled.

From 10.x, HDRP has the Cubemap Array for Point [Light](Light-Component.md) cookies and now uses octahedral projection with a regular 2D-Cookie atlas. This is to allow for a single path for light cookies and IES, but it may produce visual artifacts when using a low-resolution cube-cookie. For example, projecting pixel art data.

As the **Cubemap cookie atlas** no longer exists, it's possible that HDRP doesn't have enough space on the current 2D atlas for the cookies. If this is the case, HDRP displays an error in the Console window. To fix this, increase the size of the 2D cookie atlas. To do this:

1. Select your [HDRP Asset](HDRP-Asset.md).
2. In the Inspector, go to **Lighting** > **Cookies**.
3. In the 2D Atlas Size drop-down, select a larger cookie resolution.

From 10.x, the texture format of the color buffer in the HDRP Asset also applies to [Planar Reflection Probes](Planar-Reflection-Probe.md). Previously, Planar Reflection Probes always used a float16 rendertarget.

From 10.x, the light layer properties have moved from the HDRP settings to the HDRP Default settings Panel.

From 10.x, in Physically Based Sky, the sun disk intensity is proportional to its size. Before this the sun disk was incorrectly not drive by the sun disk size.

From 10.x, if you previously used the **UseEmissiveIntensity** property in either a Decal, Lit, LayeredLit, or Unlit Material, be aware that the **EmissiveColorLDR** property is in sRGB color space. Previously, HDRP handled **EmissiveColor** as being in linear RGB color space so there may be a mismatch in visuals between versions. To fix this mismatch, HDRP includes a migration script that handles the conversion automatically.

For projects migrating from the old 9.x.x-preview package. There is a change in the order of enum of Exposure that may shift the current exposure mode to another one in Exposure Volume. You need to correct this manually by reselecting the correct Exposure mode.

From 10.x, the debug lens attentuation property no longer exists, however you can set the lens attenuation in the HDRP Default setting Panel as either modelling a perfect lens or an imperfect one.

From 10.x, the [Screen Space Reflection](Override-Screen-Space-Reflection.md) effect always uses the color pyramid HDRP generates after the Before Refraction transparent pass. This means the color buffer only includes transparent GameObjects that use the **BeforeRefraction** [Rendering Pass](Surface-Type.md). Previously the content depended on whether the Distortion effect was active.

## Volumetric Fog

When upgrading a project to 10.2, the quality of volumetric fog in your Scene may degrade. This is because of the new volumetric fog control modes. To make volumetric fog look the same as it did in 8.x:

1. In the [Fog](Override-Fog.md) Volume Override, set **Fog Control Mode** to **Manual**.
2. For the properties this mode exposes, enter the same values as you had in 8.x

Alternatively, set **Fog Control Mode** to **Balance** and use the new performance-oriented properties to define the quality of the volumetric fog.

## Shadows

From 10.x, you no longer need to change the [HDRP Config package](HDRP-Config-Package.md) to set the [shadow filtering quality](HDRP-Asset.md#filtering-qualities) for deferred rendering. Instead, you can change the filtering quality directly on the [HDRP Asset](HDRP-Asset.md#filtering-qualities).

**Note**: If you previously hadn't set the shadow filtering quality to **Medium** on the HDRP Asset, the automatic project upgrade process changes the shadow quality which means you may need to manually change it back to its original value.

HDRP now stores OnEnable and OnDemand shadows in a separate atlas and more API is available to handle them. For more information, see [Shadows in HDRP](Shadows-in-HDRP.md).

The shader function `SampleShadow_PCSS` now requires you to pass in an additional float2 parameter which contains the shadow atlas resolution in x and the inverse of the atlas resolution in y.

Contact shadows now have Ray bias and thickness parameters. These might lead to small changes to the visual impact of contact shadows with the default parameters. Please consider tuning those values to fit the needs of your project.

## Shader config file

From 10.x, due to the change of the shadow map, the HDShadowFilteringQuality enum is now in HDShadowManager.cs. `ShaderConfig.s_DeferredShadowFiltering` and `ShaderOptions.DeferredShadowFiltering` are no longer in the source code because they have no effect anymore.

From 10.x, a new option named **ColoredShadow** is available. It allows you to control whether a shadow is chromatic or monochrome. **ColoredShadow** is enabled by default and currently only works with [Ray-traced shadows](Ray-Traced-Shadows.md).

**Note**: Colored shadows are more resource intensive to process than standard shadows.

From 10.x, the Raytracing option and equivalent generated shader macro `SHADEROPTIONS_RAYTRACING` are removed. You no longer need to edit the shader config file to use ray-traced effects in HDRP.

## Shader code

From 10.x, HDRP uses a new structure to output information from the LightLoop. It now uses a custom LightLoop struct instead of the `float3 diffuseLighting`, `float3 specularLighting` pair. This is to allow HDRP to export more information from the LightLoop in the future without breaking the API.

The following functions now pass this structure instead of the pair:

* `LightLoop()`, for both rasterization and raytracing.
* `PostEvaluateBSDF()`
* `ApplyDebug()`
* `PostEvaluateBSDFDebugDisplay()`

To upgrade an existing shader, for all the above functions:

1. Replace the declaration `float3 diffuseLighting; float3 specularLighting;` with `LightLoopOutput lightLoopOutput;` before the LightLoop call.
2. Replace the argument pair `out float3 diffuseLighting, out float3 specularLighting` with `out LightLoopOutput lightLoopOutput`.



The prototype for the function `void ModifyBakedDiffuseLighting(float3 V, PositionInputs posInput, SurfaceData surfaceData, inout BuiltinData builtinData)` in the various materials is now `void ModifyBakedDiffuseLighting(float3 V, PositionInputs posInput, PreLightData preLightData, BSDFData bsdfData, inout BuiltinData builtinData)`.

There is also a new definition for `ModifyBakedDiffuseLighting()` that uses the former prototype definition and calls the new function prototype with the correct arguments. The purpose of this change it to prepare for future lighting features. To update your custom shaders, in addition of the prototype update, you must remove the following lines:

```
BSDFData bsdfData = ConvertSurfaceDataToBSDFData(posInput.positionSS, surfaceData);

PreLightData preLightData = GetPreLightData(V, posInput, bsdfData);
```

From 10.x, HDRP includes a new rectangular area shadow evaluation function, `EvaluateShadow_RectArea`. The `GetAreaLightAttenuation()` function is renamed to `GetRectAreaShadowAttenuation()`. Also the type `DirectionalShadowType` is renamed `SHADOW_TYPE`.

From 10.x, the macro `ENABLE_RAYTRACING`, `SHADEROPTIONS_RAYTRACING`, and `RAYTRACING_ENABLED` are removed. A new multicompile is introduce for forward pass: `SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON`. This allow to enable raytracing effect without requiring edition of shader config file.

From 10.x,`SHADERPASS` for TransparentDepthPrepass and TransparentDepthPostpass identification is using respectively `SHADERPASS_TRANSPARENT_DEPTH_PREPASS` and `SHADERPASS_TRANSPARENT_DEPTH_POSTPASS`. Previously it was `SHADERPASS_DEPTH_ONLY`. Define `CUTOFF_TRANSPARENT_DEPTH_PREPASS` and `CUTOFF_TRANSPARENT_DEPTH_POSTPASS` and are removed as the new path macro can now be used.

10.x introduces a new multi-compile for Depth Prepass and Motion vector pass to allow for support of the Decal Layers feature. These passes now require you to add `#pragma multi_compile _ WRITE_DECAL_BUFFER`.

From 10.x, the shader code for the Decal.shader has changed. Previously, the code used around 16 passes to handle the rendering of different decal attributes. It now only uses four passes: DBufferProjector, DecalProjectorForwardEmissive, DBufferMesh, DecalMeshForwardEmissive. Some pass names are also different. DBufferProjector and DBufferMesh now use a multi_compile DECALS_3RT DECALS_4RT to handle the differents variants and the shader stripper is updated as well. Various Shader Decal Properties are renamed or changed to match a new set of AffectXXX properties (`_AlbedoMode`, `_MaskBlendMode`, `_MaskmapMetal`, `_MaskmapAO`, `_MaskmapSmoothness`, `_Emissive` are changed to `_AffectAlbedo`, `_AffectNormal`, `_AffectAO`, `_AffectMetal`, `_AffectSmoothness`, `_AffectEmission` - Keyword `_ALBEDOCONTRIBUTION` is now `_MATERIAL_AFFECTS_ALBEDO` and two new keywords, `_MATERIAL_AFFECTS_NORMAL`, `_MATERIAL_AFFECTS_MASKMAP`, are added). These new properties now match with properties from the Decal Shader Graph which are now exposed in the Material. A Material upgrade process automatically upgrades all the Decal Materials. However, if your project includes any C# scripts that create or manipulate a Decal Material, you need to update the scripts to use the new properties and keyword; the migration doesn't work on procedurally generated Decal Materials.

From 10.x, HDRP changed the shader code for the decal application inside a material. In previous versions, HDRP performed an optimization named "HTile", which relied on an HTileMask. This optimization is no longer beneficial so HDRP has removed all the code relating to it. This includes the HTileMask member in DecalSurfaceData and the DBufferHTileBit structure and the associated flag.

To update your custom shaders:

1. Remove the following defines that no longer exist:
 - `DBUFFERHTILEBIT_DIFFUSE`
 - `DBUFFERHTILEBIT_NORMAL`
 - `DBUFFERHTILEBIT_MASK`
2. Check if the weight of individual attributes is non-neutral. For example in your `ApplyDecalToSurfaceData()` function, replace the following lines:

```
    if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_DIFFUSE)
    {
      (...)
    }

    if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_NORMAL)
    {
      (...)
    }

    if (decalSurfaceData.HTileMask & DBUFFERHTILEBIT_MASK)
    {
        (...) ComputeFresnel0((decalSurfaceData.HTileMask & DBUFFERHTILEBIT_DIFFUSE) ? (...));
    }
```
with

```
    if (decalSurfaceData.baseColor.w  < 1.0)
    {
      (...)
    }

    if (decalSurfaceData.normalWS.w < 1.0)
    {
      (...)
    }

    if (decalSurfaceData.MAOSBlend.x < 1.0 || decalSurfaceData.MAOSBlend.y < 1.0 || decalSurfaceData.mask.w)
    {
        (...) ComputeFresnel0((decalSurfaceData.baseColor.w  < 1.0) ? (...));
    }
```

For an example of best practices to apply decals to a material, see the `ApplyDecalToSurfaceData()` function in the LitDecalData.hlsl file.

From 10.x, a new vertex normal parameter is added to the decal functions prototype in shader code to allow it to handle angle based fading.
The prototype for the function `GetDecalSurfaceData()` has changed from:
`DecalSurfaceData GetDecalSurfaceData(PositionInputs posInput, inout float alpha)`
to:
 `DecalSurfaceData GetDecalSurfaceData(PositionInputs posInput, float3 vtxNormal, inout float alpha)`

The prototype for the function `ApplyDecalToSurfaceData()` in various Material has changed from:
`void ApplyDecalToSurfaceData(DecalSurfaceData decalSurfaceData, inout SurfaceData surfaceData)`
to:
 `void ApplyDecalToSurfaceData(DecalSurfaceData decalSurfaceData, float3 vtxNormal, inout SurfaceData surfaceData)`

From 10.x, HDRP adds a new fullscreen debug pass named `FullScreenDebug`. Any object using a material based on a shader which doesn't contain this pass will not be rendered during the fullscreen debug pass.

From Unity 2020.2, the Raytracing keyword in Shader Graph is renamed to Raytracing Quality and the `RAYTRACING_SHADER_GRAPH_LOW` and `RAYTRACING_SHADER_GRAPH_HIGH` defines are now `RAYTRACING_SHADER_GRAPH_DEFAULT` and `RAYTRACING_SHADER_GRAPH_RAYTRACED` respectively. Unless you used these defines in custom Shader code, you don't need to do anything because Shader Graph automatically regenerates its Shaders with the correct defines when you load the Project.

From Unity 2020.2, a parameter `positionNDC` is added to the function `SampleEnv`. It's prototype has changed from:
`float4 SampleEnv(LightLoopContext lightLoopContext, int index, float3 texCoord, float lod, float rangeCompressionFactorCompensation, int sliceIdx = 0)`
to:
`float4 SampleEnv(LightLoopContext lightLoopContext, int index, float3 texCoord, float lod, float rangeCompressionFactorCompensation, float2 positionNDC, int sliceIdx = 0)`
For example, the call in the Lit shader is updated to:
`float4 preLD = SampleEnv(lightLoopContext, lightData.envIndex, R, PerceptualRoughnessToMipmapLevel(preLightData.iblPerceptualRoughness), lightData.rangeCompressionFactorCompensation, posInput.positionNDC);`

From 10.x, the shader keywords `_BLENDMODE_ALPHA _BLENDMODE_ADD` and `_BLENDMODE_PRE_MULTIPLY` are removed. They're no longer used and the property `_Blendmode` is used instead.
Similarly the shader keyword `_BLENDMODE_PRESERVE_SPECULAR_LIGHTING` is removed and in its place the property `_EnableBlendModePreserveSpecularLighting` is used in shader code.

For example in Material.hlsl, the following lines:

```
    #if defined(_BLENDMODE_ADD) || defined(_BLENDMODE_ALPHA)
        return float4(diffuseLighting * opacity + specularLighting, opacity);
```
are replaced by
```
    if (_BlendMode == BLENDMODE_ALPHA || _BlendMode == BLENDMODE_ADDITIVE)
        return float4(diffuseLighting * opacity + specularLighting * (
#ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
        _EnableBlendModePreserveSpecularLighting ? 1.0f :
#endif
            opacity), opacity);

```

This reduces the number of shader variants. For custom shaders, you might need to move the include of Material.hlsl after the declaration of the property `_Blendmode`. Also, if you want the custom shader to support the blend mode **Preserve Specular** option, it needs to make sure `_EnableBlendModePreserveSpecularLighting` property is defined and that the compile time constant `SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING` is defined too.


From 10.x, HDRP includes a new optimization for [Planar Reflection Probes](Planar-Reflection-Probe.md). Now, when a shader samples a probe's environment map, it samples from mip level 0 if the LightData.roughReflections parameter is enabled (has a value of 1.0). You must update your custom shaders to take this behavior into account.
For example, the call in the Lit shader is updated to:
`float4 preLD = SampleEnv(lightLoopContext, lightData.envIndex, R, PerceptualRoughnessToMipmapLevel(preLightData.iblPerceptualRoughness) * lightData.roughReflections, lightData.rangeCompressionFactorCompensation, posInput.positionNDC);`
In addition a new functionality to fake distance based roughness is added on Reflection Probe and a new helper function is introduced. The code is updated to:
`float4 preLD = SampleEnvWithDistanceBaseRoughness(lightLoopContext, posInput, lightData, R, preLightData.iblPerceptualRoughness, intersectionDistance);`
in the Lit shader. `intersectionDistance` is the return parameter of the `EvaluateLight_EnvIntersection()` function.

From 10.x, HDRP uses range remapping for the metallic property when using a mask map.
In the Lit, LitTessellation, LayeredLit, and LayeredLitTesselation shaders, two new properties are added:

* `_MetallicRemapMin`
* `_MetallicRemapMax`

In the Decal shader, the property `_MetallicRemapMin` is added, and `_MetallicScale` is renamed to `_MetallicRemapMax`.

From 10.x, a new pass ScenePickingPass is added to all the shader and master nodes to allow the editor to correctly handle picking tesselated objects and backfaced objects.

## Raytracing

From Unity 2020.2, the Raytracing Node in Shader Graph now applies the raytraced path (previously low path) to all raytraced effects except path tracing.

## Custom pass API

The signature of the Execute function now only takes a CustomPassContext as its input:
`void Execute(CustomPassContext ctx)`

The CustomPassContext contains all the parameters of the old `Execute` function, but also all the available Render Textures and a MaterialPropertyBlock unique to the custom pass instance.

This context allows you to use the new [CustomPassUtils]( ../api/UnityEngine.Rendering.HighDefinition.CustomPassUtils.html) class which contains functions to speed up the development of your custom passes.

For information on custom pass utilities, see the [custom pass manual](Custom-Pass-API-User-Manual.md) or the [CustomPassUtils API documentation](../api/UnityEngine.Rendering.HighDefinition.CustomPassUtils.html).

To upgrade your custom pass, replace the original execute function prototype with the new one. To do this, replace:

```
protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult) { ... }
```

with:

```
protected override void Execute(CustomPassContext ctx) { ... }
```

## Local Volumetric Fog Mask Texture

Previously, to convert a 2D flipbook texture to the 3D format Density Mask Textures require, you needed to use the __Local Volumetric Fog Texture Tool__ in the __Window > Rendering__ menu.
From Unity 2020.2, you can do this conversion directly through the __Texture Importer__. For information on how to use the importer to convert the flipbook texture, see the [Local Volumetric Fog documentation](Local-Volumetric-Fog.md).

## Diffusion Profiles

The diffusion profile list is now in **Default HDRP Settings** (in the Project Settings window).

This change can affect you if you have multiple HDRP assets set up in **Quality Settings**. In this case, if one or more of your HDRP assets in **Quality Settings** has a different diffusion profile than the one assigned in **Graphics Settings**, then the lists in the HDRP assets are lost. The only list that's now relevant is the one in the **Default HDRP Settings**.

Put all the diffusion profiles used in your project in the HDRP Asset assigned in the **Graphics Settings** before upgrading. This operation will prevent any issue after the upgrade regarding lost diffusion profile in the project.

## Post Processing

Previously, in the Motion Blur volume component the Camera rotation clamp was always active such that by default the part of the motion vector derived from Camera rotation was clamped differently. This can create confusion due to changes in motion vectors that are relative to Camera.

From 2020.2, the Camera rotation clamp option isn't the default, but needs to be selected as an option under the **Camera Clamp Mode** setting. Additional clamping controls for Camera influence on motion blur are available under the same setting.
