using Unity.XR.CoreUtils;
using UnityEngine;

namespace Plugin.Systems.UI
{
    // "borrowed" from huskvr
    internal sealed class UICanvas : MonoBehaviour
    {
        private Vector3 LastCamFwd = Vector3.zero;

        private const float Distance = 72f;
        private static float Scale => Vars.Config.UIInteraction.UISize;

        private void UpdatePos()
        {
            LastCamFwd = Vars.MainCamera.transform.forward * Distance;
            transform.rotation =  Vars.MainCamera.transform.rotation;
        }
        private void ResetPos()
        {
            Vector3 customUp = CameraController.Instance.gravityRotation * Vector3.up;
            
            LastCamFwd = Vector3.ProjectOnPlane(LastCamFwd, customUp);
            if (LastCamFwd != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(LastCamFwd, customUp);
        }

        public void Start()
        {
            transform.localScale = Vector3.one * Scale;
            LastCamFwd = Vector3.back * Distance;
            UpdatePos();
        }
        public void Update()
        {
            if (!Vars.IsPlayerFrozen) UpdatePos(); else ResetPos();
            transform.position = Vars.MainCamera.transform.position + LastCamFwd;
        }
    }
}
