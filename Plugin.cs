using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalerComanpany.Patches;

namespace LethalerCompany
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            
            LoadConfig();

            mls = BepInEx.Logging.Logger.CreateLogSource(pluginGUID);

            // Individually Patch to Allow De-Conflicting with other Patches
            if(WeatherDisabled.Value) harmony.PatchAll(typeof(NoWeatherPatch));
            if(Invincibility.Value && pluginVersion.Split('.')[0] != "1") harmony.PatchAll(typeof(InvincibilityPatch)); // For Testing Only
            harmony.PatchAll(typeof(TurretPatch));
            harmony.PatchAll(typeof(HarderQuotasPatch));
            harmony.PatchAll(typeof(DunGenPatch));
            harmony.PatchAll(typeof(SpringManPatch));
            harmony.PatchAll(typeof(EnemySpawnPatch));

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


            WeatherDisabled = Config.Bind("Other",
                                                "NoWeatherInfo",
                                                true,
                                                "Disables weather info in Moons list and on the Map Screen. Change this if you want weather info.");
            
            

            
            if(pluginVersion.Split('.')[0] != "1") 
                Invincibility = Config.Bind("Other",
                                                "Invincibility",
                                                false,
                                                "");
        }

        private const string pluginGUID = "Xilophor.LethalerCompany";
        private const string pluginName = "LethalerCompany";
        private const string pluginVersion = "0.2.0";

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
        private ConfigEntry<bool> Invincibility;
    }
}
