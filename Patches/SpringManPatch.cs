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
            float multiplier = (float)(System.Math.Round(Random.Range(targetChaseSpeed - 0.75f,targetChaseSpeed + 0.5f)*10f)/10f /14.5f); // Random Target Speed / Normal Speed (14.5f)

            ___currentChaseSpeed *= multiplier; // Normally 14.5f
            ___currentAnimSpeed *= multiplier; // Normally 1f

            Plugin.Instance.mls.LogDebug("Spawned SpringMan-" + __instance.enemyType.enemyName + " with chase speed " + 14.5f * multiplier);
        }

        static readonly float targetChaseSpeed = 16.25f;
    }
}