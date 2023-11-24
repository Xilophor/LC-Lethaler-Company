using System;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace LethalerComanpany.Patches
{
    [HarmonyPatch]
    public class NoWeatherPatch
    {

        /*
            Disables any weather info in the terminal.
        */
        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        [HarmonyPrefix]
        static void DisableWeatherInfo(Terminal __instance, ref string modifiedDisplayText)
        {
            int num = modifiedDisplayText.Split("[planetTime]", StringSplitOptions.None).Length - 1;
            if (num > 0)
            {
                Regex regex = new(Regex.Escape("[planetTime]"));
                int num2 = 0;
                while (num2 < num && __instance.moonsCatalogueList.Length > num2)
                {
                    string text;
                    if (GameNetworkManager.Instance.isDemo && __instance.moonsCatalogueList[num2].lockedForDemo)
                    {
                        text = "(Locked)";
                    }
                    else
                    {
                        text = "";
                    }
                    modifiedDisplayText = regex.Replace(modifiedDisplayText, text, 1);
                    num2++;
                }
            }
        }

        /*
            Removes any weather info in the Map Info.
        */
        [HarmonyPatch(typeof(StartOfRound), "SetMapScreenInfoToCurrentLevel")]
        [HarmonyPostfix]
        static void RemoveWeatherInfo(StartOfRound __instance)
        {
            string newText = __instance.screenLevelDescription.text;
            newText = newText.Split("Weather:")[0];
            __instance.screenLevelDescription.text = newText;
        }
    }
}