using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using VRTRAKILL.Utilities;

namespace Plugin.Systems.UI.Patches
{
    [HarmonyPatch] internal class HUDOptionsP
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CanvasController), nameof(CanvasController.Awake))]
        static void ResizeCanvases(CanvasController __instance)
        {
            // Stretches screen effects so it's not a small square in the middle of the hud
            string[] ScreenEffects =
            {
                "HurtScreen", "BlackScreen", "ParryFlash",
                "UnderwaterOverlay", "Black", "White"
            };
            foreach (string ScreenEffect in ScreenEffects)
                try
                {
                    Transform T = __instance.gameObject.transform.Find(ScreenEffect);
                    T.transform.localScale *= 10;
                    for (int i = 0; i < T.childCount; i++)
                        T.GetChild(i).transform.localScale /= 10;
                }
                catch { continue; }

            // prime bosses specific
            try { Object.FindObjectOfType<FlashImage>().transform.localScale *= 10; } catch { }

            // disable unnecessary stuff (for now)
            string[] ScreenEffectsToDisable =
            {
                "PowerUpVignette",
            };
            foreach (string ScreenEffectToDisable in ScreenEffectsToDisable)
                try { __instance.gameObject.transform.Find(ScreenEffectToDisable).GetComponent<Image>().enabled = false; } catch { continue; }

            // Relayer skybox in 2-4
            try { GameObject.Find("CityFromAbove").layer = 0; } catch { }

            UIConverter.RecursiveConvertCanvas(__instance.gameObject);
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CanvasController), nameof(CanvasController.OnEnable))]
        static void DeployGTFOTW(CanvasController __instance)
        {
            if (Assets.UI_GTFOTW == null) Assets.LoadAllCustomAssets();
            GameObject UI_GTFOTW = Object.Instantiate(Assets.UI_GTFOTW, Vector3.zero, Quaternion.identity, __instance.transform);

            Assets.UI_GTFOTW.transform.localScale = Vector3.zero;

            // Sets it's index to 0 so that it's above everything else
            UI_GTFOTW.transform.SetSiblingIndex(0);
            UI_GTFOTW.transform.localScale = Vector3.one;
            UI_GTFOTW.transform.localPosition = Vector3.zero;

            UIConverter.ConvertCanvas(UI_GTFOTW.GetComponent<Canvas>());
            VRTRAKILL.Utilities.Unity.RecursiveChangeLayer(UI_GTFOTW, (int)Layers.UI);

            GTFOTW GTFOTW = UI_GTFOTW.AddComponent<GTFOTW>();
            GTFOTW.DetectorTransform = Vars.MainCamera.transform;
        }

        [HarmonyPostfix] [HarmonyPatch(typeof(ScreenZone), nameof(ScreenZone.OnTriggerEnter))] static void ConvertThing()
        {
            //foreach (Canvas C in Resources.FindObjectsOfTypeAll(typeof(Canvas)))
            //    if (!C.gameObject.HasComponent<UICanvas>())
            UIConverter.RecursiveConvertCanvas();
        }

        /// <summary>
        ///     Fuck you
        /// </summary>
        [HarmonyPostfix] [HarmonyPatch(typeof(FlashImage), nameof(FlashImage.Flash))] static void FlashImageTweak(FlashImage __instance)
        {
            if (__instance.gameObject.name.Contains("White") || __instance.gameObject.name.Contains("Black"))
                __instance.transform.localScale *= 20;
        }

        //COULDNT BE BOTHERED TO TRANSPILE
        //This shit just doesn't work oh well
        //[HarmonyPrefix] [HarmonyPatch(typeof(LimboSkybox), nameof(LimboSkybox.InitializeRT))] static bool LimboSkyBoxFix(LimboSkybox __instance)
        //{
        //    int fakeWidth = (int)((float)XRSettings.eyeTextureWidth * 2 / __instance.downscaleFactor);
        //    int fakeHeight = (int)((float)XRSettings.eyeTextureHeight  * 2/ __instance.downscaleFactor);
        //    if (__instance.lastWidth != fakeWidth || __instance.lastHeight != fakeHeight)
        //    {
        //        if (__instance.skybox)
        //        {
        //            __instance.fakeCam.targetTexture = null;
        //            __instance.skybox.Release();
        //            if (Application.isPlaying)
        //            {
        //                Object.Destroy(__instance.skybox);
        //            }
        //        }
        //        __instance.lastWidth = fakeWidth;
        //        __instance.lastHeight = fakeHeight;
        //        var desc = new RenderTextureDescriptor(fakeWidth, fakeHeight, RenderTextureFormat.ARGB32);
        //        desc.vrUsage = VRTextureUsage.TwoEyes;  // important
        //        desc.depthBufferBits = 24;
        //        __instance.skybox = new RenderTexture(desc);
        //        __instance.fakeCam.targetTexture = __instance.skybox;
        //        Shader.SetGlobalTexture("_LimboSky", __instance.skybox);
        //        Shader.SetGlobalFloat("_LimboSkyWidth", (float)fakeWidth);
        //        Shader.SetGlobalFloat("_LimboSkyHeight", (float)fakeHeight);
        //    }
        //    return false;
        //}
    }
}
