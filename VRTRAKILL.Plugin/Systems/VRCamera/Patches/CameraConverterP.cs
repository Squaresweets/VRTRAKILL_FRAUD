using HarmonyLib;
using Plugin.Systems.Input;
using System;
using System.Collections.Generic;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Valve.VR;
using VRBasePlugin.Systems.VRCamera;
namespace Plugin.Systems.VRCamera.Patches
{
    [HarmonyPatch] public class CameraConverterP
    {
        // ty huskvr you pretty
        public static Camera DesktopWorldCam, DesktopUICam;
        public static Camera leftEye, rightEye;

        [HarmonyPrefix] [HarmonyPatch(typeof(CameraController), nameof(CameraController.Start))] static void ConvertCameras(CameraController __instance)
        {
            while (__instance.cam == null && __instance.hudCamera == null) {}

            __instance.cam.nearClipPlane = .01f;
            __instance.cam.stereoTargetEye = StereoTargetEyeMask.None;

            //// some binary magic (that i don't understand) to enable the layer with the hands
            __instance.cam.cullingMask |= 1 << (int)Layers.AlwaysOnTop;
            __instance.hudCamera.enabled = false;

            // for some particular reason destroying it is a bad idea.
            GameObject.Find("Virtual Camera").SetActive(false);

            VRControllerLocations.Instance.CalculateEyeOffsets();
            leftEye = new GameObject("Left", typeof(Camera)).GetComponent<Camera>();
            VRTRAKILL.Utilities.Unity.CopyCameraValues(leftEye, __instance.cam);
            leftEye.transform.SetParent(__instance.transform.parent);
            leftEye.transform.localPosition = VRControllerLocations.Instance.leftEyeOffset;
            leftEye.stereoTargetEye = StereoTargetEyeMask.Left;

            rightEye = new GameObject("Right", typeof(Camera)).GetComponent<Camera>();
            VRTRAKILL.Utilities.Unity.CopyCameraValues(rightEye, __instance.cam);
            rightEye.transform.SetParent(__instance.transform.parent);
            rightEye.transform.localPosition = VRControllerLocations.Instance.rightEyeOffset;
            rightEye.stereoTargetEye = StereoTargetEyeMask.Right;

            __instance.cam.enabled = false;

            //#region Desktop View
            //DesktopWorldCam = new GameObject("Desktop World Camera").AddComponent<Camera>();
            //DesktopWorldCam.transform.parent = Vars.MainCamera.transform;
            //DesktopWorldCam.transform.localPosition = Vector3.zero;
            //DesktopWorldCam.gameObject.AddComponent<DesktopCamera>();

            //DesktopUICam = new GameObject("Desktop UI Camera").AddComponent<Camera>();
            //DesktopUICam.transform.parent = Vars.MainCamera.transform;
            //DesktopUICam.transform.localPosition = Vector3.zero;
            //DesktopUICam.gameObject.AddComponent<DesktopUICamera>();
            //if (!Vars.Config.DesktopView.Enabled)
            //{
            //    DesktopWorldCam.gameObject.SetActive(false);
            //    DesktopUICam.gameObject.SetActive(false);
            //}
            //#endregion
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


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.LateUpdate))]
        static void HandleRotationsAndPositions(CameraController __instance)
        {
            // do nothing
            if (!__instance.nm) return;

            __instance.rotationX = -VRControllerLocations.Instance.headRot.eulerAngles.x;
            __instance.rotationY = VRControllerLocations.Instance.headRot.eulerAngles.y + InputVars.TurnOffset;
            __instance.tiltRotationZ = VRControllerLocations.Instance.headRot.eulerAngles.z;
            __instance.ApplyRotations();

            PortalAwareSetTransformFromBody(__instance.transform, VRControllerLocations.Instance.headPos, __instance.transform.rotation);
            PortalAwareSetTransformFromBody(leftEye.transform, InputTracking.GetLocalPosition(XRNode.LeftEye), __instance.transform.rotation);
            PortalAwareSetTransformFromBody(rightEye.transform, InputTracking.GetLocalPosition(XRNode.RightEye), __instance.transform.rotation);
        }

        public static void PortalAwareSetTransformFromBody(Transform t, Vector3 localPosition, Quaternion worldRotation)
        {
            CameraController cc = CameraController.Instance;
            Vector3 transformedLocalPos = cc.gravityRotation * Quaternion.AngleAxis(InputVars.TurnOffset, Vector3.up) * localPosition;
            t.position = cc.transform.parent.position + transformedLocalPos;
            t.rotation = worldRotation;

            MoveFromPlayerThroughPortals(t);
        }

        public static void MoveFromPlayerThroughPortals(Transform t)
        {
            CameraController cc = CameraController.Instance;
            PortalHandle hitPortal;
            if (PortalManagerV2.Instance.Scene.FindPortalBetween(cc.transform.parent.position, t.position, out hitPortal, out _, out _, true))
            {
                Matrix4x4 travelmatrix = PortalManagerV2.Instance.Scene.GetPortalObject(hitPortal).travelMatrix;

                t.position = travelmatrix.MultiplyPoint3x4(t.position);
                t.rotation = travelmatrix.rotation * t.rotation;
            }
        }

        [HarmonyPrefix] [HarmonyPatch(typeof(CameraController), nameof(CameraController.Transform))]
        static void TransformBefore(CameraController __instance, ref float __state)
        {
            __state = __instance.rotationY;
        }
        [HarmonyPostfix] [HarmonyPatch(typeof(CameraController), nameof(CameraController.Transform))]
        static void TransformAfter(CameraController __instance, ref float __state)
        {
            InputVars.TurnOffset += __instance.rotationY - __state;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.GetDefaultPos))]
        static bool GetDefaultPos(ref Vector3 __result)
        {
            if(Controllers.VRGunsSystem.Instance == null) __result = Vector3.zero;
            __result = Vars.DominantHand.transform.position;
            return false;
        }
    }
}
