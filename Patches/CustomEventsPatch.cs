using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using HarmonyLib;
using LethalerCompany;
using UnityEngine;
using Random = System.Random;


namespace LethalerComanpany.Patches
{
    [HarmonyPatch]
    public class CustomEventsPatch
    {
        public class EventCoroutine: MonoBehaviour { }
        //Variable reference for the class
        private static EventCoroutine evntC;

        private static void Init()
        {
            //If the instance not exit the first time we call the static class
            if (evntC == null)
            {
                //Create an empty object called MyStatic
                GameObject gameObject = new GameObject("CustomEventCoroutine");


                //Add this script to the object
                evntC = gameObject.AddComponent<EventCoroutine>();
            }
        }



        [HarmonyPatch(typeof(RoundManager), "Update")]
        [HarmonyPostfix]
        private static void UpdateEvents(RoundManager __instance)
        {
            if (!__instance.IsServer)
            {
                return;
            }
            if (!__instance.dungeonFinishedGeneratingForAllPlayers)
            {
                return;
            }
            if (EventTimes.Count > currentEventIndex && __instance.timeScript.currentDayTime > (float)occuranceTimes[currentEventIndex])
            {
                Init();

                mls.LogInfo("Starting event");

                switch (EventTimes[occuranceTimes[currentEventIndex]])
                {
                    case EventTypes.FlickerLights:
                        evntC.StartCoroutine(FlickerLights(__instance));
                        break;
                    case EventTypes.PowerOutage:
                        evntC.StartCoroutine(PowerOutage(__instance));
                        break;
                }

			    currentEventIndex++;
            }
        }

        [HarmonyPatch(typeof(RoundManager), "LoadNewLevelWait")]
        [HarmonyPostfix]
        private static void ResetVariables(RoundManager __instance)
        {
            if (!__instance.IsServer) return;

            mls.LogInfo("Resetting Event variables");

            hasFlickered = false;
            outageEvent = false;
            eventRandom = new(StartOfRound.Instance.randomMapSeed + 4);
            occuranceTimes = new List<int>();
            EventTimes = new Dictionary<int,EventTypes>();
            apparatice = UnityEngine.Object.FindObjectOfType<LungProp>();
        }

        /*
            Add Event Checks to Semi-Hourly updates
        */
        [HarmonyPatch(typeof(RoundManager), "AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPostfix]
        private static void CauseEvents(RoundManager __instance, int ___currentHour)
        {
            if (!__instance.IsServer) return;

            if (currentEventIndex < occuranceTimes.Count)
            {
                foreach (var evnt in EventTimes)
                {
                    if (evnt.Key < occuranceTimes[currentEventIndex])
                        EventTimes.Remove(evnt.Key);
                    else
                        break;
                }
                occuranceTimes.RemoveRange(0,currentEventIndex);

                currentEventIndex = 0;
            } else {
                occuranceTimes.Clear();
                EventTimes.Clear();
                currentEventIndex = 0;
            }

            mls.LogInfo("Causing Events");

            float timeOffset = __instance.timeScript.lengthOfHours * (float)___currentHour;

            foreach (var evnt in eventChances)
            {
                if(eventRandom.NextDouble() < evnt.Value)
                {
                    int minAmount = 1;
                    int maxAmount = 1;
                    EventTypes method = EventTypes.None;

                    switch (evnt.Key)
                    {
                        case EventTypes.FlickerLights:
                            if(outageEvent) continue;

                            if (eventRandom.NextDouble() < 0.09d && hasFlickered)
                            {
                                int timeToOccur = eventRandom.Next((int)(2f+timeOffset),(int)(__instance.timeScript.lengthOfHours * (float)__instance.hourTimeBetweenEnemySpawnBatches + timeOffset));
                                EventTimes.Add(timeToOccur, EventTypes.PowerOutage);
                                occuranceTimes.Add(timeToOccur);
                                occuranceTimes.Sort();


                                mls.LogInfo("Event " + EventTypes.PowerOutage + " chosen");
                                continue;
                            } else {
                                maxAmount = 3;
                                method = EventTypes.FlickerLights;
                            }
                            break;
                    }

                    if (method == EventTypes.None) continue;

                    int i;
                    
                    for (i = 0; i < eventRandom.Next(minAmount,maxAmount); i++)
                    {
                        int timeToOccur = eventRandom.Next((int)(5f+timeOffset),(int)(__instance.timeScript.lengthOfHours * (float)__instance.hourTimeBetweenEnemySpawnBatches + timeOffset)-20);
                        EventTimes.Add(timeToOccur, method);
                        occuranceTimes.Add(timeToOccur);
                        mls.LogInfo("Event " + evnt.Key + " chosen to happen at: " + timeToOccur +", hours are "+__instance.timeScript.lengthOfHours+" long");
                    }
                    occuranceTimes.Sort();

                    mls.LogInfo("Event " + evnt.Key + " chosen to happen " + i + " times");
                }
            }
        }

        static IEnumerator PowerOutage(RoundManager __instance)
        {
            mls.LogInfo("Shorting out Lights");

            if (eventRandom.NextDouble() < 0.08)
            {
                mls.LogInfo("Damaging Apparatice");
                __instance.FlickerLights(false, false);
                yield return new WaitForSeconds(1.5f);
                __instance.FlickerLights(false, false);
                yield return new WaitForSeconds(4.5f);
                __instance.SwitchPower(false);
                __instance.powerOffPermanently = true;
                outageEvent = true;
                // TODO: Add explosion when power goes out; make the LungProp worth little
            }
            else
            {
                __instance.FlickerLights(false, false);
                yield return new WaitForSeconds(6.25f);
                __instance.TurnBreakerSwitchesOff();
            }
            yield break;
        }

        static IEnumerator FlickerLights(RoundManager __instance)
        {
            mls.LogInfo("Flickering Lights at "+ __instance.timeScript.currentDayTime);
            __instance.FlickerLights();
            hasFlickered = true;
            yield break;
        }
        
        static IEnumerator BurstPipes(RoundManager __instance)
        {
            mls.LogInfo("Bursting Pipes at "+ __instance.timeScript.currentDayTime);
            yield break;
        }

        static readonly ManualLogSource mls = Plugin.Instance.mls;
        static Random eventRandom;

        static bool hasFlickered = false;
        static bool outageEvent = false;
        static int currentEventIndex;
        static LungProp apparatice;

        static List<int> occuranceTimes;
        static Dictionary<int, EventTypes> EventTimes { get; set; }

        enum EventTypes { None, FlickerLights, PowerOutage, BurstPipes };
        static readonly Dictionary<EventTypes, double> eventChances = new() {
            {EventTypes.FlickerLights, 0.18d}
            }; // Chance out of 1
    }
}