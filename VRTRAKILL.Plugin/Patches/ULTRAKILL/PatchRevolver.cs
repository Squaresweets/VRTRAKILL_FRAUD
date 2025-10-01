using HarmonyLib;
using Plugin.Systems;
using UnityEngine;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch(typeof(Revolver))] internal class PatchRevolver
{
    [HarmonyPrefix] [HarmonyPatch(nameof(Revolver.ThrowCoin))]
    private static bool ThrowCoin(Revolver __instance)
    {
        if (Vars.Config.EnableMBP) __instance.camObj = Vars.NonDominantHand;
        else if (Vars.Config.EnableCBA) __instance.camObj = Vars.DominantHand;
        else __instance.camObj = Camera.main.gameObject;

        //I couldn't be bothered to transpile this
		if (__instance.punch == null || !__instance.punch.gameObject.activeInHierarchy)
			__instance.punch = MonoSingleton<FistControl>.Instance.currentPunch;
		if (__instance.punch)
			__instance.punch.CoinFlip();
		GameObject gameObject = Object.Instantiate<GameObject>(__instance.coin, __instance.camObj.transform.position, __instance.camObj.transform.rotation);
		gameObject.GetComponent<Coin>().sourceWeapon = __instance.gc.currentWeapon;
		MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.CoinToss);
		gameObject.GetComponent<Rigidbody>().AddForce(__instance.camObj.transform.forward * 20f + Vector3.up * 15f + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(true), ForceMode.VelocityChange);
		__instance.pierceCharge = 0f;
		__instance.pierceReady = false;

        return false;
    }
    [HarmonyPrefix] [HarmonyPatch(nameof(Revolver.Shoot))]
    private static void Shoot(Revolver __instance)
    {
        __instance.gunBarrel = Vars.DominantHand; //So it comes from the right positions
    }
}