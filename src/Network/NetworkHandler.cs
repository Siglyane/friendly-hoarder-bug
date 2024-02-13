using FriendlyHoarderBug.src.Components;
using GameNetcodeStuff;
using Unity.Netcode;

namespace FriendlyHoarderBug.src.Network
{
    public class NetworkHandler : NetworkBehaviour
    {

        [ServerRpc(RequireOwnership = false)]
        public void UpdateLastPlayerToHeldServerRpc(ulong playerIndex, ushort grabbableObjectId)
        {
            UpdateLastPlayerToHeldClientRpc(playerIndex, grabbableObjectId);
        }


        [ClientRpc]
        public void UpdateLastPlayerToHeldClientRpc(ulong playerIndex, ushort grabbableObjectId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerIndex];
            GrabbableObject grabbableObject = SharedData.Instance.GrabbableObjectId[grabbableObjectId];

            SharedData.Instance.LastPlayerWhoHeld[grabbableObject] = player;
        }
    }
}
