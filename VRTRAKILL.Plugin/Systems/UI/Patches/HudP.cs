using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace Plugin.Systems.UI.Patches
{
    [HarmonyPatch] internal sealed class HudP
    {
        // Magic numbers to make good hud good.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudController), nameof(HudController.Start))]
        static void ReparentHUD(HudController __instance)
        {
            if (__instance.altHud) return;
            else
            {
                __instance.transform.parent = CanvasController.Instance.transform;
                __instance.transform.localPosition = Vector3.zero;
                __instance.transform.localEulerAngles = Vector3.zero;
            }
        }
        [HarmonyPrefix] [HarmonyPatch(typeof(HUDPos), nameof(HUDPos.CheckPos))] static void RetransformElements(HUDPos __instance)
        {
            if (__instance.GetComponent<Canvas>() == null) return;
            UIConverter.ConvertCanvas(__instance.GetComponent<Canvas>(), Force: true, DontAddComponent: true);
            __instance.gameObject.AddComponent<HideWhenMenuActive>();

            __instance.transform.localScale = new Vector3(.5f, .35f, .5f) * 0.1f;
            switch (__instance.gameObject.name)
            {
                case "GunCanvas":
                    //StandardHUDWorker SHUDW = __instance.transform.parent.gameObject.AddComponent<StandardHUDWorker>();
                    //SHUDW.StandardHUD = __instance.gameObject;

                    __instance.defaultPos = new Vector3(-640, -420, 120) * 0.1f;
                    __instance.defaultRot = new Vector3(0, -20, 0);

                    __instance.reversePos = new Vector3(640, -420, 120) * 0.1f;
                    __instance.reverseRot = new Vector3(0, 20, 0);
                    break;

                case "StyleCanvas":
                    __instance.defaultPos = new Vector3(660, 60, -240)*0.1f;
                    __instance.defaultRot = new Vector3(356, 45, 357);

                    __instance.reversePos = new Vector3(-660, 60, -240)*0.1f;
                    __instance.reverseRot = new Vector3(0, -45, 0);
                    break;
            }
        }
        [HarmonyPostfix] [HarmonyPatch(typeof(FinalRank), nameof(FinalRank.Start))] static void CFinalRank(FinalRank __instance)
        {
            UIConverter.ConvertCanvas(__instance.transform.parent.GetComponent<Canvas>(), Force: true);
            __instance.transform.localPosition = Vector3.zero;
            __instance.transform.localScale = new Vector3(8, 4, 4)*0.3f;
        }
        [HarmonyPostfix] [HarmonyPatch(typeof(ScreenBlood), nameof(ScreenBlood.Start))] static void CScreenBlood(ScreenBlood __instance)
        {
            // makes blood splatters bigger for aesthetic reasons
            __instance.transform.localScale *= 10;
        }
    }
}
