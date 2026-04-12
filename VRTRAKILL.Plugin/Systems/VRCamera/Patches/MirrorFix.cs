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
    //Goes through and updates this section:
    /*
            else
            {
                Vector4 vector3 = transpose2 * vector2;
                projectionMatrix2 = mainCam.CalculateObliqueMatrix(vector3 * -1f);
            }
    */
    // Of portalrenderv2. Needed because the projection matrix isn't symetric for vr, so when they get flipped it looks wrong
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo targetMethod = AccessTools.Method(typeof(Camera), nameof(Camera.CalculateObliqueMatrix));
        MethodInfo replacementMethod = AccessTools.Method(typeof(MirrorFix), nameof(MirrorFix.CorrectedMirrorMatrix));

        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(PortalRenderV2), nameof(PortalRenderV2.mainCam)))
            {
                codes[i - 1].opcode = OpCodes.Nop; // remove ldarg.0
                codes[i].opcode = OpCodes.Ldarg_1; //Use enter cam instead
                codes[i].operand = null;
            }
        }
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == targetMethod)
            {
                codes.Insert(i++, new CodeInstruction(OpCodes.Ldloc_S, 22)); //Get portal object
                codes[i].opcode = OpCodes.Call;
                codes[i].operand = replacementMethod;
            }
        }

        return codes.AsEnumerable();
    }

    public static Matrix4x4 CorrectedMirrorMatrix(Camera enterCam, Vector4 clipPlane, Portal portalObject)
    {
        if (!portalObject.mirror) return enterCam.CalculateObliqueMatrix(clipPlane);

        Matrix4x4 originalProj = enterCam.projectionMatrix;
        Matrix4x4 flippedProj = originalProj;
        flippedProj[0, 2] = -flippedProj[0, 2]; //Inverts the left/right VR skew

        enterCam.projectionMatrix = flippedProj;
        Matrix4x4 correctedObliqueMatrix = enterCam.CalculateObliqueMatrix(clipPlane);
        enterCam.projectionMatrix = originalProj;

        return correctedObliqueMatrix;
    }
}