using UnityEngine;
using System.Collections;
using Plugin.Systems.Input;
using Valve.VR.InteractionSystem;
using Valve.VR;

namespace Plugin.Systems.VRCamera
{
    internal class VRCameraTurning : MonoSingleton<VRCameraTurning>
    {
        private bool IsTurning; private float SnapTurnTimer;
        public void Update()
        {
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