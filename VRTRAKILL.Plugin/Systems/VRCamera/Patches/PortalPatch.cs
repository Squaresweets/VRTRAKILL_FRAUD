using HarmonyLib;
using Plugin.Systems.VRCamera.Patches;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.XR;
using Valve.VR.InteractionSystem;

namespace Plugin.Systems.VRCamera.Patches
{
    [HarmonyPatch]
    public class PortalPatch
    {
        static PortalRenderV2 leftEyeRender;
        static PostProcessV2_Handler leftEyePP;

        static PortalRenderV2 rightEyeRender; //Left is using the default one
        static PostProcessV2_Handler rightEyePP;

        static Camera rightEyePortalCam;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.InitCam))]
        static bool InitStopDuplicate(PortalManagerV2 __instance)
        {
            Camera.onPreRender -= __instance.OnPreRenderCallback; //Only add it once
            Camera.onPreRender += __instance.OnPreRenderCallback;
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnEnable))]
        static void SetCamToLeft(PortalManagerV2 __instance)
        {
            CameraConverterP.SetupEyeCameras();
            __instance.mainCamera = CameraConverterP.leftEye;
            //Need to set it all up
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnEnable))]
        static void SetupDuplicates(PortalManagerV2 __instance)
        {
            leftEyeRender = __instance.render;

            if(rightEyeRender != null) GameObject.Destroy(rightEyeRender);
            rightEyeRender = __instance.gameObject.AddComponent<PortalRenderV2>();
            rightEyeRender.portalCompositeMaterial = new Material(leftEyeRender.portalCompositeMaterial);
            rightEyeRender.portalMaterial = new Material(leftEyeRender.portalMaterial);
            rightEyeRender.portalBitset64DownsampleMat = new Material(leftEyeRender.portalBitset64DownsampleMat);
            rightEyeRender.fakeRecursionCopy = new Material(leftEyeRender.fakeRecursionCopy);
            rightEyeRender.obliqueCutoff = 0.2f;
            rightEyeRender.mainCam = CameraConverterP.rightEye;

            //Copy it all over to a new one lmao
            leftEyePP = PostProcessV2_Handler.Instance;

            if(rightEyePP != null) GameObject.Destroy(rightEyePP);
            rightEyePP = __instance.gameObject.AddComponent<PostProcessV2_Handler>();
            rightEyePP.postProcessV2_VSRM = new Material(leftEyePP.postProcessV2_VSRM);
            rightEyePP.screenNormal = new Material(leftEyePP.screenNormal);
            rightEyePP.heatWaveMat = new Material(leftEyePP.heatWaveMat);
            rightEyePP.outlinePx = leftEyePP.outlinePx;
            rightEyePP.oilTex = leftEyePP.oilTex;
            rightEyePP.sandTex = leftEyePP.sandTex;
            rightEyePP.buffTex = leftEyePP.buffTex;
            rightEyePP.ditherTexture = leftEyePP.ditherTexture;
            rightEyePP.vignetteTexture = leftEyePP.vignetteTexture;
            rightEyePP.distance = leftEyePP.distance;
            rightEyePP.radiantBuff = new Material(leftEyePP.radiantBuff);
            rightEyePP.paletteCompute = leftEyePP.paletteCompute;
            rightEyePP.paletteCalc = leftEyePP.paletteCalc;
            //Eye set after start

            if(rightEyePortalCam != null) GameObject.Destroy(rightEyePortalCam);
            rightEyePortalCam = new GameObject("Right portal cam", typeof(Camera)).GetComponent<Camera>();
            rightEyePortalCam.transform.parent = __instance.transform;
            rightEyePortalCam.CopyFrom(__instance.portalCamera);
            rightEyePortalCam.enabled = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PostProcessV2_Handler), nameof(PostProcessV2_Handler.Start))]
        [HarmonyPatch(typeof(PostProcessV2_Handler), nameof(PostProcessV2_Handler.ChangeCamera))]
        static void PPSetCamera(PostProcessV2_Handler __instance)
        {
            if (__instance == leftEyePP) __instance.mainCam = CameraConverterP.leftEye;
            else if (__instance == rightEyePP) __instance.mainCam = CameraConverterP.rightEye;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.LateUpdate))]
        static void LateUpdateLeft(PortalManagerV2 __instance)
        {
            PostProcessV2_Handler.Instance = leftEyePP;
            leftEyeRender.pph = leftEyePP;
            leftEyePP.mainCam = CameraConverterP.leftEye;

            __instance.mainCamera = CameraConverterP.leftEye;
            if (__instance.mainCamera)
            {
                //Two very important lines!!!
                __instance.mainCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Left, __instance.mainCamera.worldToCameraMatrix);
                __instance.mainCamera.projectionMatrix = __instance.mainCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.LateUpdate))]
        static void LateUpdateRight(PortalManagerV2 __instance)
        {
            PostProcessV2_Handler.Instance = rightEyePP;
            rightEyeRender.pph = rightEyePP;
            rightEyePP.mainCam = CameraConverterP.rightEye;

            if (rightEyeRender.mainCam)
            {
                rightEyeRender.mainCam.SetStereoViewMatrix(Camera.StereoscopicEye.Right, rightEyeRender.mainCam.worldToCameraMatrix);
                rightEyeRender.mainCam.projectionMatrix = rightEyeRender.mainCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            }
            if (__instance.initialized)
                rightEyeRender.Setup(__instance.Scene, CameraConverterP.rightEye, rightEyePortalCam);
            PostProcessV2_Handler.Instance = leftEyePP;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnPreRenderCallback))]
        static void OnPreRenderCalbackLeft(PortalManagerV2 __instance, Camera cam)
        {
            PostProcessV2_Handler.Instance = leftEyePP;
            leftEyeRender.pph = leftEyePP;
            leftEyePP.mainCam = CameraConverterP.leftEye;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnPreRenderCallback))]
        static void OnPreRenderCallbackRight(PortalManagerV2 __instance, Camera cam)
        {
            if (__instance == null || PortalManagerV2.Instance == null) return;

            PostProcessV2_Handler.Instance = rightEyePP;
            rightEyeRender.pph = rightEyePP;
            rightEyePP.mainCam = CameraConverterP.rightEye;
            if (cam == CameraConverterP.rightEye) rightEyeRender.Render(cam);
            PostProcessV2_Handler.Instance = leftEyePP;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.SetPortalOcclusion))]
        static void SetPortalOcclusion(bool enabled)
        {
            rightEyeRender.SetPortalOcclusion(enabled);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalRenderV2), nameof(PortalRenderV2.SetupRenderData))]
        static void PortalRenderFix(PortalRenderV2 __instance)
        {
            if (__instance.portalCam) __instance.portalCam.stereoTargetEye = StereoTargetEyeMask.None;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalRenderV2), nameof(PortalRenderV2.Render))]
        static void PortalRenderFix2(PortalRenderV2 __instance)
        {
            if (__instance.portalCam) //Essentially just checking if we are configured
            {
                //All the globals that get overwritten
                Shader.SetGlobalTexture(__instance.portalDepthID, __instance.pph.depthBuffer);
                Shader.SetGlobalTexture(__instance.portalOcclusionDataID, __instance.portalOcclusionData);
                Shader.SetGlobalTexture(__instance.portalCompositeColorID, __instance.portalCompositeColor);
                Shader.SetGlobalTexture(__instance.portalCompositeOutlineDatahID, __instance.portalCompositeOutlineData);
                Shader.SetGlobalTexture(__instance.portalCompositeOcclusionDataID, __instance.portalCompositeOcclusionData[0]);
            }

            if (__instance.portalCam)
                __instance.portalCam.stereoTargetEye = StereoTargetEyeMask.None;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PostProcessV2_Handler), nameof(PostProcessV2_Handler.OnPreRenderCallback))]
        static void FixRed(PostProcessV2_Handler __instance, Camera cam)
        {
            __instance.usedComputeShadersAtStart = false;
        }
        //Other hand done in VRArmTransformer (bad ik)
        [HarmonyPrefix] [HarmonyPatch(typeof(WeaponPos), nameof(WeaponPos.Start))] static void AddWeaponRenderers(WeaponPos __instance)
        {
            if (!__instance.GetComponent<PortalAwareRenderer>())
                __instance.gameObject.AddComponent<PortalAwareRenderer>().objectType = PortalAwareRenderer.ObjectType.Player;
        }
    }
}
