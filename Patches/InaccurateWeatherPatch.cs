using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using HarmonyLib;
using LethalerCompany;

namespace LethalerCompany.Patches
{
    [HarmonyPatch]
    public class InaccurateWeatherPatch
    {

        /*
            Generates the weather forecasts for every moon and stores the forecasts.
            Does not need to be saved in a file - seed is based off of mapSeed which
            is saved by the game.
        */
        [HarmonyPatch(typeof(StartOfRound), "SetPlanetsWeather")]
        [HarmonyPostfix]
        static void GenerateWeatherInfo(StartOfRound __instance)
        {
            mls.LogInfo("Generating Weather Focasts");

            WeatherForecast.Clear();

            Random random = new(__instance.randomMapSeed + 35);

            List<SelectableLevel> levels = __instance.levels.ToList<SelectableLevel>();

            foreach (var level in levels)
            {
                LevelWeatherType forecastedWeather;
                if (random.NextDouble() < (1 - (double)(Plugin.Instance.WeatherAccuracyRate.Value/100))) {
                    List<RandomWeatherWithVariables> allowableWeathers = new() { };

                    foreach (var weather in level.randomWeathers)
                    {
                        if (weather.weatherType == level.currentWeather)
                            continue;

                        allowableWeathers.Add(weather);
                    }

                    if(allowableWeathers.Count == 0)
                    {
                        mls.LogWarning("No available weathers allowed to be selected for " + level.PlanetName + ". Chosen " + level.currentWeather.ToString());
                        forecastedWeather = level.currentWeather;
                    }
                    else
                    {
                        forecastedWeather = allowableWeathers[random.Next(0, allowableWeathers.Count)].weatherType;
                    }
                }
                else
                {
                    forecastedWeather = level.currentWeather;
                }

                WeatherForecast[level] = forecastedWeather;

                mls.LogDebug(level.PlanetName + ": " + WeatherForecast[level].ToString() + ", a:" + level.currentWeather.ToString());
            }

            UpdateScreenWeatherInfo(__instance);
        }

        /*
            Displays forecasts in the terminal when browsing moon list.
        */
        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        [HarmonyPrefix]
        static void ChangeWeatherInfo(Terminal __instance, ref string modifiedDisplayText)
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
                    else if (WeatherForecast[__instance.moonsCatalogueList[num2]] == LevelWeatherType.None)
                    {
                        text = "";
                    }
                    else
                    {
                        text = "(" + WeatherForecast[__instance.moonsCatalogueList[num2]].ToString() + ")";
                    }
                    modifiedDisplayText = regex.Replace(modifiedDisplayText, text, 1);
                    num2++;
                }
            }
        }

        /*
            Updates the Screen's weather info with the forecast.
        */
        [HarmonyPatch(typeof(StartOfRound), "SetMapScreenInfoToCurrentLevel")]
        [HarmonyPostfix]
        static void UpdateScreenWeatherInfo(StartOfRound __instance)
        {
            mls.LogDebug("Updating Map Screen");

            if (WeatherForecast.Count == 0)
            {
                mls.LogDebug("Unable to update screen - no weather info available"); // Updating the Map Screen happens before setting planet info, 'cause who cares about giving correct map info ;-;
                return;
            }

            string text = __instance.screenLevelDescription.text;
            text = text.Split("Weather:")[0];

            if (WeatherForecast[__instance.currentLevel] != LevelWeatherType.None)
            {
                mls.LogInfo("Moon has forecasted weather");

                text = text + "Weather: " + WeatherForecast[__instance.currentLevel].ToString();
            }
            __instance.screenLevelDescription.text = text;
        }

        static readonly ManualLogSource mls = Plugin.Instance.mls;
        static Dictionary<SelectableLevel, LevelWeatherType> WeatherForecast { get; set; } = new Dictionary<SelectableLevel, LevelWeatherType>(){};
    }
}