using Plugin.Systems;
using Plugin.Systems.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace VRBasePlugin.Systems.VRCamera
{
    [ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
    internal class VRControllerLocations : MonoSingleton<VRControllerLocations>
    {
        public Vector3 headPos;
        public Quaternion headRot = Quaternion.identity;

        public Vector3 leftPos;
        public Quaternion leftRot = Quaternion.identity;
        private Transform _left;

        public Vector3 rightPos;
        public Quaternion rightRot = Quaternion.identity;
        private Transform _right;

        IEnumerator Start()
        {
            transform.position = Vector3.zero; //IK its unneccessary but just make sure

            //if (_head != null) GameObject.DestroyImmediate(_head.gameObject);
            //_head = new GameObject("Head").transform;
            //_head.SetParent(transform);
            //_head.gameObject.AddComponent<SteamVR_TrackedObject>();

            SteamVR_Actions._default.Activate();
            yield return null;

            if (_left != null) GameObject.DestroyImmediate(_left.gameObject);
            _left = new GameObject("Left").transform;
            _left.SetParent(transform);
            SteamVR_Behaviour_Pose l = _left.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            l.poseAction = SteamVR_Actions._default.LeftPose;
            l.inputSource = SteamVR_Input_Sources.LeftHand;

            if (_right != null) GameObject.DestroyImmediate(_right.gameObject);
            _right = new GameObject("Right").transform;
            _right.SetParent(transform);
            SteamVR_Behaviour_Pose r = _right.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            r.poseAction = SteamVR_Actions._default.RightPose;
            r.inputSource = SteamVR_Input_Sources.RightHand;

            l.gameObject.SetActive(false);
            r.gameObject.SetActive(false);
            yield return null;
            l.gameObject.SetActive(true);
            r.gameObject.SetActive(true);
        }

        private bool IsTurning; private float SnapTurnTimer;
        public void Update()
        {
            //if (_head != null)
            {
                //headPos = _head.position;
                //headRot = _head.rotation;
                headPos = InputTracking.GetLocalPosition(XRNode.Head);
                headRot = InputTracking.GetLocalRotation(XRNode.Head);
            }
            if (_left != null)
            {
                leftPos = _left.position;
                leftRot = _left.rotation;
            }
            if (_right != null)
            {
                rightPos = _right.position;
                rightRot = _right.rotation;
            }

            if (!Vars.Config.Controllers.SnapTurn)
            {
                if (InputVars.TurnVector.x > 0 + Vars.Config.Controllers.Deadzone)
                    InputVars.TurnOffset += Vars.Config.Controllers.SmoothSpeed * Time.deltaTime;
                if (InputVars.TurnVector.x < 0 - Vars.Config.Controllers.Deadzone)
                    InputVars.TurnOffset -= Vars.Config.Controllers.SmoothSpeed * Time.deltaTime;
            }
            else
            {
                if (IsTurning)
                {
                    SnapTurnTimer += Time.deltaTime;
                    if (SnapTurnTimer >= .2f || InputVars.TurnVector.x == 0) { IsTurning = false; SnapTurnTimer = 0; }
                }
                else
                {
                    if (InputVars.TurnVector.x > 0 + Vars.Config.Controllers.Deadzone)
                    { IsTurning = true; InputVars.TurnOffset += Vars.Config.Controllers.SnapAngles; }
                    else if (InputVars.TurnVector.x < 0 - Vars.Config.Controllers.Deadzone)
                    { IsTurning = true; InputVars.TurnOffset -= Vars.Config.Controllers.SnapAngles; }
                }
            }
        }
    }
}
