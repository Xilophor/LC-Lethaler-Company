using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;


namespace LethalerComanpany.Patches
{
    [HarmonyPatch]
    public class DunGenPatch
    {

        /*
            Removes any weather info in the Map Info.
        */
        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPrefix]
        private static bool ChangeQuotaVariables(RoundManager __instance)
        {
            if (!__instance.IsHost) return true;

            Debug.Log("SpawnableMapObjects:");
            Debug.Log(__instance.currentLevel.name);
            foreach (var spawnableObject in __instance.currentLevel.spawnableMapObjects)
            {
                spawnableObject.numberToSpawn.keys = MapObjectCurves[__instance.currentLevel.name][spawnableObject.prefabToSpawn.name];

                var text = "";
                for (var i = 0; i < spawnableObject.numberToSpawn.keys.Length; i++) 
                    text += " " + spawnableObject.numberToSpawn.keys[i].time + "_" + spawnableObject.numberToSpawn.keys[i].value;
                Debug.Log(spawnableObject.prefabToSpawn.name + ";" + text);
            }

            return true;
        }

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