using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LethalerCompany;
using UnityEngine;

namespace LethalerCompany.Patches
{
    [HarmonyPatch]
    public class EnemySpawnPatch
    {

        /*
            I cannot increase the rate of spawn batches; the update is in int hours only,
            so unless I want to halve/increase the time between batches, I cannot do it.
            Instead, I will increase the amount of enemies in the spawn batches. This will
            result in a similar effect to increasing the rate of spawn batches - except
            I can more specifically control the increased batch sizes (i.e. 1.2x size).
        */
        [HarmonyPatch(typeof(RoundManager), "PlotOutEnemiesForNextHour")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> IncreasedSpawnBatchSize(IEnumerable<CodeInstruction> instructions)
        {
            int divIndex = -1;

            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count(); i++)
            {
                if (inst[i].opcode == OpCodes.Div && inst[i+1].opcode == OpCodes.Callvirt)
                {
                    divIndex = i;
                    break;
                } 
            }

            if (divIndex > -1)
            {
                CodeInstruction mulInst = new(OpCodes.Mul);
                CodeInstruction valInst = new(OpCodes.Ldc_R4, (float) increasedSpawnsMultiplier);

                inst.InsertRange(divIndex + 1, new List<CodeInstruction> { valInst, mulInst });
                
                Plugin.Instance.mls.LogDebug("Altered indoor enemy spawn batch size by " + increasedSpawnsMultiplier);
            }
            else Plugin.Instance.mls.LogWarning("Unable to alter indoor enemy spawn batch size");

            return inst.AsEnumerable();
        }

        // Changes the Spawning start time from 85f to that of new start time. In Vanilla, each hour is 100f.
        [HarmonyPatch(typeof(RoundManager), "Update")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EarlierSpawnTime(IEnumerable<CodeInstruction> instructions)
        {
            int floatIndex = -1;

            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count(); i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4) // Search for float that is used - Currently there is only one specified float in the method
                {
                    floatIndex = i;
                    break;
                } 
            }

            if (floatIndex > -1)
            {
                inst[floatIndex].operand = (float) newStartTime; // Change the start time float to the new one
                
                Plugin.Instance.mls.LogDebug("Altered enemy spawn start time to " + newStartTime);
            }
            else Plugin.Instance.mls.LogWarning("Unable to alter enemy spawn start time");

            return inst.AsEnumerable();
        }

        static readonly float increasedSpawnsMultiplier = 1.2f;

        static readonly float newStartTime = 15f;
    }
}