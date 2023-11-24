using System;
using System.Collections.Generic;
using HarmonyLib;

namespace LethalerComanpany.Patches
{
    [HarmonyPatch(typeof(CentipedeAI))]
    public class CentipedePatch
    {
        // Make Centipede stay on player on teleport and mark that it is outside (for dying if out too long and OnboardShipAI)
        [HarmonyPatch("OnPlayerTeleport")]
        [HarmonyPrefix]
        static bool OnPlayerTeleport(CentipedeAI __instance)
        {
            centipedeAIs[__instance]["isOutside"] = !(bool)(centipedeAIs[__instance]["isOutside"] ?? false);
            return false; //Skip Function
        }

        [HarmonyPatch(typeof(EnemyAI), "KillEnemyOnOwnerClient")]
        [HarmonyPrefix]
        static bool IgnoreKill(CentipedeAI __instance)
        {
            if (__instance.enemyType.name == "Centepede" && !__instance.clingingToPlayer.isInsideFactory && !__instance.clingingToPlayer.isPlayerDead)
                return false;
            return true;
        }

        static Dictionary<CentipedeAI, Dictionary<string, object>> centipedeAIs = new(){};
    }
}