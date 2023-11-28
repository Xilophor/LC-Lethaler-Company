using HarmonyLib;
using LethalerCompany;
using UnityEngine;
using Unity.Netcode;
using System.Linq.Expressions;

namespace LethalerCompany
{
    [HarmonyPatch]
    public class NetworkGenerator
    {

        /*
            Update Walkspeed Variables on Spawn
        */
        #region Host
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuManager), "StartHosting")]
        static void CacheHostNetworkHandler(ref GameNetworkManager __instance)
        {
            NetworkManager.Singleton.AddNetworkPrefab(cachedNetworkObject);

            Plugin.Instance.mls.LogInfo("Cached host network handler");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuManager), "StartHosting")]
        static void SpawnNetworkHandler(ref GameNetworkManager __instance)
        {
            try
            {
                cachedNetworkObject.GetComponent<NetworkObject>().Spawn();
                Plugin.Instance.mls.LogInfo("Spawned host network handler");
            }
            catch
            {
                Plugin.Instance.mls.LogError("Failed to spawned host network handler");
            }

        }
        #endregion Host

        #region Client
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartClient")]
        static void CacheClientNetworkHandler(ref GameNetworkManager __instance)
        {
            NetworkManager.Singleton.AddNetworkPrefab(cachedNetworkObject);
            GameObject.Destroy(cachedNetworkObject);

            Plugin.Instance.mls.LogInfo("Cached client network handler");
        }
        #endregion Client

        public static GameObject cachedNetworkObject = new("Lethaler-Company_NetworkHandler", typeof(NetworkObject), typeof(NetworkHandler));
    }
}