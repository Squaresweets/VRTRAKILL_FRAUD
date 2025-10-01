using HarmonyLib;
using Plugin.Systems;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch(typeof(StainVoxelManager))] internal class PatchStains
{
    //THIS IS GONNA BE HARD TO GET WORKING
    //[HarmonyPostfix]
    //[HarmonyPatch(nameof(StainVoxelManager.SetupStainCommandBuffer))]
    //private static void BLOOD(StainVoxelManager __instance, Camera mainCam, RenderTexture mainTex, RenderTexture stainCopy, RenderTexture depth)
    //{
    //    // Create command buffer if needed
    //    if (__instance.cb == null)
    //    {
    //        __instance.cb = new CommandBuffer();
    //        __instance.cb.name = "Gasoline Stain";
    //    }

    //    // Create composite material if needed
    //    if (__instance.gasolineCompositeMaterial == null)
    //    {
    //        __instance.gasolineCompositeMaterial = new Material(__instance.gasolineCompositeShader);
    //    }

    //    // Set up stain material
    //    __instance.gasStainMat.SetTexture("_DepthBuffer", depth);
    //    __instance.gasStainMat.SetBuffer("stainInstances", __instance.stainBuffer);
    //    __instance.gasStainMat.SetBuffer("instanceBuffer", __instance.propBuffer);

    //    __instance.gasStainMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);

    //    // Clear previous commands
    //    __instance.cb.Clear();

    //    // Render stains into temporary texture
    //    __instance.cb.SetRenderTarget(stainCopy.colorBuffer);
    //    __instance.cb.ClearRenderTarget(false, true, Color.white);
    //    __instance.cb.DrawMeshInstancedIndirect(__instance.gasStainMesh, 0, __instance.gasStainMat, 0, __instance.argsBuffer, 0, null);

    //    // Blit directly to camera target (screen / VR eye buffers)
    //    __instance.cb.Blit(stainCopy.colorBuffer, BuiltinRenderTextureType.CameraTarget, __instance.gasolineCompositeMaterial);

    //    // Attach the command buffer to the camera
    //    mainCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, __instance.cb);
    //}
}