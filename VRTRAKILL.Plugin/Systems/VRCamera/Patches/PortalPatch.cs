using HarmonyLib;
using Plugin.Systems.VRCamera.Patches;
using System;
using System.IO;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Plugin.Systems.VRCamera.Patches
{
    [HarmonyPatch]
    public class PortalPatch
    {
        static PortalRenderV2 rightEye; //Left is using the default one

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnEnable))]
        static void OnEnableThing(PortalManagerV2 __instance)
        {
            CameraConverterP.SetupEyeCameras();
            __instance.mainCamera = CameraConverterP.leftEye;
            //Need to set it all up
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnEnable))]
        static void OnEnableThingAfter(PortalManagerV2 __instance)
        {
            rightEye = __instance.gameObject.AddComponent<PortalRenderV2>();
            rightEye.portalCompositeMaterial = new Material(__instance.render.portalCompositeMaterial);
            rightEye.portalMaterial = new Material(__instance.render.portalMaterial);
            rightEye.portalBitset64DownsampleMat = new Material(__instance.render.portalBitset64DownsampleMat);
            rightEye.fakeRecursionCopy = new Material(__instance.render.fakeRecursionCopy);
            rightEye.obliqueCutoff = 0.2f;
            rightEye.mainCam = CameraConverterP.rightEye;
            //Need to set it all up
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PostProcessV2_Handler), nameof(PostProcessV2_Handler.LateUpdate))]
        static void LateUpdatePP(PostProcessV2_Handler __instance)
        {
            __instance.mainCam = CameraConverterP.leftEye;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.LateUpdate))]
        static void LateUpdateTest(PortalManagerV2 __instance)
        {
            if (__instance.mainCamera)
                __instance.mainCamera.projectionMatrix = __instance.mainCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            if (rightEye.mainCam)
                rightEye.mainCam.projectionMatrix = rightEye.mainCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            __instance.mainCamera = CameraConverterP.leftEye;
        }
        static bool doInitialize = true;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.LateUpdate))]
        static void LateUpdateThing(PortalManagerV2 __instance)
        {
            doInitialize = false;
            if (__instance.initialized)
                rightEye.Setup(__instance.Scene, CameraConverterP.rightEye, __instance.portalCamera);
            doInitialize = true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalRenderV2), nameof(PortalRenderV2.DepthPrepass))]
        static bool DisableInitialise(PortalRenderV2 __instance)
        {
            if (doInitialize) return true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnPreRenderCallback))]
        static void BeforeOnPreRenderCallback(PortalManagerV2 __instance, Camera cam)
        {
            Debug.LogError(cam.name);
            //Debug.LogError($"{cam.name}");
            //if (cam.GetCommandBuffers(CameraEvent.BeforeForwardOpaque).Length > 0)
            //    Debug.LogError($"{cam.name} {cam.GetCommandBuffers(CameraEvent.BeforeForwardOpaque)[0].name}");

            //__instance.portalCamera.stereoTargetEye = StereoTargetEyeMask.None;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnPreRenderCallback))]
        static void OnPreRenderCallback(PortalManagerV2 __instance, Camera cam)
        {
            if (__instance == null || PortalManagerV2.Instance == null) return;
            if (cam == CameraConverterP.rightEye) rightEye.Render(cam);
            //Object.Destroy(otherMainTex);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.SetPortalOcclusion))]
        static void SetPortalOcclusion(bool enabled)
        {
            rightEye.SetPortalOcclusion(enabled);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalRenderV2), nameof(PortalRenderV2.SetupRenderData))]
        static void PortalRenderFix(PortalRenderV2 __instance)
        {
            if (__instance.portalCam)
                __instance.portalCam.stereoTargetEye = StereoTargetEyeMask.None;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalRenderV2), nameof(PortalRenderV2.Render))]
        static void PortalRenderFix2(PortalRenderV2 __instance)
        {
            if (__instance.portalCam)
                __instance.portalCam.stereoTargetEye = StereoTargetEyeMask.None;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PostProcessV2_Handler), nameof(PostProcessV2_Handler.SetupRTs))]
        static void PPFix(PostProcessV2_Handler __instance)
        {
            //__instance.mainTex.vrUsage = VRTextureUsage.TwoEyes;
        }

        public static void Save(RenderTexture rt, string path)
        {
            // Read RT into Texture2D
            RenderTexture current = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = current;

            SaveTexture(tex, path);
            UnityEngine.Object.Destroy(tex);
        }

        static void SaveTexture(Texture2D tex, string path)
        {
            int w = tex.width;
            int h = tex.height;
            Color32[] pixels = tex.GetPixels32();

            int rowSize = (w * 3 + 3) & ~3; // rows padded to 4 bytes
            int dataSize = rowSize * h;
            int fileSize = 54 + dataSize;

            using (var bw = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                // BMP header
                bw.Write((byte)'B');
                bw.Write((byte)'M');
                bw.Write(fileSize);
                bw.Write(0);
                bw.Write(54);

                // DIB header (BITMAPINFOHEADER)
                bw.Write(40);
                bw.Write(w);
                bw.Write(h);
                bw.Write((short)1);
                bw.Write((short)24); // 24-bit
                bw.Write(0);
                bw.Write(dataSize);
                bw.Write(0);
                bw.Write(0);
                bw.Write(0);
                bw.Write(0);

                // Pixel data (BGR, bottom-up)
                byte[] padding = new byte[rowSize - w * 3];

                for (int y = 0; y < h; y++)
                {
                    int row = y * w;
                    for (int x = 0; x < w; x++)
                    {
                        Color32 c = pixels[row + x];
                        bw.Write(c.b);
                        bw.Write(c.g);
                        bw.Write(c.r);
                    }
                    bw.Write(padding);
                }
            }
        }
    }
}
