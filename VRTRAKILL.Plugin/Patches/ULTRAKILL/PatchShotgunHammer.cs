using HarmonyLib;
using Plugin.Systems;
using Plugin.Systems.VRAvatar;
using UnityEngine;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch(typeof(ShotgunHammer))] internal class PatchShotgunHammer
{
    [HarmonyPostfix] [HarmonyPatch(nameof(ShotgunHammer.LateUpdate))]
    static void LateUpdate_AddIK(ShotgunHammer __instance)
    {
        if (VRigController.Instance != null)
        {
            __instance.transform.position = VRigController.Instance.Rig.FeedbackerB.Forearm.position;
            __instance.transform.LookAt(VRigController.Instance.Rig.FeedbackerB.Hand.Root.position);
        }
    }
}
