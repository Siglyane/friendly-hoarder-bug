using FriendlyHoarderBug.src.Components;
using HarmonyLib;

namespace FriendlyHoarderBug.src.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShipLeave")]
        static void PrefixShipLeave()
        {
            SharedData.FlushDictionaries();
        }
    }
}
