using HarmonyLib;
using Plugin.Systems;
using Plugin.Systems.VRAvatar;
using UnityEngine;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch(typeof(RocketLauncher))] internal class PatchRocketLauncher
{
    [HarmonyPostfix] [HarmonyPatch(nameof(RocketLauncher.Update))]
    private static void Update_AddIK(RocketLauncher __instance)
    {
        if (VRigController.Instance != null)
        {
            __instance.transform.position = VRigController.Instance.Rig.FeedbackerB.Forearm.position;
            __instance.transform.LookAt(Vars.DominantHand.transform/*VRigController.Instance.Rig.FeedbackerB.Hand.Root*/);
        }
    }
}
