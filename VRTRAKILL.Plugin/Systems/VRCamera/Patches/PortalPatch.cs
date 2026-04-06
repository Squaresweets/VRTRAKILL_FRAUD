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
        static PortalRenderV2 leftEyeRender;
        static PostProcessV2_Handler leftEyePP;

        static PortalRenderV2 rightEyeRender; //Left is using the default one
        static PostProcessV2_Handler rightEyePP;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnDisable))]
        static void OnDisableThing(PortalManagerV2 __instance)
        {
            Debug.LogError("DISABLING PORTAL MANAGER AND DISPOSING THE THING");
        }
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
            leftEyeRender = __instance.render;

            rightEyeRender = __instance.gameObject.AddComponent<PortalRenderV2>();
            rightEyeRender.portalCompositeMaterial = new Material(leftEyeRender.portalCompositeMaterial);
            rightEyeRender.portalMaterial = new Material(leftEyeRender.portalMaterial);
            rightEyeRender.portalBitset64DownsampleMat = new Material(leftEyeRender.portalBitset64DownsampleMat);
            rightEyeRender.fakeRecursionCopy = new Material(leftEyeRender.fakeRecursionCopy);
            rightEyeRender.obliqueCutoff = 0.2f;
            rightEyeRender.mainCam = CameraConverterP.rightEye;

            //Copy it all over to a new one lmao
            leftEyePP = PostProcessV2_Handler.Instance;

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
        static void LateUpdateTest(PortalManagerV2 __instance)
        {
            PostProcessV2_Handler.Instance = leftEyePP;
            leftEyeRender.pph = leftEyePP;

            if (__instance.mainCamera)
                __instance.mainCamera.projectionMatrix = __instance.mainCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            __instance.mainCamera = CameraConverterP.leftEye;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.LateUpdate))]
        static void LateUpdateThing(PortalManagerV2 __instance)
        {
            PostProcessV2_Handler.Instance = rightEyePP;
            rightEyeRender.pph = rightEyePP;

            if (rightEyeRender.mainCam)
                rightEyeRender.mainCam.projectionMatrix = rightEyeRender.mainCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            if (__instance.initialized)
                rightEyeRender.Setup(__instance.Scene, CameraConverterP.rightEye, __instance.portalCamera);
            PostProcessV2_Handler.Instance = leftEyePP;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnPreRenderCallback))]
        static void BeforeOnPreRenderCallback(PortalManagerV2 __instance, Camera cam)
        {
            //Debug.LogError(cam.name);
            //Debug.LogError(__instance.Scene.renderHandles);
            //Debug.LogError(__instance.Scene.renderHandles.Length);
            PostProcessV2_Handler.Instance = leftEyePP;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortalManagerV2), nameof(PortalManagerV2.OnPreRenderCallback))]
        static void OnPreRenderCallback(PortalManagerV2 __instance, Camera cam)
        {
            if (__instance == null || PortalManagerV2.Instance == null) return;
            PostProcessV2_Handler.Instance = rightEyePP;
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
