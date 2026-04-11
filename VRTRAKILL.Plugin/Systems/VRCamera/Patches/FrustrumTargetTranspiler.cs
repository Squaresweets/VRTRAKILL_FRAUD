using HarmonyLib;
using Plugin.Systems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Plugin.Systems.VRCamera.Patches;

[HarmonyPatch]
internal class FrustrumTargetTranspiler
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Railcannon), nameof(Railcannon.GetStuff));
        yield return AccessTools.Method(typeof(ShotgunHammer), nameof(ShotgunHammer.Awake));
        yield return AccessTools.Method(typeof(SecondaryRevolver), nameof(SecondaryRevolver.Start));
    }
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //We want to replace Camera.main.GetComponent<CameraFrustumTargeter>();
        //With CameraFrustrumTarget.Instance
        return new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Call && i.operand is MethodInfo f2 && f2 == AccessTools.PropertyGetter(typeof(Camera), nameof(Camera.main))),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand.ToString().Contains(nameof(CameraFrustumTargeter))))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(CameraFrustumTargeter), nameof(CameraFrustumTargeter.Instance))))
                .InstructionEnumeration();
    }
}
