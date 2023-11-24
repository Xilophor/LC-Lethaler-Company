using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System;
using TMPro;
using MonoMod.Cil;

namespace LethalerComanpany.Patches
{
    [HarmonyPatch(typeof(Turret))]
    public class TurretPatch
    {


        /*
            Increases the Detection Range of Turrets
        */
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void IncreaseDetectionRange(Turret __instance)
        {
            __instance.rotationRange = Mathf.Abs(55f); //Default: 45f
        }

        /*
            Increases the Movement Range of Turrets
        
        [HarmonyPatch("SwitchRotationOnInterval")]
        [HarmonyPostfix]
        static void IncreaseMovementRange(Turret __instance)
        {
            __instance.targetRotation *= movementRangeMultiplier; //Based off of detection range (55f)if (this.rotatingSmoothly)
		
        }*/
        // Have to increase the clamp range as well, otherwise it gets clamped and just stays on the extremes longer
        /*[HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void IncreaseClampRange(Turret __instance, bool ___rotatingSmoothly, bool ___rotatingClockwise)
        {
            if (___rotatingClockwise) return;
            if (___rotatingSmoothly)
			    __instance.turnTowardsObjectCompass.localEulerAngles = new Vector3(-180f, Mathf.Clamp(__instance.targetRotation, -__instance.rotationRange * movementRangeMultiplier, __instance.rotationRange * movementRangeMultiplier), 180f);

            __instance.turretRod.rotation = Quaternion.RotateTowards(__instance.turretRod.rotation, __instance.turnTowardsObjectCompass.rotation, __instance.rotationSpeed * Time.deltaTime);
        }*/

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
            int indexB = -1;
            int returnIndex = -1;

            // Find the Section of IL to Modify
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Ldarg_0 && !foundMassUsageMethod)
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
                //Check for Return code after and if not at end of function (only happens once) 
                else if(codes[i].opcode == OpCodes.Callvirt && codes[i+1].opcode == OpCodes.Ret && codes.Count > i+2)
                {
                    indexB = i+2;

                    for (int j = indexB; j < codes.Count; j++)
                    {
                        //Locate Return Instruction
                        if (codes[j].opcode == OpCodes.Ret)
                        {
                            returnIndex = j;
                            break;
                        }
                    }
                    break;
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

            //Skip the MovementUpdate for Smooth Movement to allow reimplementation for wider clamping
            /*Debug.Log(indexB + ", " + returnIndex);
            if(indexB > -1 && returnIndex > -1)
            {
                Label jumpTarget = ilGenerator.DefineLabel();

                codes[returnIndex] = CodeInstructionExtensions.WithLabels(codes[returnIndex], jumpTarget);

                codes.Insert(indexB, new CodeInstruction(OpCodes.Br,jumpTarget)); //Skip to End CodeInstructionExtensions.WithLabels(new CodeInstruction(OpCodes.Br_S), codes)
            }*/

            return codes.AsEnumerable();
        }
    
    static readonly float movementRangeMultiplier = 1.727f;

    }
}