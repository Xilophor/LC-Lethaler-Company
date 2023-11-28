using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalerCompany;
using LethalerCompany.Patches;
using UnityEngine;

namespace LethalerCompany
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {

        private void NetcodeWeaver()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            //NetworkGenerator.Init();
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            
            LoadConfig();

            mls = BepInEx.Logging.Logger.CreateLogSource(pluginGUID);


            // Individually Patch to Allow De-Conflicting with other Patches
            if(WeatherDisabled.Value) harmony.PatchAll(typeof(NoWeatherPatch));
            if(WeatherInaccurate.Value && !WeatherDisabled.Value) harmony.PatchAll(typeof(InaccurateWeatherPatch));
            harmony.PatchAll(typeof(TurretPatch));
            harmony.PatchAll(typeof(HarderQuotasPatch));
            harmony.PatchAll(typeof(DunGenPatch));
            harmony.PatchAll(typeof(SpringManPatch));
            harmony.PatchAll(typeof(EnemySpawnPatch));
            harmony.PatchAll(typeof(CustomEventsPatch));

            // Initialize Patch for Mod Network Handler
            harmony.PatchAll(typeof(NetworkGenerator));

            NetcodeWeaver(); //Initialize NetworkPatch

            mls.LogInfo($"{pluginGUID} loaded");
        }

        private void LoadConfig()
        {
            startingQuota = Config.Bind("​Quota",
                                                "startingQuota",
                                                365);
            startingCredits = Config.Bind("​Quota",
                                                "startingCredits",
                                                85);
            deadlineDaysAmount = Config.Bind("​Quota",
                                                "deadlineDaysAmount",
                                                5);
            increaseSteepness = Config.Bind("​Quota",
                                                "increaseSteepness",
                                                12f);
            baseIncrease = Config.Bind("​Quota",
                                                "baseIncrease",
                                                225);
            randomizerMultiplier = Config.Bind("​Quota",
                                                "randomizerMultiplier",
                                                1.15f);


            WeatherDisabled = Config.Bind("Weather",
                                                "No Weather Info",
                                                false,
                                                "Disables weather info in Moons list and on the Map Screen. Change this if you want to hide weather info. Will override inaccurate weather.");
            
            WeatherInaccurate = Config.Bind("Weather",
                                                "Inaccurate Weather Forecast",
                                                true,
                                                "Weather forecasting isn't always accurate - the company will sometimes report incorrect weather information.");
            WeatherAccuracyRate = Config.Bind("Weather",
                                                "Forecast Accuracy Percentage",
                                                72f,
                                                "This decides how accurate the company will be at forecasting the weather.");

            
            /*if(pluginVersion.Split('.')[0] != "1") 
                Invincibility = Config.Bind("Other",
                                                "Invincibility",
                                                false,
                                                "");*/
        }

        private const string pluginGUID = "Xilophor.LethalerCompany";
        private const string pluginName = "LethalerCompany";
        private const string pluginVersion = "0.4.0";

        private static readonly Harmony harmony = new(pluginGUID);

        public static Plugin Instance;

        public ManualLogSource mls;

        public ConfigEntry<int> startingQuota;
        public ConfigEntry<int> startingCredits;
        public ConfigEntry<int> deadlineDaysAmount;
        public ConfigEntry<float> increaseSteepness;
        public ConfigEntry<int> baseIncrease;
        public ConfigEntry<float> randomizerMultiplier;

        private ConfigEntry<bool> WeatherDisabled;
        private ConfigEntry<bool> WeatherInaccurate;
        public ConfigEntry<float> WeatherAccuracyRate;
        //private ConfigEntry<bool> Invincibility;
    }
}
