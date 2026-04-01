using HarmonyLib;
using Plugin.Systems.VRCamera.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.Rendering;

namespace Plugin.Systems.VRCamera.Patches
{
    [HarmonyPatch] public class PortalPatch
    {
        //static PortalRenderV2 rightEye; //Left is using the default one

        [HarmonyPrefix] [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnEnable))]
        static void OnEnableThing(PortalManagerV2 __instance)
        {
            CameraConverterP.SetupEyeCameras();
            __instance.mainCamera = CameraConverterP.leftEye;
            //rightEye = __instance.gameObject.AddComponent<PortalRenderV2>();
            //Need to set it all up
        }

        [HarmonyPrefix] [HarmonyPatch(typeof(PostProcessV2_Handler), nameof(PostProcessV2_Handler.LateUpdate))]
        static void LateUpdatePP(PostProcessV2_Handler __instance)
        {
            __instance.mainCam = CameraConverterP.leftEye;
        }
        [HarmonyPrefix] [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.LateUpdate))]
        static void LateUpdateTest(PortalManagerV2 __instance)
        {
            if (__instance.mainCamera)
                __instance.mainCamera.projectionMatrix = __instance.mainCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            __instance.mainCamera = CameraConverterP.leftEye;
        }
        [HarmonyPostfix] [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.LateUpdate))]
        static void LateUpdateThing(PortalManagerV2 __instance)
        {
            //if(__instance.initialized)
            //    rightEye.Setup(__instance.Scene, CameraConverterP.rightEye, __instance.portalCamera);
        }

        [HarmonyPrefix] [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnPreRenderCallback))]
        static void BeforeOnPreRenderCallback(PortalManagerV2 __instance, Camera cam)
        {
            //Debug.LogError($"{cam.name}");
            //if (cam.GetCommandBuffers(CameraEvent.BeforeForwardOpaque).Length > 0)
            //    Debug.LogError($"{cam.name} {cam.GetCommandBuffers(CameraEvent.BeforeForwardOpaque)[0].name}");

            //__instance.portalCamera.stereoTargetEye = StereoTargetEyeMask.Both;
        }
        [HarmonyPostfix] [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnPreRenderCallback))]
        static void OnPreRenderCallback(PortalManagerV2 __instance, Camera cam)
        {
            if (__instance == null || PortalManagerV2.Instance == null) return;

            //__instance.portalCamera.stereoTargetEye = StereoTargetEyeMask.Right;
            //if (cam == CameraConverterP.rightEye) rightEye.Render(cam);
        }

        [HarmonyPostfix] [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.SetPortalOcclusion))]
        static void SetPortalOcclusion(bool enabled)
        {
            //rightEye.SetPortalOcclusion(enabled);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalRenderV2), nameof(PortalRenderV2.SetupRenderData))]
        static void PortalRenderFix(PortalRenderV2 __instance)
        {
            if(__instance.portalCam)
                __instance.portalCam.stereoTargetEye = StereoTargetEyeMask.None;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalRenderV2), nameof(PortalRenderV2.Render))]
        static void PortalRenderFix2(PortalRenderV2 __instance)
        {
            if(__instance.portalCam)
                __instance.portalCam.stereoTargetEye = StereoTargetEyeMask.None;
        }
    }
}
