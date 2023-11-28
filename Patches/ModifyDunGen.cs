using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using LethalerCompany;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Random = System.Random;


namespace LethalerCompany.Patches
{
    [HarmonyPatch]
    public class DunGenPatch
    {

        /*
            Changes map object spawn curves.
        */
        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPrefix]
        private static bool ChangeQuotaVariables(RoundManager __instance)
        {
            if (!__instance.IsHost) return true;

            Plugin.Instance.mls.LogDebug("SpawnableMapObjects:");
            Plugin.Instance.mls.LogDebug(__instance.currentLevel.name);
            foreach (var spawnableObject in __instance.currentLevel.spawnableMapObjects)
            {
                spawnableObject.numberToSpawn.keys = MapObjectCurves[__instance.currentLevel.name][spawnableObject.prefabToSpawn.name];

                var text = "";
                for (var i = 0; i < spawnableObject.numberToSpawn.keys.Length; i++) 
                    text += " " + spawnableObject.numberToSpawn.keys[i].time + "_" + spawnableObject.numberToSpawn.keys[i].value;
                mls.LogDebug(spawnableObject.prefabToSpawn.name + ";" + text);
            }

            __instance.scrapValueMultiplier *= 1.15f;
            __instance.scrapAmountMultiplier *= 1.3f;
            __instance.mapSizeMultiplier *= 1.1f;

            mls.LogDebug(string.Format("Changed multipliers to scrapVal: {0}x, scrapAmnt: {1}x, mapSize: {2}x", __instance.scrapValueMultiplier, __instance.scrapAmountMultiplier, __instance.mapSizeMultiplier));

            return true;
        }

        // Add (more) Quicksand patches at start of round for rainy and stormy weather
        [HarmonyPatch(typeof(RoundManager), "SpawnOutsideHazards")]
        [HarmonyPostfix]
        private static void CustomEventUpdate(RoundManager __instance)
        {
		    NavMeshHit navMeshHit = default;
		    Random random = new(StartOfRound.Instance.randomMapSeed + 3);
            if (TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Rainy)
            {
                int num = random.Next(3, 10);
                if (random.Next(0, 100) < 5)
                {
                    num = random.Next(3, 20);
                }
                for (int i = 0; i < num; i++)
                {
                    Vector3 vector = __instance.GetRandomNavMeshPositionInBoxPredictable(__instance.outsideAINodes[random.Next(0, __instance.outsideAINodes.Length)].transform.position, 30f, navMeshHit, random, -1) + Vector3.up;
                    Object.Instantiate<GameObject>(__instance.quicksandPrefab, vector, Quaternion.identity, __instance.mapPropsContainer.transform);
                }

                mls.LogInfo("Generated " + num + " extra quicksand pools");
            }
            if (TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Stormy)
            {
                int num = random.Next(0, 6);
                if (random.Next(0, 100) < 10)
                {
                    num = random.Next(2, 13);
                }
                for (int i = 0; i < num; i++)
                {
                    Vector3 vector = __instance.GetRandomNavMeshPositionInBoxPredictable(__instance.outsideAINodes[random.Next(0, __instance.outsideAINodes.Length)].transform.position, 30f, navMeshHit, random, -1) + Vector3.up;
                    Object.Instantiate<GameObject>(__instance.quicksandPrefab, vector, Quaternion.identity, __instance.mapPropsContainer.transform);
                }

                mls.LogInfo("Generated " + num + " quicksand pools");
            }
        }

        static readonly ManualLogSource mls = Plugin.Instance.mls;

        readonly static Dictionary<string, Keyframe[]> ExperimentationCurves = new(){{"Landmine", new Keyframe[]{new(0f,2f), new(.8f, 17f), new(1f, 25f)}},{"TurretContainer", new Keyframe[]{new(0f,0f), new(.2f, 1f), new(.9f, 3f), new(1f, 10f)}}};
        readonly static Dictionary<string, Keyframe[]> AssuranceCurves = new(){{"Landmine", new Keyframe[]{new(0f,4f), new(.8f, 9f), new(1f, 19f)}},{"TurretContainer", new Keyframe[]{new(0f,1f), new(.2f, 4f), new(.9f, 15f), new(1f, 20f)}}};
        readonly static Dictionary<string, Keyframe[]> VowCurves = new(){{"Landmine", new Keyframe[]{new(0f,5f), new(.9f, 12f), new(1f, 23f)}},{"TurretContainer", new Keyframe[]{new(0f,2f), new(.2f, 6f), new(.9f, 19f), new(1f, 25f)}}};
        readonly static Dictionary<string, Keyframe[]> OffenseCurves = new(){{"Landmine", new Keyframe[]{new(0f,5f), new(.8f, 22f), new(1f, 36f)}},{"TurretContainer", new Keyframe[]{new(0f,0f), new(.2f, 3f), new(.9f, 9f), new(1f, 13f)}}};
        readonly static Dictionary<string, Keyframe[]> MarchCurves = new(){{"Landmine", new Keyframe[]{new(0f,5f), new(.8f, 20f), new(1f, 32f)}},{"TurretContainer", new Keyframe[]{new(0f,2f), new(.2f, 6f), new(.9f, 16f), new(1f, 22f)}}};
        readonly static Dictionary<string, Keyframe[]> RendCurves = new(){{"Landmine", new Keyframe[]{new(0f,1f), new(.8f, 9f), new(1f, 13f)}}}; //,{"TurretContainer", new Keyframe[]{new(0f,0f), new(.2f, 1f), new(.9f, 6f), new(1f, 15f)}}};
        readonly static Dictionary<string, Keyframe[]> DineCurves = new(){{"Landmine", new Keyframe[]{new(0f,11f), new(.2f, 16f), new(.8f, 32f), new(1f, 45f)}},{"TurretContainer", new Keyframe[]{new(0f,3f), new(.2f, 7f), new(.8f, 16f), new(1f, 24f)}}};
        readonly static Dictionary<string, Keyframe[]> TitanCurves = new(){}; //new(){{"Landmine", new Keyframe[]{new(0f,5f), new(.8f, 22f), new(1f, 32f)}},{"TurretContainer", new Keyframe[]{new(0f,0f), new(.2f, 1f), new(.9f, 6f), new(1f, 15f)}}};

        readonly static Dictionary<string, Dictionary<string, Keyframe[]>> MapObjectCurves = new(){{"ExperimentationLevel", ExperimentationCurves}, {"AssuranceLevel", AssuranceCurves}, {"VowLevel", VowCurves}, {"OffenseLevel", OffenseCurves}, {"MarchLevel", MarchCurves}, {"RendLevel", RendCurves}, {"DineLevel", DineCurves}, {"TitanLevel", TitanCurves}};
    }
}