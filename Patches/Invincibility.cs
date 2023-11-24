using HarmonyLib;
using GameNetcodeStuff;

namespace LethalerComanpany.Patches
{
    [HarmonyPatch]
    public class InvincibilityPatch
    {

        /*
            Removes any weather info in the Map Info.
        */
        [HarmonyPatch(typeof(PlayerControllerB), "AllowPlayerDeath")]
        [HarmonyPostfix]
        static void DisableDeath(PlayerControllerB __instance, ref bool __result)
        {
            __result = false;
        }
    }
}