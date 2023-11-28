using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using LethalerCompany.Patches;

namespace LethalerCompany
{
    public class NetworkHandler : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            // if (!IsServer || !IsHost && IsOwner) {
            // }
            if (Instance == null) Instance = this;

            if(!IsHost) {
                NetworkGenerator.cachedNetworkObject = gameObject;
                Plugin.Instance.mls.LogInfo("Spawned Client Network Handler");
            }

            this.OnNetworkSpawn();
        }

        [ClientRpc]
        public void EventClientRPC(CustomEventsPatch.EventTypes eventType)
        {
            CustomEventsPatch.ReceivedEventsFromServer(eventType);
        }

        public static NetworkHandler Instance;
    }
}