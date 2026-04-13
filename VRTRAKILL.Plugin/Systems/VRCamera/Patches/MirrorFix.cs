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
    static void Prefix(PortalRenderV2 __instance, Camera enterCam)
    {
        __instance.defaultProjectionMatrix = enterCam.projectionMatrix;
    }
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo targetMethod = AccessTools.Method(typeof(Camera), nameof(Camera.CalculateObliqueMatrix));
        MethodInfo replacementMethod = AccessTools.Method(typeof(MirrorFix), nameof(MirrorFix.CorrectedMirrorMatrix));

        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == targetMethod)
            {
                codes.Insert(i++, new CodeInstruction(OpCodes.Ldloc_S, 22)); //Get portal object
                codes.Insert(i++, new CodeInstruction(OpCodes.Ldarg_S, 7)); //Get inMirroredSpace

                codes[i].opcode = OpCodes.Call;
                codes[i].operand = replacementMethod;
            }
        }
        return codes.AsEnumerable();
    }

    public static Matrix4x4 CorrectedMirrorMatrix(Camera mainCam, Vector4 clipPlane, Portal portalObject, bool inMirroredSpace)
    {
        bool destinationIsMirrored = inMirroredSpace ^ portalObject.mirror;
        Matrix4x4 baseProj = mainCam.projectionMatrix;

        if (!destinationIsMirrored)
            return mainCam.CalculateObliqueMatrix(clipPlane);

        Matrix4x4 flippedProj = baseProj;
        flippedProj[0, 2] = -flippedProj[0, 2];

        mainCam.projectionMatrix = flippedProj;
        Matrix4x4 correctedObliqueMatrix = mainCam.CalculateObliqueMatrix(clipPlane);
        mainCam.projectionMatrix = baseProj;

        return correctedObliqueMatrix;
    }
}