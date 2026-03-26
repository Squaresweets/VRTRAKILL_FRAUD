using HarmonyLib;
using Plugin.Systems.Input;
using System.Collections.Generic;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Valve.VR;
namespace Plugin.Systems.VRCamera.Patches
{
    [HarmonyPatch] public class CameraConverterP
    {
        // ty huskvr you pretty
        public static GameObject Container;
        public static Camera DesktopWorldCam, DesktopUICam;
        public static Transform headPos;

        static Quaternion startingRotation;
        [HarmonyPrefix] [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Start))] public static void Containerize(NewMovement __instance)
        {
            CameraController cc = __instance.cc;

            Container = __instance.gameObject;
            Container.AddComponent<VRCameraTurning>();

            #region Desktop View
            DesktopWorldCam = new GameObject("Desktop World Camera").AddComponent<Camera>();
            DesktopWorldCam.transform.parent = Vars.MainCamera.transform;
            DesktopWorldCam.transform.localPosition = Vector3.zero;
            DesktopWorldCam.gameObject.AddComponent<DesktopCamera>();
            DesktopUICam = new GameObject("Desktop UI Camera").AddComponent<Camera>();
            DesktopUICam.transform.parent = Vars.MainCamera.transform;
            DesktopUICam.transform.localPosition = Vector3.zero;
            DesktopUICam.gameObject.AddComponent<DesktopUICamera>();
            if (!Vars.Config.DesktopView.Enabled)
            {
                DesktopWorldCam.gameObject.SetActive(false);
                DesktopUICam.gameObject.SetActive(false);
            }
            #endregion

            //Move the actual camera to a child so we can do more stuff with it

            //cameraChild = new GameObject("Camera child");
            //cameraChild.transform.SetParent(cc.transform, false);
            //cameraChild.AddComponent<Camera>().CopyFrom(cc.cam);
            //GameObject.DestroyImmediate(cc.cam);
            //cc.cam = cameraChild.GetComponent<Camera>();

            headPos = new GameObject("Head pos").transform;
            headPos.gameObject.AddComponent<SteamVR_TrackedObject>();

            cc.cam.targetTexture = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRDisplaySubsystem>().GetRenderTextureForRenderPass(0);
        }
        [HarmonyPostfix] [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.GetHurt))] public static void FixWeirdDeathThing(NewMovement __instance)
        {
            if (__instance.dead)
            {
                __instance.rb.constraints = __instance.defaultRBConstraints;
                __instance.cc.enabled = true;
            }

        }
        [HarmonyPostfix] [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Start))] static void ScaleObjects(NewMovement __instance)
        {
            // this should've been bigger, but i've changed my mind a thousand years ago and it works
            // this mod is officially my opus magnum spaghetti code and dumpster fire
            //Container.transform.localScale = new Vector3(2, 2, 2);
            __instance.gameObject.AddComponent<VRPlayer.VRKeybindsController>();
        }

        [HarmonyPrefix] [HarmonyPatch(typeof(CameraController), nameof(CameraController.Start))] static void ConvertCameras(CameraController __instance)
        {
            while (__instance.cam == null && __instance.hudCamera == null) {}

            __instance.cam.nearClipPlane = .01f;
            __instance.cam.stereoTargetEye = StereoTargetEyeMask.Both;

            //// some binary magic (that i don't understand) to enable the layer with the hands
            __instance.cam.cullingMask |= 1 << (int)Layers.AlwaysOnTop;
            __instance.hudCamera.enabled = false;

            XRSettings.gameViewRenderMode = GameViewRenderMode.RightEye;

            // for some particular reason destroying it is a bad idea.
            GameObject.Find("Virtual Camera").SetActive(false);
        }

        static float rotationYOffset;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.LateUpdate))]
        static void HandleRotationsAndPositions(CameraController __instance)
        {
            // do nothing
            if (!__instance.nm || __instance.platformerCamera) return;

            __instance.transform.localPosition = Vector3.zero;

            __instance.rotationX = -headPos.localEulerAngles.x;
            __instance.rotationY = headPos.localEulerAngles.y + rotationYOffset + InputVars.TurnOffset;
            __instance.tiltRotationZ = headPos.localEulerAngles.z;
            __instance.ApplyRotations();

            __instance.transform.localPosition = headPos.position;
        }

        [HarmonyPrefix] [HarmonyPatch(typeof(CameraController), nameof(CameraController.Transform))]
        static void TransformBefore(CameraController __instance, ref float __state)
        {
            __state = __instance.rotationY;
        }
        [HarmonyPostfix] [HarmonyPatch(typeof(CameraController), nameof(CameraController.Transform))]
        static void TransformAfter(CameraController __instance, ref float __state)
        {
            rotationYOffset += __instance.rotationY - __state;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.GetDefaultPos))]
        static bool GetDefaultPos(ref Vector3 __result)
        {
            if(Controllers.VRGunsSystem.Instance == null) __result = Vector3.zero;
            __result = Vars.DominantHand.transform.position;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortalRenderV2), nameof(PortalRenderV2.Render))]
        static void PortalRenderFix2(PortalRenderV2 __instance)
        {
            if(__instance.portalCam)
            __instance.portalCam.stereoTargetEye = StereoTargetEyeMask.None;
        }
    }
}
