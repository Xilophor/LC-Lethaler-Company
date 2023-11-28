using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System;
using TMPro;
using MonoMod.Cil;

namespace LethalerCompany.Patches
{
    [HarmonyPatch(typeof(Turret))]
    public class TurretPatch
    {


        /*
            Increases the Detection/Movement Range of Turrets
        */
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void IncreaseDetectionRange(Turret __instance)
        {
            __instance.rotationRange = Mathf.Abs(55f); //Default: 45f
        }

        /*
            Decreases the Charging time to fire quicker.
        */
        [HarmonyPatch("Update")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DecreaseCharingTime(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            bool foundMassUsageMethod = false;
            int startIndexA = -1;
            int endIndexA = -1;

            // Find the Section of IL to Modify
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (foundMassUsageMethod) break;
                if(codes[i].opcode == OpCodes.Ldarg_0)
                {
                    startIndexA = i+1;

                    for (int j = startIndexA; j < codes.Count; j++)
                    {
                        if (codes[j].opcode == OpCodes.Call) break;
                        string strOperand = codes[j].operand as string;
                        //Reference Operand
                        if (strOperand == "Charging timer is up, setting to firing mode")
                        {
                            foundMassUsageMethod = true;
                            endIndexA = j;
                            break;
                        }
                    }
                }
            }

            //Modify the Charging Time
            if(startIndexA > -1 && endIndexA > -1)
            {
                for (int i = startIndexA; i < endIndexA; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_R4 && CodeInstructionExtensions.OperandIs(codes[i], 1.5f))
                    {
                        codes[i] = new CodeInstruction(OpCodes.Ldc_R4, 1.15f); //Sets the charge time to 1.15 seconds
                        break;
                    }
                }
            }

            return codes.AsEnumerable();
        }

    }
}