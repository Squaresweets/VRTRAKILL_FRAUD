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
        static PortalRenderV2 rightEye; //Left is using the default one

        [HarmonyPostfix] [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnEnable))]
        static void OnEnable(PortalManagerV2 __instance)
        {
            __instance.mainCamera = CameraConverterP.leftEye;
            rightEye = __instance.gameObject.AddComponent<PortalRenderV2>();
        }

        [HarmonyPostfix] [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.LateUpdate))]
        static void LateUpdate(PortalManagerV2 __instance)
        {
            if(__instance.initialized)
                rightEye.Setup(__instance.Scene, CameraConverterP.rightEye, __instance.portalCamera);
        }

        [HarmonyPrefix] [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnPreRenderCallback))]
        static void BeforeOnPreRenderCallback(PortalManagerV2 __instance, Camera cam)
        {
            __instance.portalCamera.stereoTargetEye = StereoTargetEyeMask.Left;
        }
        [HarmonyPostfix] [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnPreRenderCallback))]
        static void OnPreRenderCallback(PortalManagerV2 __instance, Camera cam)
        {
            if (__instance == null || PortalManagerV2.Instance == null) return;

            __instance.portalCamera.stereoTargetEye = StereoTargetEyeMask.Right;
            if (cam == CameraConverterP.rightEye) rightEye.Render(cam);
        }

        [HarmonyPostfix] [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.SetPortalOcclusion))]
        static void SetPortalOcclusion(bool enabled)
        {
            rightEye.SetPortalOcclusion(enabled);
        }
        [HarmonyPostfix] [HarmonyPatch(typeof(PortalRenderV2), nameof(PortalRenderV2.OnOcclusionReadbackCompleted))]
        static void OnOcclusionReadbackCompleted(AsyncGPUReadbackRequest request)
        {
            rightEye.occlusionBitset = request.GetData<ulong>(0)[0];
        }
    }
}
