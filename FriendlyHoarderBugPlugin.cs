using BepInEx;
using BepInEx.Logging;
using FriendlyBug;
using FriendlyBug.Patches;
using GameNetcodeStuff;
using HarmonyLib;
using HoarderFriendlyBug.Network;
using RuntimeNetcodeRPCValidator;

namespace HoarderFriendlyBug
{
    [BepInDependency(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION)]
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class FriendlyHoarderBugPlugin : BaseUnityPlugin
    {
        private const string modGUID = "siglyane.friendlybugmod";
        private const string modName = "Friendly Hoarder Bug";
        private const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        internal ManualLogSource mls;

        public static FriendlyHoarderBugPlugin Instance;

        private NetcodeValidator netcodeValidator;


        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            netcodeValidator = new NetcodeValidator(modGUID);
            netcodeValidator.PatchAll();
            netcodeValidator.BindToPreExistingObjectByBehaviour<NetworkHandler, PlayerControllerB>();

            harmony.PatchAll(typeof(FriendlyHoarderBugPlugin));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(HoarderBugAiPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
        }
    }
}

