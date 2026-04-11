using HarmonyLib;
using Plugin.Systems.Input;
using System.Linq;
using System.Collections.Generic;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Valve.VR;
using VRBasePlugin.Systems.VRCamera;
using Plugin.Systems;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Plugin.Systems.VRCamera.Patches
{
    [HarmonyPatch] public class CameraConverterP
    {
        // ty huskvr you pretty
        public static Camera DesktopWorldCam, DesktopUICam;
        public static Camera leftEye, rightEye;

        public static void SetupEyeCameras()
        {
            CameraController cc = CameraController.Instance;

            cc.cam.nearClipPlane = 0.01f;
            cc.cam.stereoTargetEye = StereoTargetEyeMask.None;

            //// some binary magic (that i don't understand) to enable the layer with the hands
            cc.cam.cullingMask |= 1 << (int)Layers.AlwaysOnTop;
            cc.hudCamera.enabled = false;

            leftEye = new GameObject("Left", typeof(Camera)).GetComponent<Camera>();
            VRTRAKILL.Utilities.Unity.CopyCameraValues(leftEye, cc.cam);
            leftEye.transform.SetParent(cc.transform.parent);
            leftEye.stereoTargetEye = StereoTargetEyeMask.Left;

            rightEye = new GameObject("Right", typeof(Camera)).GetComponent<Camera>();
            VRTRAKILL.Utilities.Unity.CopyCameraValues(rightEye, cc.cam);
            rightEye.transform.SetParent(cc.transform.parent);
            rightEye.stereoTargetEye = StereoTargetEyeMask.Right;
        }
        [HarmonyPrefix] [HarmonyPatch(typeof(CameraController), nameof(CameraController.Start))] static void ConvertCameras(CameraController __instance)
        {
            while (__instance.cam == null && __instance.hudCamera == null) {}

            // for some particular reason destroying it is a bad idea.
            GameObject.Find("Virtual Camera").SetActive(false);

            __instance.cam.enabled = false;

            //#region Desktop View
            //DesktopWorldCam = new GameObject("Desktop World Camera").AddComponent<Camera>();
            //DesktopWorldCam.transform.parent = leftEye.transform;
            //DesktopWorldCam.transform.localPosition = Vector3.zero;
            //DesktopWorldCam.gameObject.AddComponent<DesktopCamera>();

            //DesktopUICam = new GameObject("Desktop UI Camera").AddComponent<Camera>();
            //DesktopUICam.transform.parent = leftEye.transform;
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

            InputVars.TurnOffset = 0;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CameraController), nameof(CameraController.LateUpdate))]
        static void HandleRotationsAndPositions(CameraController __instance)
        {
            // do nothing
            if (!__instance.player) { __instance.nm = NewMovement.Instance; __instance.player = __instance.nm.gameObject; }

            __instance.rotationX = -VRControllerLocations.Instance.headRot.eulerAngles.x;
            __instance.rotationY = VRControllerLocations.Instance.headRot.eulerAngles.y + InputVars.TurnOffset;
            __instance.tiltRotationZ = VRControllerLocations.Instance.headRot.eulerAngles.z;
            __instance.ApplyRotations();

            PortalAwareSetTransformFromBody(leftEye.transform, InputTracking.GetLocalPosition(XRNode.LeftEye), __instance.transform.rotation);
            PortalAwareSetTransformFromBody(rightEye.transform, InputTracking.GetLocalPosition(XRNode.RightEye), __instance.transform.rotation);
        }

        public static void PortalAwareSetTransformFromBody(Transform t, Vector3 localPosition, Quaternion worldRotation, bool hands = false)
        {
            CameraController cc = CameraController.Instance;
            Quaternion parentRot =
                cc.gravityRotation *
                Quaternion.AngleAxis(InputVars.TurnOffset, Vector3.up);

            Vector3 transformedLocalPos = parentRot * localPosition;
            t.position = cc.transform.parent.position + transformedLocalPos;

            if (hands) t.rotation = parentRot * worldRotation;
            else t.rotation = worldRotation;

            MoveFromPlayerThroughPortals(t);
        }

        public static void MoveFromPlayerThroughPortals(Transform t)
        {
            CameraController cc = CameraController.Instance;
            PortalManagerV2 pm = PortalManagerV2.Instance;
            PortalScene Scene = pm.Scene;

            PortalHandle portalHandle;
            Vector3 intersection;
            PortalTravellerFlags portalTravellerFlags = PortalTravellerFlags.Player;

            //Code from PortalManager2
            if (Scene.FindCrossedPortal(cc.transform.parent.position, t.transform.position, out portalHandle, out intersection))
            {
                Portal portalObject = Scene.GetPortalObject(portalHandle);
                PortalTravellerFlags travelFlags = portalObject.GetTravelFlags(portalHandle.side);
                bool canTravel = travelFlags.HasFlag(portalTravellerFlags);
                PortalHandleSequence portalSequence = new PortalHandleSequence(new PortalHandle[]
                {
                    portalHandle
                });
                if (canTravel)
                {
                    Matrix4x4 travelMatrix = Scene.GetTravelMatrix(portalHandle);
                    Vector3 vector2 = travelMatrix.MultiplyPoint3x4(intersection);
                    Vector3 direction = travelMatrix.MultiplyPoint3x4(t.transform.position) - vector2;
                    PortalTraversalV2[] intersections;
                    PortalPhysicsV2.ProjectThroughPortals(vector2, direction, pm.empty_lm, out _, out _, out intersections);
                    for (int i = 0; i < intersections.Length; i++)
                    {
                        PortalHandle portalHandle2 = intersections[i].portalHandle;
                        if (!Scene.GetPortalObject(portalHandle2).GetTravelFlags(intersections[i].portalHandle.side).HasFlag(portalTravellerFlags))
                        {
                            canTravel = false;
                            break;
                        }
                    }
                    if (canTravel)
                    {
                        if (intersections.Length != 0)
                        {
                            portalSequence = PortalHandleSequence.Prepend(portalHandle, intersections);
                            travelMatrix = Scene.GetTravelMatrix(portalSequence);
                        }
                        PortalTravelDetails details = PortalTravelDetails.WithInteresction(portalSequence, intersections, travelMatrix, intersection);

                        //Actually do the movement
                        t.transform.position = details.enterToExit.MultiplyPoint3x4(t.transform.position);
                        t.transform.rotation = details.enterToExit.rotation * t.transform.rotation;
                    }
                }
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerAnimations), nameof(PlayerAnimations.Start))]
        static void RemovePlayerModel(PlayerAnimations __instance)
        {
            //Add in the VR head
            GameObject head = GameObject.Instantiate(Assets.VHead, leftEye.transform);
            head.transform.localPosition = new Vector3(0, 0, -0.07f);
            head.transform.localScale *= 2f;
            Object.Destroy(head.GetComponent<CapsuleCollider>());
            foreach (Transform t in head.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = LayerMask.NameToLayer("Portal");
            //Done so we get the correct unlit look
            head.GetComponentInChildren<SkinnedMeshRenderer>().material = __instance.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material;

            __instance.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToList().ForEach(x => UnityEngine.Object.Destroy(x));
            __instance.GetComponentsInChildren<MeshRenderer>(true).ToList().ForEach(x => UnityEngine.Object.Destroy(x));
            __instance.GetComponentsInChildren<GunColorGetter>(true).ToList().ForEach(x => UnityEngine.Object.Destroy(x));
        }
    }
}
