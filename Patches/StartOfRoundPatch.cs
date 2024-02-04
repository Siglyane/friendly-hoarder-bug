using FriendlyBug.Data;
using HarmonyLib;

namespace FriendlyBug
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
