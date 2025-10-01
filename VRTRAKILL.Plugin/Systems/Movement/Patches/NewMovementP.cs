﻿using HarmonyLib;
using ULTRAKILL.Cheats;
using UnityEngine;
using Plugin.Systems.Input;

namespace Plugin.Systems.Movement.Patches
{
    [HarmonyPatch(typeof(NewMovement))]
    internal sealed class NewMovementP
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(NewMovement.Start))]
        static void Start(NewMovement __instance)
        {
            __instance.walkSpeed *= Vars.Config.MovementMultiplier;
            __instance.jumpPower *= Vars.Config.MovementMultiplier;
            __instance.wallJumpPower *= Vars.Config.MovementMultiplier;
        }
    }
}
