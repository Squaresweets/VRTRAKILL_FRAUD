using HarmonyLib;
using Plugin.Systems.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Plugin.Systems.Movement.Patches;
[HarmonyPatch]
internal class MovementTranspiler
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(PlatformerMovement), nameof(PlatformerMovement.Update));
        yield return AccessTools.Method(typeof(ClimbStep), nameof(ClimbStep.FixedUpdate));
        yield return AccessTools.Method(typeof(Flight), nameof(Flight.Update));
        yield return AccessTools.Method(typeof(Noclip), nameof(Noclip.UpdateTick));
        yield return AccessTools.Method(typeof(Grenade), nameof(Grenade.LateUpdate));
        yield return AccessTools.Method(typeof(NewMovement), nameof(NewMovement.Update));
        yield return AccessTools.Method(typeof(NewMovement), nameof(NewMovement.Dodge));
    }
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f == AccessTools.Field(typeof(PlayerInput), "Move")),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand is MethodInfo m && m.Name == "ReadValue"))
                .Repeat(matcher => 
                    matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MovementTranspiler), nameof(GetReplacementMove))))
                    .RemoveInstruction())
                .InstructionEnumeration();
    }
    static Vector2 GetReplacementMove(PlayerInput input) => InputVars.MoveVector;
}
