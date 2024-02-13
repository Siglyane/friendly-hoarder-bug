using FriendlyHoarderBug.src.Components;
using GameNetcodeStuff;
using Unity.Netcode;

namespace FriendlyHoarderBug.src.Network
{
    internal class HoarderbugNetworkHandler : NetworkBehaviour
    {
        [ServerRpc(RequireOwnership = false)]
        public void UpdateHBServerRpc(ulong hoar, ushort grabbableObjectId, ulong playerId)
        {
            UpdateHBClientRpc(hoar, grabbableObjectId, playerId);
        }


        [ClientRpc]
        public void UpdateHBClientRpc(ulong hoar, ushort grabbableObjectId, ulong playerId)
        {
            HoarderBugAI hoarderBugAI = SharedData.Instance.HoarderBugId[hoar];
            GrabbableObject grabbableObject = SharedData.Instance.GrabbableObjectId[grabbableObjectId];
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];

            SharedData.Instance.isHoarderBugFriendly[player] = true;
            SharedData.Instance.HoarderBugBestFriend[hoarderBugAI] = player;
        }
    }
}
