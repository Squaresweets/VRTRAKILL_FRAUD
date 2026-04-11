using HarmonyLib;
using ULTRAKILL.Portal;
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LimboSkybox), nameof(LimboSkybox.UpdateCamera))]
        static bool LimboFix(Camera cam, LimboSkybox __instance)
        {
            if (!__instance.isActiveAndEnabled) return false;
            __instance.InitializeRT();
            if (Application.isPlaying) __instance.playerCam = __instance.cc.cam;
            if (cam != null) __instance.playerCam = cam;
            if (__instance.playerCam == null) return false;
            Vector3 vector = (__instance.playerCam.transform.position - __instance.playerStartPos) / 16f;
            float num = __instance.lockMinimumHeight ? Mathf.Max(vector.y, 0f) : vector.y;
            __instance.fakeCam.transform.position = __instance.fakeCamStart.position + new Vector3(vector.x, num, vector.z);
            __instance.fakeCam.transform.rotation = __instance.playerCam.transform.rotation;
            __instance.fakeCam.cullingMask = __instance.playerCam.cullingMask;
            __instance.fakeCam.fieldOfView = __instance.playerCam.fieldOfView;
            __instance.fakeCam.targetTexture = __instance.skybox;

            __instance.fakeCam.projectionMatrix = __instance.playerCam.projectionMatrix; //ONLY ADDED LINE

            Shader.SetGlobalTexture("_LimboSky", __instance.skybox);
            Shader.SetGlobalFloat("_LimboSkyWidth", (float)__instance.lastWidth);
            Shader.SetGlobalFloat("_LimboSkyHeight", (float)__instance.lastHeight);
            __instance.fakeCam.Render();

            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpaceSkybox), nameof(SpaceSkybox.UpdateCamera))]
        static void SpaceFix(Camera cam, SpaceSkybox __instance)
        {
            if (!__instance.isActiveAndEnabled) return;
            if (Application.isPlaying) __instance.playerCam = __instance.cc.cam;
            if (cam != null) __instance.playerCam = cam;
            if (__instance.playerCam == null) return;
            __instance.fakeCam.transform.rotation = __instance.playerCam.transform.rotation;
            __instance.fakeCam.cullingMask = __instance.playerCam.cullingMask;
            __instance.fakeCam.fieldOfView = __instance.playerCam.fieldOfView;
            __instance.fakeCam.targetTexture = __instance.skybox;
            __instance.fakeCam.projectionMatrix = __instance.playerCam.projectionMatrix; //ONLY ADDED LINE
            __instance.fakeCam.Render();
        }
    }
}
