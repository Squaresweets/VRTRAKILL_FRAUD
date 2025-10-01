using VRTRAKILL.Utilities;
using UnityEngine;
using Plugin.Systems.Controllers;

namespace Plugin.Systems.Arms;

internal class VRArmController : VRArmControllerBase
{
    public Transform RawTarget { get; set; }
    public override void Start()
    {
        base.Start();

        if (Vars.Config.Controllers.LeftHanded || gameObject.HasComponent<Sandbox.Arm.SandboxArm>())
        { Target = Vars.DominantHand.transform; RawTarget = Vars.NDHC.transform; }
        else { Target = Vars.NonDominantHand.transform; RawTarget = Vars.DHC.transform; }
    }

    public override void LateUpdate()
    {
        if (Vars.IsMainMenu || Arm == null) return;

        if (!gameObject.HasComponent<HookArm>()) base.LateUpdate();
        Arm.GameObject.position = Target.position;
        Arm.GameObject.rotation = Target.rotation;

        Arm.Root.localPosition = OffsetPos;
        Arm.Hand.Root.rotation = Target.rotation * Quaternion.Euler(OffsetRot);

        if (Arm.Type == VRAvatar.Armature.ArmType.Feedbacker)
            Arm.Hand.Root.rotation *= Quaternion.Euler(0, 90, 0);
    }
}
