using HarmonyLib;
using Plugin.Systems;
using Plugin.Systems.VRAvatar;
using UnityEngine;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch(typeof(RocketLauncher))]
internal class PatchRocketLauncher
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(RocketLauncher.Update))]
    private static void Update_AddIK(RocketLauncher __instance)
    {
        if (VRigController.Instance != null)
        {
            __instance.transform.position = VRigController.Instance.Rig.FeedbackerB.Forearm.position;
            __instance.transform.LookAt(Vars.DominantHand.transform/*VRigController.Instance.Rig.FeedbackerB.Hand.Root*/);
        }
    }

    static Vector3 oldPos;
    static Quaternion oldRot;
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RocketLauncher.Shoot))]
    [HarmonyPatch(nameof(RocketLauncher.ShootCannonball))]
    [HarmonyPatch(nameof(RocketLauncher.ShootNapalm))]
    private static void FixFiringDir(RocketLauncher __instance)
    {
        CameraController cam = MonoSingleton<CameraController>.Instance;
        if (cam == null || Vars.DominantHand == null) return;

        oldPos = cam.transform.position;
        oldRot = cam.transform.rotation;

        cam.transform.SetPositionAndRotation(Vars.DominantHand.transform.position, Vars.DominantHand.transform.rotation);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(RocketLauncher.Shoot))]
    [HarmonyPatch(nameof(RocketLauncher.ShootCannonball))]
    [HarmonyPatch(nameof(RocketLauncher.ShootNapalm))]
    static void FixFiringDir2()
    {
        CameraController cam = MonoSingleton<CameraController>.Instance;
        if (cam == null) return;

        cam.transform.SetPositionAndRotation(oldPos, oldRot);
    }
}
