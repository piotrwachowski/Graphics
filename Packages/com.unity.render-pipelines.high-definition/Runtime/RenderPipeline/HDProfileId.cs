using UnityEngine.Rendering;

namespace UnityEngine.Rendering.HighDefinition
{
    internal enum HDProfileId
    {
        CopyDepthBuffer,
        CopyDepthInTargetTexture,
        BuildCoarseStencilAndResolveIfNeeded,
        AmbientOcclusion,
        HorizonSSAO,
        UpSampleSSAO,
        ScreenSpaceShadows,
        ScreenSpaceShadowsDebug,
        BuildLightList,
        GenerateLightAABBs,
        Distortion,
        AccumulateDistortion,
        ApplyDistortion,
        ForwardDepthPrepass,
        DeferredDepthPrepass,
        TransparentDepthPrepass,
        GBuffer,
        DBufferRender,
        DBufferPrepareDrawData,
        DBufferNormal,
        DisplayDebugDecalsAtlas,
        DisplayDebugViewMaterial,
        DebugViewMaterialGBuffer,
        SubsurfaceScattering,
        SsrTracing,
        SsrReprojection,
        SsrAccumulate,

        // SSGI
        SSGIPass,
        SSGITrace,
        SSGIDenoise,
        SSGIUpscale,
        SSGIConvert,

        ForwardOpaque,
        ForwardOpaqueDebug,
        ForwardTransparent,
        ForwardTransparentDebug,

        ForwardPreRefraction,
        ForwardPreRefractionDebug,
        ForwardTransparentDepthPrepass,
        RenderForwardError,
        TransparentDepthPostpass,
        ObjectsMotionVector,
        CameraMotionVectors,
        ColorPyramid,
        DepthPyramid,
        PostProcessing,
        AfterPostProcessingObjects,
        RenderFullScreenDebug,
        ClearBuffers,
        ClearStencil,
        HDRenderPipelineRenderCamera,
        HDRenderPipelineRenderAOV,
        HDRenderPipelineAllRenderRequest,
        CullResultsCull,
        CustomPassCullResultsCull,
        DisplayCookieAtlas,
        RenderWireFrame,
        ConvolveReflectionProbe,
        ConvertReflectionProbe,
        ConvolvePlanarReflectionProbe,
        UpdateReflectionProbeAtlas,
        BlitTextureToReflectionProbeAtlas,
        DisplayReflectionProbeAtlas,
        PreIntegradeWardCookTorrance,
        FilterCubemapCharlie,
        FilterCubemapGGX,
        AreaLightCookieConvolution,
        DisplayLocalVolumetricFogAtlas,

        UpdateSkyEnvironmentConvolution,
        RenderSkyToCubemap,
        UpdateSkyAmbientProbe,
        PreRenderSky,
        RenderSky,
        RenderClouds,
        OpaqueAtmosphericScattering,
        InScatteredRadiancePrecomputation,

        VolumeVoxelization,
        VolumetricLighting,
        VolumetricLightingFiltering,
        PrepareVisibleLocalVolumetricFogList,
        UpdateLocalVolumetricFogAtlas,

        // Volumetric clouds
        VolumetricClouds,
        VolumetricCloudsPrepare,
        VolumetricCloudsTrace,
        VolumetricCloudsReproject,
        VolumetricCloudsPreUpscale,
        VolumetricCloudsUpscaleAndCombine,
        VolumetricCloudsShadow,
        VolumetricCloudMapGeneration,
        VolumetricCloudsAmbientProbe,

        // Water surface
        WaterSurfaceSimulation,
        WaterSurfaceRenderingGBuffer,
        WaterSurfaceRenderingSSR,
        WaterSurfaceRenderingDeferred,
        WaterSurfaceRenderingUnderWater,

        // RT Cluster
        RaytracingBuildCluster,
        RaytracingCullLights,
        RaytracingDebugCluster,
        // RT acceleration structure setup
        RaytracingBuildAccelerationStructure,
        RaytracingBuildAccelerationStructureDebug,
        // RTR
        RaytracingReflectionDirectionGeneration,
        RaytracingReflectionEvaluation,
        RaytracingReflectionAdjustWeight,
        RaytracingReflectionFilter,
        RaytracingReflectionUpscale,
        // RTAO
        RaytracingAmbientOcclusion,
        RaytracingFilterAmbientOcclusion,
        RaytracingComposeAmbientOcclusion,
        RaytracingClearHistoryAmbientOcclusion,
        // RT Shadows
        RaytracingDirectionalLightShadow,
        RaytracingLightShadow,
        RaytracingAreaLightShadow,
        // RTGI
        RaytracingIndirectDiffuseDirectionGeneration,
        RaytracingIndirectDiffuseEvaluation,
        RaytracingIndirectDiffuseUpscale,
        RaytracingFilterIndirectDiffuse,
        RaytracingIndirectDiffuseAdjustWeight,

        // RTSSS
        RaytracingSSS,
        RaytracingSSSTrace,
        RaytracingSSSCompose,
        // RTShadow
        RaytracingWriteShadow,
        // Other ray tracing
        RaytracingDebugOverlay,
        RayTracingRecursiveRendering,
        RayTracingDepthPrepass,
        RayTracingFlagMask,
        // RT Deferred Lighting
        RaytracingDeferredLighting,
        // Denoisers
        HistoryValidity,
        TemporalFilter,
        DiffuseFilter,

        UpdateGlobalConstantBuffers,
        UpdateEnvironment,
        ConfigureKeywords,
        RecordRenderGraph,

        PrepareLightsForGPU,
        PrepareGPULightdata,
        PrepareGPUProbeData,
        ConvertLightsGpuFormat,
        ProcessVisibleLights,
        ProcessDirectionalAndCookies,
        SortVisibleLights,
        BuildVisibleLightEntities,
        ProcessShadows,

        // Profile sampler for shadow
        RenderShadowMaps,
        RenderMomentShadowMaps,
        RenderEVSMShadowMaps,
        RenderEVSMShadowMapsBlur,
        RenderEVSMShadowMapsCopyToAtlas,
        BlitDirectionalMixedCachedShadowMaps,
        BlitPunctualMixedCachedShadowMaps,
        BlitAreaMixedCachedShadowMaps,

        // Profile sampler for tile pass
        TileClusterLightingDebug,
        DisplayShadows,

        RenderDeferredLightingCompute,
        RenderDeferredLightingComputeAsPixel,
        RenderDeferredLightingSinglePass,
        RenderDeferredLightingSinglePassMRT,

        // Misc
        VolumeUpdate,
        CustomPassVolumeUpdate,
        OffscreenUIRendering,

        // XR
        XRMirrorView,
        XRCustomMirrorView,
        XRDepthCopy,

        // Low res transparency
        DownsampleDepth,
        LowResTransparent,
        UpsampleLowResTransparent,

        // Post-processing
        AlphaCopy,
        StopNaNs,
        FixedExposure,
        DynamicExposure,
        ApplyExposure,
        TemporalAntialiasing,
        DeepLearningSuperSamplingColorMask,
        DeepLearningSuperSampling,
        DepthOfField,
        DepthOfFieldKernel,
        DepthOfFieldCoC,
        DepthOfFieldPrefilter,
        DepthOfFieldPyramid,
        DepthOfFieldDilate,
        DepthOfFieldTileMax,
        DepthOfFieldGatherFar,
        DepthOfFieldGatherNear,
        DepthOfFieldPreCombine,
        DepthOfFieldCombine,
        LensFlareDataDriven,
        LensFlareComputeOcclusionDataDriven,
        LensFlareMergeOcclusionDataDriven,
        MotionBlur,
        MotionBlurMotionVecPrep,
        MotionBlurTileMinMax,
        MotionBlurTileNeighbourhood,
        MotionBlurTileScattering,
        MotionBlurKernel,
        PaniniProjection,
        Bloom,
        ColorGradingLUTBuilder,
        UberPost,
        FXAA,
        SMAA,
        SceneUpsampling,
        SetResolutionGroup,
        FinalPost,
        FinalImageHistogram,
        HDRDebugData,
        CustomPostProcessBeforeTAA,
        CustomPostProcessBeforePP,
        CustomPostProcessAfterPPBlurs,
        CustomPostProcessAfterPP,
        CustomPostProcessAfterOpaqueAndSky,
        ContrastAdaptiveSharpen,
        EdgeAdaptiveSpatialUpsampling,
        PrepareProbeVolumeList,
        ProbeVolumeDebug,

        AOVExecute,
        AOVOutput,
#if ENABLE_VIRTUALTEXTURES
        VTFeedbackDownsample,
#endif
    }
}
