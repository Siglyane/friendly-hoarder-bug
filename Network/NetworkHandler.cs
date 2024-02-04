using FriendlyBug.Data;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace HoarderFriendlyBug.Network
{
    public class NetworkHandler : NetworkBehaviour
    {

        public static NetworkHandler Instance;


        [ServerRpc(RequireOwnership = false)]
        public void UpdateLastPlayerToHeldServerRpc(ulong playerIndex, ushort grabbableObjectId)
        {
            UpdateLastPlayerToHeldClientRpc(playerIndex, grabbableObjectId);
        }


        [ClientRpc]
        public void UpdateLastPlayerToHeldClientRpc(ulong playerIndex, ushort grabbableObjectId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerIndex];
            Debug.Log("Here");
            GrabbableObject grabbableObject = SharedData.Instance.GrabbableObjectId[grabbableObjectId];

            SharedData.Instance.LastPlayerWhoHeld[grabbableObject] = player;
        }
    }
}
