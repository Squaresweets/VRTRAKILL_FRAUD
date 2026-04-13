using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ULTRAKILL.Portal;
using UnityEngine;

namespace Plugin.Systems.VRCamera.Patches;

[HarmonyPatch(typeof(PortalRenderV2), nameof(PortalRenderV2.SetupRenderData))]
public static class MirrorFix
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo targetMethod = AccessTools.Method(typeof(Camera), nameof(Camera.CalculateObliqueMatrix));
        MethodInfo replacementMethod = AccessTools.Method(typeof(MirrorFix), nameof(MirrorFix.CorrectedMirrorMatrix));

        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(PortalRenderV2), nameof(PortalRenderV2.defaultProjectionMatrix)))
            {
                codes[i - 1] = new CodeInstruction(OpCodes.Nop);
                codes[i] = new CodeInstruction(OpCodes.Ldloc_S, 9); //Get "Projection matrix" instead lol
            }
            // Find the call to CalculateObliqueMatrix
            if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == targetMethod)
            {
                // Stack contains: [Camera mainCam, Vector4 clipPlane]

                // 1. Push the Portal object (Local 22)
                codes.Insert(i++, new CodeInstruction(OpCodes.Ldloc_S, 22));

                // 2. Push inMirroredSpace. 
                // CRITICAL: If mirrors are broken, change this 7 to an 8!
                codes.Insert(i++, new CodeInstruction(OpCodes.Ldarg_S, 7));

                // 3. Swap the callvirt to our static method
                codes[i].opcode = OpCodes.Call;
                codes[i].operand = replacementMethod;
            }
        }
        return codes.AsEnumerable();
    }

    public static Matrix4x4 CorrectedMirrorMatrix(Camera mainCam, Vector4 clipPlane, Portal portalObject, bool inMirroredSpace)
    {
        // If the XOR result is True, the space we are entering is physically mirrored.
        // Logic: (Not Mirrored + Entering Mirror = True) | (Mirrored + Entering Portal = True)
        // (Mirrored + Entering Mirror = False) <- Reflections cancel out!
        bool destinationIsMirrored = inMirroredSpace ^ portalObject.mirror;

        // Use the base projection matrix of the camera.
        // DO NOT use Camera.projectionMatrix if you have already modified it this frame; 
        // Unity's CalculateObliqueMatrix needs the 'clean' eye frustum.
        Matrix4x4 baseProj = mainCam.projectionMatrix;

        if (!destinationIsMirrored)
        {
            return mainCam.CalculateObliqueMatrix(clipPlane);
        }

        // --- VR SKEW FLIP ---
        // We flip the horizontal asymmetry [0, 2] to match the reflected eye perspective.
        Matrix4x4 flippedProj = baseProj;
        flippedProj[0, 2] = -flippedProj[0, 2];

        // Swap, calculate, and restore. 
        // This ensures we don't 'leak' the flipped projection into other portal calculations.
        mainCam.projectionMatrix = flippedProj;
        Matrix4x4 correctedObliqueMatrix = mainCam.CalculateObliqueMatrix(clipPlane);
        mainCam.projectionMatrix = baseProj;

        return correctedObliqueMatrix;
    }
}