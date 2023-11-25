using System;
using HarmonyLib;
using LethalerCompany;
using UnityEngine;

namespace LethalerComanpany.Patches
{
    [HarmonyPatch]
    public class SpringManPatch
    {

        /*
            Update Walkspeed Variables on Spawn
        */
        [HarmonyPatch(typeof(SpringManAI), MethodType.Constructor)]
        [HarmonyPostfix]
        static void UpdateVariables(SpringManAI __instance, ref float ___currentChaseSpeed, ref float ___currentAnimSpeed)
        {
            float multiplier = targetChaseSpeed/14.5f; // Target Speed / Normal Speed (14.5f)

            ___currentChaseSpeed *= multiplier; // Normally 14.5f
            ___currentAnimSpeed *= multiplier; // Normally 1f
        }

        static readonly float targetChaseSpeed = 16.25f;
    }
}