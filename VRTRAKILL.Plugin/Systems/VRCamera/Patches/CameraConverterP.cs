using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
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

        static Quaternion startingRotation;
        [HarmonyPrefix] [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Start))] public static void Containerize()
        {
            Container = new GameObject("Main Camera Rig");
            Container.transform.parent = Vars.MainCamera.transform.parent;

            Container.transform.localPosition = new Vector3(0, -4.3f, 0);
            Container.transform.localRotation = Vars.MainCamera.transform.rotation;

            Container.AddComponent<VRCameraTurning>();

            Vars.MainCamera.transform.parent = Container.transform;
            //Vars.MainCamera.GetComponent<SteamVR_TrackedObject>().origin = Container.transform;
            //Vars.UICamera.transform.parent = Container.transform;

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

            //__instance.hudCamera.depth++;

            XRSettings.gameViewRenderMode = GameViewRenderMode.RightEye;

            // for some particular reason destroying it is a bad idea.
            GameObject.Find("Virtual Camera").SetActive(false);
        }
        [HarmonyPostfix] [HarmonyPatch(typeof(CameraController), nameof(CameraController.Start))] static void AddSVRCam(CameraController __instance)
        {

            //GameObject.DestroyImmediate(__instance.GetComponent<AudioLowPassFilter>());
            __instance.cam.targetTexture = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRDisplaySubsystem>().GetRenderTextureForRenderPass(0);

            //SteamVR_Camera c = __instance.gameObject.AddComponent<SteamVR_Camera>();
            //c.Expand();
            //SteamVR_TrackedObject t = GameObject.FindObjectOfType<SteamVR_TrackedObject>();
            //Plugin.Log.LogMessage("Parent:" + c.head.transform.name);
            //Plugin.Log.LogMessage("Parent:" + t.transform.GetChild(0).name);

            //__instance.gameObject.AddComponent<TrackedPoseDriver>();
            //tracker.origin = Container.transform;

            //Transform origin = new GameObject().transform;
            //origin.transform.parent = __instance.transform.parent;
            //origin.transform.position = __instance.transform.position;
            //offset = origin.transform.localPosition;
            ////origin.transform.position = __instance.transform.position;
            //__instance.transform.parent = origin;
            //__instance.gameObject.AddComponent<SteamVR_TrackedObject>().origin = origin;
            __instance.gameObject.AddComponent<SteamVR_TrackedObject>();

            //tracker.origin = origin;
            //__instance.gameObject.AddComponent<SteamVR_Render>();        // Create a new camera GameObject
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.Update))]
        static bool DoNothing(CameraController __instance)
        {
            __instance.transform.parent.localPosition = Vector3.zero;
            // do nothing
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.GetDefaultPos))]
        static bool GetDefaultPos(ref Vector3 __result)
        {
            if(Controllers.VRGunsSystem.instance == null) __result = Vector3.zero;
            __result = Vars.DominantHand.transform.position;
            return false;
        }
    }
}
