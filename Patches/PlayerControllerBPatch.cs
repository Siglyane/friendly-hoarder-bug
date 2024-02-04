using FriendlyBug.Data;
using GameNetcodeStuff;
using HarmonyLib;
using HoarderFriendlyBug.Network;

namespace FriendlyBug.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("DiscardHeldObject")]
        [HarmonyPrefix]
        private static void DiscardHeldObjectPatch(PlayerControllerB __instance)
        {
            if (__instance.currentlyHeldObjectServer != null)
            {
                SharedData.Instance.LastPlayerWhoHeld[__instance.currentlyHeldObjectServer] = __instance;
                __instance.gameObject.GetComponent<NetworkHandler>().UpdateLastPlayerToHeldServerRpc(__instance.playerClientId, __instance.currentlyHeldObjectServer.NetworkBehaviourId);
            }
        }

        [HarmonyPatch("GrabObjectClientRpc")]
        [HarmonyPostfix]
        private static void GrabObjectClientRpcPatch(PlayerControllerB __instance)
        {
            if (__instance.currentlyHeldObjectServer != null)
            {
                SharedData.Instance.GrabbableObjectId[__instance.currentlyHeldObjectServer.NetworkBehaviourId] = __instance.currentlyHeldObjectServer;
            }

        }
    }
}

