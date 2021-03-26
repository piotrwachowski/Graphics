using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.Universal
{
    public static class NativeRenderPass
    {
        internal const int kRenderPassMapSize = 10;
        internal const int kRenderPassMaxCount = 20;
        internal static Dictionary<Hash128, int[]> m_MergeableRenderPassesMap = new Dictionary<Hash128, int[]>(kRenderPassMapSize);
        internal static int[][] m_MergeableRenderPassesMapArrays;
        internal static Hash128[] m_SceneIndexToPassHash = new Hash128[kRenderPassMaxCount];
        internal static Dictionary<Hash128, int> m_RenderPassesAttachmentCount = new Dictionary<Hash128, int>(kRenderPassMapSize);

        internal static void SetupFrameData(CameraData cameraData, List<ScriptableRenderPass> activeRenderPassQueue)
        {
            //TODO: edge cases to detect that should affect possible passes to merge
            // - total number of color attachment > 8

            // Scene and RenderPasses setup:
            // - Go through all the passes and mark the final one as last pass (Make sure the list is already sorted!)
            // - Setup the RenderPass merging lists.

            m_MergeableRenderPassesMap.Clear();
            m_RenderPassesAttachmentCount.Clear();

            uint currentHashIndex = 0;
            // reset all the passes last pass flag
            for (int i = 0; i < activeRenderPassQueue.Count; ++i)
            {
                var renderPass = activeRenderPassQueue[i];

                // Empty configure to setup dimensions/targets and whatever data is needed for merging
                // We do not execute this at this time, so render targets are still invalid

                var width = renderPass.renderTargetWidth != -1 ? renderPass.renderTargetWidth : cameraData.cameraTargetDescriptor.width;
                var height = renderPass.renderTargetHeight != -1 ? renderPass.renderTargetHeight : cameraData.cameraTargetDescriptor.height;
                var sampleCount = renderPass.renderTargetSampleCount != -1 ? renderPass.renderTargetSampleCount : cameraData.cameraTargetDescriptor.msaaSamples;
                var rtID = renderPass.depthOnly ? renderPass.colorAttachment.GetHashCode() : renderPass.depthAttachment.GetHashCode();

                Hash128 hash = CreateRenderPassHash(width, height, rtID, sampleCount, currentHashIndex);

                m_SceneIndexToPassHash[i] = hash;

                if (!renderPass.useNativeRenderPass)
                    continue;

                if (!m_MergeableRenderPassesMap.ContainsKey(hash))
                {
                    m_MergeableRenderPassesMap.Add(hash, m_MergeableRenderPassesMapArrays[m_MergeableRenderPassesMap.Count]);
                    m_RenderPassesAttachmentCount.Add(hash, 0);
                }
                else if (m_MergeableRenderPassesMap[hash][GetValidPassIndexCount(m_MergeableRenderPassesMap[hash]) - 1] != (i - 1))
                {
                    // if the passes are not sequential we want to split the current mergeable passes list. So we increment the hashIndex and update the hash

                    currentHashIndex++;
                    hash = CreateRenderPassHash(width, height, rtID, sampleCount, currentHashIndex);

                    m_SceneIndexToPassHash[i] = hash;

                    m_MergeableRenderPassesMap.Add(hash, m_MergeableRenderPassesMapArrays[m_MergeableRenderPassesMap.Count]);
                    m_RenderPassesAttachmentCount.Add(hash, 0);
                }

                m_MergeableRenderPassesMap[hash][GetValidPassIndexCount(m_MergeableRenderPassesMap[hash])] = i;
            }

            for (int i = 0; i < activeRenderPassQueue.Count; ++i)
                activeRenderPassQueue[i].attachmentIndices = new NativeArray<int>(8, Allocator.Temp);
        }

        internal static void SetMRTAttachmentsList(ScriptableRenderPass renderPass, ref CameraData cameraData, uint validColorBuffersCount, bool needCustomCameraColorClear,
            bool needCustomCameraDepthClear, List<ScriptableRenderPass> activeRenderPassQueue,
            ref AttachmentDescriptor[] activeColorAttachmentDescriptors, ref AttachmentDescriptor activeDepthAttachmentDescriptor)
        {
            int currentSceneIndex = renderPass.sceneIndex;
            Hash128 currentPassHash = m_SceneIndexToPassHash[currentSceneIndex];
            int[] currentMergeablePasses = m_MergeableRenderPassesMap[currentPassHash];
            bool isFirstMergeablePass = currentMergeablePasses.First() == currentSceneIndex;

            if (!isFirstMergeablePass)
                return;

            m_RenderPassesAttachmentCount[currentPassHash] = 0;

            int currentAttachmentIdx = 0;
            foreach (var passIdx in currentMergeablePasses)
            {
                if (passIdx == -1)
                    break;
                ScriptableRenderPass pass = activeRenderPassQueue[passIdx];

                for (int i = 0; i < pass.attachmentIndices.Length; ++i)
                    pass.attachmentIndices[i] = -1;

                // TODO: review the lastPassToBB logic to mak it work with merged passes
                bool isLastPass = pass.isLastPass;
                bool isLastPassToBB = false;

                for (int i = 0; i < validColorBuffersCount; ++i)
                {
                    AttachmentDescriptor currentAttachmentDescriptor = new AttachmentDescriptor(pass.renderTargetFormat[i] != GraphicsFormat.None ? pass.renderTargetFormat[i] : SystemInfo.GetGraphicsFormat(DefaultFormat.LDR));

                    // if this is the current camera's last pass, also check if one of the RTs is the backbuffer (BuiltinRenderTextureType.CameraTarget)
                    isLastPassToBB |= isLastPass && (pass.colorAttachments[i] == BuiltinRenderTextureType.CameraTarget);

                    int existingAttachmentIndex = FindAttachmentDescriptorIndexInList(currentAttachmentIdx, currentAttachmentDescriptor, activeColorAttachmentDescriptors);

                    if (existingAttachmentIndex == -1)
                    {
                        // add a new attachment

                        activeColorAttachmentDescriptors[currentAttachmentIdx] = currentAttachmentDescriptor;

                        activeColorAttachmentDescriptors[currentAttachmentIdx].ConfigureTarget(pass.colorAttachments[i], false, true);
                        if (needCustomCameraColorClear)
                            activeColorAttachmentDescriptors[currentAttachmentIdx].ConfigureClear(Color.black, 1.0f, 0);

                        pass.attachmentIndices[i] = currentAttachmentIdx;

                        currentAttachmentIdx++;
                        m_RenderPassesAttachmentCount[currentPassHash]++;
                    }
                    else
                    {
                        // attachment was already present
                        pass.attachmentIndices[i] = existingAttachmentIndex;
                    }
                }

                // TODO: this is redundant and is being setup for each attachment. Needs to be done only once per mergeable pass list (we need to make sure mergeable passes use the same depth!)
                activeDepthAttachmentDescriptor = new AttachmentDescriptor(GraphicsFormat.DepthAuto);
                activeDepthAttachmentDescriptor.ConfigureTarget(pass.depthAttachment, !needCustomCameraDepthClear, !isLastPassToBB);
                if (needCustomCameraDepthClear)
                    activeDepthAttachmentDescriptor.ConfigureClear(Color.black, 1.0f, 0);
            }
        }

        internal static void SetAttachmentList(ScriptableRenderPass renderPass, ref CameraData cameraData, RenderTargetIdentifier passColorAttachment, RenderTargetIdentifier passDepthAttachment, ClearFlag finalClearFlag, Color finalClearColor,
            List<ScriptableRenderPass> activeRenderPassQueue,
            ref AttachmentDescriptor[] activeColorAttachmentDescriptors, ref AttachmentDescriptor activeDepthAttachmentDescriptor)
        {
            int currentSceneIndex = renderPass.sceneIndex;
            Hash128 currentPassHash = m_SceneIndexToPassHash[currentSceneIndex];
            int[] currentMergeablePasses = m_MergeableRenderPassesMap[currentPassHash];
            bool isFirstMergeablePass = currentMergeablePasses.First() == currentSceneIndex;

            if (!isFirstMergeablePass)
                return;

            m_RenderPassesAttachmentCount[currentPassHash] = 0;

            int currentAttachmentIdx = 0;
            foreach (var passIdx in currentMergeablePasses)
            {
                if (passIdx == -1)
                    break;
                ScriptableRenderPass pass = activeRenderPassQueue[passIdx];

                for (int i = 0; i < pass.attachmentIndices.Length; ++i)
                    pass.attachmentIndices[i] = -1;

                AttachmentDescriptor currentAttachmentDescriptor;
                var usesTargetTexture = cameraData.targetTexture != null;
                var depthOnly = renderPass.depthOnly || (usesTargetTexture &&
                    cameraData.targetTexture.graphicsFormat == GraphicsFormat.DepthAuto);
                // Offscreen depth-only cameras need this set explicitly
                if (depthOnly && usesTargetTexture)
                {
                    passColorAttachment = new RenderTargetIdentifier(cameraData.targetTexture);
                    currentAttachmentDescriptor = new AttachmentDescriptor(GraphicsFormat.DepthAuto);
                }
                else
                    currentAttachmentDescriptor = new AttachmentDescriptor(cameraData.cameraTargetDescriptor.graphicsFormat);

                if (pass.overrideCameraTarget)
                {
                    GraphicsFormat hdrFormat = GraphicsFormat.None;
                    if (cameraData.isHdrEnabled)
                    {
                        if (!Graphics.preserveFramebufferAlpha && RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Linear | FormatUsage.Render))
                            hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
                        else if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Linear | FormatUsage.Render))
                            hdrFormat = GraphicsFormat.R16G16B16A16_SFloat;
                        else
                            hdrFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);
                    }

                    var defaultFormat = cameraData.isHdrEnabled ? hdrFormat : SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
                    currentAttachmentDescriptor = new AttachmentDescriptor(pass.renderTargetFormat[0] != GraphicsFormat.None ? pass.renderTargetFormat[0] : defaultFormat);
                }

                bool isLastPass = pass.isLastPass;
                var samples = pass.renderTargetSampleCount != -1 ? pass.renderTargetSampleCount : cameraData.cameraTargetDescriptor.msaaSamples;

                var colorAttachmentTarget = (depthOnly || passColorAttachment != BuiltinRenderTextureType.CameraTarget)
                    ? passColorAttachment
                    : (usesTargetTexture ? new RenderTargetIdentifier(cameraData.targetTexture.colorBuffer)
                        : BuiltinRenderTextureType.CameraTarget);

                var depthAttachmentTarget = (passDepthAttachment != BuiltinRenderTextureType.CameraTarget)
                    ? passDepthAttachment
                    : (usesTargetTexture ? new RenderTargetIdentifier(cameraData.targetTexture.depthBuffer)
                        : BuiltinRenderTextureType.Depth);

                // TODO: review the lastPassToBB logic to mak it work with merged passes
                // keep track if this is the current camera's last pass and the RT is the backbuffer (BuiltinRenderTextureType.CameraTarget)
                // knowing isLastPassToBB can help decide the optimal store action as it gives us additional information about the current frame
                bool isLastPassToBB = isLastPass && (colorAttachmentTarget == BuiltinRenderTextureType.CameraTarget);
                currentAttachmentDescriptor.ConfigureTarget(colorAttachmentTarget, ((uint)finalClearFlag & (uint)ClearFlag.Color) == 0, !(samples > 1 && isLastPassToBB));

                // TODO: this is redundant and is being setup for each attachment. Needs to be done only once per mergeable pass list (we need to make sure mergeable passes use the same depth!)
                activeDepthAttachmentDescriptor = new AttachmentDescriptor(GraphicsFormat.DepthAuto);
                activeDepthAttachmentDescriptor.ConfigureTarget(depthAttachmentTarget, ((uint)finalClearFlag & (uint)ClearFlag.Depth) == 0 , !isLastPassToBB);

                if (finalClearFlag != ClearFlag.None)
                {
                    // We don't clear color for Overlay render targets, however pipeline set's up depth only render passes as color attachments which we do need to clear
                    if ((cameraData.renderType != CameraRenderType.Overlay || depthOnly && ((uint)finalClearFlag & (uint)ClearFlag.Color) != 0))
                        currentAttachmentDescriptor.ConfigureClear(finalClearColor, 1.0f, 0);
                    if (((uint)finalClearFlag & (uint)ClearFlag.Depth) != 0)
                        activeDepthAttachmentDescriptor.ConfigureClear(Color.black, 1.0f, 0);
                }

                if (samples > 1)
                    currentAttachmentDescriptor.ConfigureResolveTarget(colorAttachmentTarget); // resolving to the implicit color target's resolve surface TODO: handle m_CameraResolveTarget if present?

                int existingAttachmentIndex = FindAttachmentDescriptorIndexInList(currentAttachmentIdx, currentAttachmentDescriptor, activeColorAttachmentDescriptors);

                if (existingAttachmentIndex == -1)
                {
                    // add a new attachment
                    pass.attachmentIndices[0] = currentAttachmentIdx;
                    activeColorAttachmentDescriptors[currentAttachmentIdx] = currentAttachmentDescriptor;
                    currentAttachmentIdx++;
                    m_RenderPassesAttachmentCount[currentPassHash]++;
                }
                else
                {
                    // attachment was already present
                    pass.attachmentIndices[0] = existingAttachmentIndex;
                }
            }
        }

        internal static void Configure(CommandBuffer cmd, ScriptableRenderPass renderPass, CameraData cameraData, List<ScriptableRenderPass> activeRenderPassQueue)
        {
            int currentSceneIndex = renderPass.sceneIndex;
            Hash128 currentPassHash = m_SceneIndexToPassHash[currentSceneIndex];
            int[] currentMergeablePasses = m_MergeableRenderPassesMap[currentPassHash];
            bool isFirstMergeablePass = currentMergeablePasses.First() == currentSceneIndex;

            if (isFirstMergeablePass)
            {
                foreach (var passIdx in currentMergeablePasses)
                {
                    if (passIdx == -1)
                        break;
                    ScriptableRenderPass pass = activeRenderPassQueue[passIdx];

                    pass.Configure(cmd, cameraData.cameraTargetDescriptor);
                }
            }
        }

        internal static void Execute(ScriptableRenderContext context, ScriptableRenderPass renderPass, CameraData cameraData, ref RenderingData renderingData,  List<ScriptableRenderPass> activeRenderPassQueue,
            ref AttachmentDescriptor[] activeColorAttachmentDescriptors, ref AttachmentDescriptor activeDepthAttachmentDescriptor, RenderTargetIdentifier activeDepthAttachment)
        {
            int currentSceneIndex = renderPass.sceneIndex;
            Hash128 currentPassHash = m_SceneIndexToPassHash[currentSceneIndex];
            int[] currentMergeablePasses = m_MergeableRenderPassesMap[currentPassHash];

            int validColorBuffersCount = m_RenderPassesAttachmentCount[currentPassHash];

            bool isLastPass = renderPass.isLastPass;
            // TODO: review the lastPassToBB logic to mak it work with merged passes
            // keep track if this is the current camera's last pass and the RT is the backbuffer (BuiltinRenderTextureType.CameraTarget)
            bool isLastPassToBB = isLastPass && (activeColorAttachmentDescriptors[0].loadStoreTarget == BuiltinRenderTextureType.CameraTarget);
            bool useDepth = activeDepthAttachment == RenderTargetHandle.CameraTarget.Identifier() && (!(isLastPassToBB || (isLastPass && cameraData.camera.targetTexture != null)));
            var depthOnly = renderPass.depthOnly || (cameraData.targetTexture != null && cameraData.targetTexture.graphicsFormat == GraphicsFormat.DepthAuto);

            var attachments =
                new NativeArray<AttachmentDescriptor>(useDepth && !depthOnly ? validColorBuffersCount + 1 : 1, Allocator.Temp);

            for (int i = 0; i < validColorBuffersCount; ++i)
                attachments[i] = activeColorAttachmentDescriptors[i];

            if (useDepth && !depthOnly)
                attachments[validColorBuffersCount] = activeDepthAttachmentDescriptor;

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            var sampleCount = desc.msaaSamples;
            int width = renderPass.renderTargetWidth != -1 ? renderPass.renderTargetWidth : desc.width;
            int height = renderPass.renderTargetHeight != -1 ? renderPass.renderTargetHeight : desc.height;
            sampleCount = renderPass.renderTargetSampleCount != -1
                ? renderPass.renderTargetSampleCount
                : sampleCount;

            bool isFirstMergeablePass = currentMergeablePasses[0] == currentSceneIndex;
            int validPassCount = GetValidPassIndexCount(currentMergeablePasses);
            bool isLastMergeablePass = currentMergeablePasses[validPassCount - 1] == currentSceneIndex;

            var attachmentIndicesCount = GetSubPassAttachmentIndicesCount(renderPass);

            var attachmentIndices = new NativeArray<int>(!depthOnly ? (int)attachmentIndicesCount : 0, Allocator.Temp);
            if (!depthOnly)
            {
                for (int i = 0; i < attachmentIndicesCount; ++i)
                {
                    attachmentIndices[i] = renderPass.attachmentIndices[i];
                }
            }

            if (validPassCount == 1 || isFirstMergeablePass)
            {
                context.BeginRenderPass(width, height, Math.Max(sampleCount, 1), attachments,
                    useDepth ? (!depthOnly ? validColorBuffersCount : 0) : -1);
                attachments.Dispose();

                context.BeginSubPass(attachmentIndices);
            }
            else
            {
                if (!AreAttachmentIndicesCompatible(activeRenderPassQueue[currentSceneIndex - 1], activeRenderPassQueue[currentSceneIndex]))
                {
                    context.EndSubPass();
                    context.BeginSubPass(attachmentIndices);
                }
            }

            attachmentIndices.Dispose();

            renderPass.Execute(context, ref renderingData);

            if (validPassCount == 1 || isLastMergeablePass)
            {
                context.EndSubPass();
                context.EndRenderPass();
            }

            for (int i = 0; i < activeColorAttachmentDescriptors.Length; ++i)
            {
                activeColorAttachmentDescriptors[i] = RenderingUtils.emptyAttachment;
            }
            activeDepthAttachmentDescriptor = RenderingUtils.emptyAttachment;
        }

        internal static uint GetSubPassAttachmentIndicesCount(ScriptableRenderPass pass)
        {
            uint numValidAttachments = 0;

            foreach (var attIdx in pass.attachmentIndices)
            {
                if (attIdx >= 0)
                    ++numValidAttachments;
            }

            return numValidAttachments;
        }

        internal static bool AreAttachmentIndicesCompatible(ScriptableRenderPass lastSubPass, ScriptableRenderPass currentSubPass)
        {
            uint lastSubPassAttCount = GetSubPassAttachmentIndicesCount(lastSubPass);
            uint currentSubPassAttCount = GetSubPassAttachmentIndicesCount(currentSubPass);

            if (currentSubPassAttCount > lastSubPassAttCount)
                return false;

            uint numEqualAttachments = 0;
            for (int currPassIdx = 0; currPassIdx < currentSubPassAttCount; ++currPassIdx)
            {
                for (int lastPassIdx = 0; lastPassIdx < lastSubPassAttCount; ++lastPassIdx)
                {
                    if (currentSubPass.attachmentIndices[currPassIdx] == lastSubPass.attachmentIndices[lastPassIdx])
                        numEqualAttachments++;
                }
            }

            return (numEqualAttachments == currentSubPassAttCount);
        }

        internal static uint GetValidColorAttachmentCount(AttachmentDescriptor[] colorAttachments)
        {
            uint nonNullColorBuffers = 0;
            if (colorAttachments != null)
            {
                foreach (var attachment in colorAttachments)
                {
                    if (attachment != RenderingUtils.emptyAttachment)
                        ++nonNullColorBuffers;
                }
            }
            return nonNullColorBuffers;
        }

        internal static int FindAttachmentDescriptorIndexInList(int attachmentIdx, AttachmentDescriptor attachmentDescriptor, AttachmentDescriptor[] attachmentDescriptors)
        {
            int existingAttachmentIndex = -1;
            for (int i = 0; i < attachmentIdx; ++i)
            {
                AttachmentDescriptor att = attachmentDescriptors[i];

                if (att.loadStoreTarget == attachmentDescriptor.loadStoreTarget)
                {
                    existingAttachmentIndex = i;
                    break;
                }
            }

            return existingAttachmentIndex;
        }

        internal static int GetValidPassIndexCount(int[] array)
        {
            for (int i = 0; i < array.Length; ++i)
                if (array[i] == -1)
                    return i;
            return array.Length - 1;
        }

        internal static Hash128 CreateRenderPassHash(int width, int height, int depthID, int sample, uint hashIndex)
        {
            return new Hash128((uint)width * 10000 + (uint)height, (uint)depthID, (uint)sample, hashIndex);
        }
    }
}
