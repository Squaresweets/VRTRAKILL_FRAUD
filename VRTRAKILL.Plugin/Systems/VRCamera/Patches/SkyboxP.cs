using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Plugin.Systems.VRCamera.Patches
{
    [HarmonyPatch]
    public class SkyboxP
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LimboSkybox), nameof(LimboSkybox.UpdateCamera))]
        static bool LimboFix2(Camera cam, LimboSkybox __instance)
        {
            PortalPatch.AddLimboSkyboxToAllPPHs(__instance); //AND THIS

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
            Shader.SetGlobalFloat("_LimboSkyWidth", __instance.lastWidth);
            Shader.SetGlobalFloat("_LimboSkyHeight", __instance.lastHeight);
            __instance.fakeCam.Render();

            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpaceSkybox), nameof(SpaceSkybox.UpdateCamera))]
        static bool SpaceFix(Camera cam, SpaceSkybox __instance)
        {
            if (!__instance.isActiveAndEnabled) return false;
            if (Application.isPlaying) __instance.playerCam = __instance.cc.cam;
            if (cam != null) __instance.playerCam = cam;
            if (__instance.playerCam == null) return false;
            __instance.fakeCam.transform.rotation = __instance.playerCam.transform.rotation;
            __instance.fakeCam.cullingMask = __instance.playerCam.cullingMask;
            __instance.fakeCam.fieldOfView = __instance.playerCam.fieldOfView;
            __instance.fakeCam.targetTexture = __instance.skybox;
            __instance.fakeCam.projectionMatrix = __instance.playerCam.projectionMatrix; //ONLY ADDED LINE
            __instance.fakeCam.Render();

            return false;
        }
    }
}
