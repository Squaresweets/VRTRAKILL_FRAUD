using HarmonyLib;
using Plugin.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Plugin.Patches.ULTRAKILL;

[HarmonyPatch]
internal class PunchTranspiler
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Punch), nameof(Punch.ActiveFrame));
    }
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //Convert this.cc.GetDefaultPos() to Vars.NDHC.transform.position
        IEnumerable<CodeInstruction> sequence = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f.FieldType == typeof(CameraController)),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == AccessTools.Method(typeof(CameraController), nameof(CameraController.GetDefaultPos))))
                .Repeat(matcher => {
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Vars), "NDHC")));
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform))));
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.position))));
                })
                .InstructionEnumeration();
        //this.camObj.transform.forward -> PatchPunch.Direction
        sequence = new CodeMatcher(sequence)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f.Name == "camObj"),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == AccessTools.PropertyGetter(typeof(GameObject), nameof(GameObject.transform))),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.forward))))
                .Repeat(matcher => {
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(PatchPunch), "Direction")));
                    matcher.RemoveInstruction();
                    matcher.RemoveInstruction();
                    matcher.RemoveInstruction();
                })
                .InstructionEnumeration();
        //this.camObj.transform.rotation -> Vars.NDHC.transform.position
        sequence = new CodeMatcher(sequence)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f.Name == "camObj"),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == AccessTools.PropertyGetter(typeof(GameObject), nameof(GameObject.transform))),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m == AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.rotation))))
                .Repeat(matcher => {
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Vars), "NDHC")));
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform))));
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.rotation))));
                })
                .InstructionEnumeration();
        return sequence;
    }
}
