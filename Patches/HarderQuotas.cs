using HarmonyLib;
using LethalerCompany;

namespace LethalerCompany.Patches
{
    [HarmonyPatch]
    public class HarderQuotasPatch
    {

        /*
            Removes any weather info in the Map Info.
        */
        [HarmonyPatch(typeof(TimeOfDay), "Awake")]
        [HarmonyPostfix]
        private static void ChangeQuotaVariables(ref QuotaSettings ___quotaVariables)
        {
            ___quotaVariables.startingQuota = Plugin.Instance.startingQuota.Value;
            ___quotaVariables.startingCredits = Plugin.Instance.startingCredits.Value;
            ___quotaVariables.deadlineDaysAmount = Plugin.Instance.deadlineDaysAmount.Value;
            ___quotaVariables.increaseSteepness = Plugin.Instance.increaseSteepness.Value;
            ___quotaVariables.baseIncrease = Plugin.Instance.baseIncrease.Value;
            ___quotaVariables.randomizerMultiplier = Plugin.Instance.randomizerMultiplier.Value;
        }
    }
}