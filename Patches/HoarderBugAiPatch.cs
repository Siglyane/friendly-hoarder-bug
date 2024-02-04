using FriendlyBug.Data;
using HarmonyLib;
using HoarderFriendlyBug.Components;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace FriendlyBug.Patches
{
    [HarmonyPatch(typeof(HoarderBugAI))]
    internal class HoarderBugAiPatch
    {

        [HarmonyPatch("GrabTargetItemIfClose")]
        [HarmonyPostfix]
        static void GrabTargetItemIfClosePatch(HoarderBugAI __instance, ref bool __result)
        {
            if (__result)
            {
                var hasPlayerHeldIt = SharedData.Instance.LastPlayerWhoHeld.ContainsKey(__instance.heldItem.itemGrabbableObject);
                var hasHoarderBugABestFriend = SharedData.Instance.HoarderBugBestFriend.ContainsKey(__instance);
                if (hasPlayerHeldIt)
                {
                    var playerWhoHoldItem = SharedData.Instance.LastPlayerWhoHeld.GetValueSafe(__instance.heldItem.itemGrabbableObject);
                    bool hasFavoriteItem = SharedData.Instance.HoarderBugFavoriteItem.ContainsKey(__instance);


                    if (!SharedData.Instance.HoarderBugIsFriendly.ContainsKey(__instance)) SharedData.Instance.HoarderBugIsFriendly.Add(__instance, true);

                    SharedData.UpsertFriendGiftsList(__instance.heldItem.itemGrabbableObject, giftStatus: HoarderBugGiftStatus.HoarderBugOwned);

                    if (!hasFavoriteItem || SharedData.Instance.HoarderBugFavoriteItem[__instance].itemGrabbableObject.itemProperties.creditsWorth < __instance.heldItem.itemGrabbableObject.itemProperties.creditsWorth)
                    {
                        SharedData.Instance.isHoarderBugFriendly[playerWhoHoldItem] = true;
                        SharedData.Instance.HoarderBugFavoriteItem[__instance] = __instance.heldItem;
                        SharedData.Instance.HoarderBugBestFriend[__instance] = playerWhoHoldItem;
                    }

                    SharedData.UpsertFriendList(__instance);
                }
                else if (hasHoarderBugABestFriend)
                {
                    CheckForFriend(__instance);
                }

            }
        }

        [HarmonyPatch("IsHoarderBugAngry")]
        [HarmonyPrefix]
        static bool IsHoarderBugAngryPatch(HoarderBugAI __instance)
        {
            if (SharedData.Instance.HoarderBugIsFriendly.ContainsKey(__instance))
            {
                return false;
            }

            return true;

        }

        [HarmonyPatch("DropItemClientRpc")]
        [HarmonyPostfix]
        static void DropItemClientRpcPatch(ref HoarderBugAI __instance)
        {
            if (SharedData.Instance.HoarderBugIsFriendly.ContainsKey(__instance))
            {
                GiftFriendRoutine(ref __instance);
            }
        }


        [HarmonyPatch("SetReturningToNest")]
        [HarmonyPrefix]
        static bool StepSetReturninToNest(HoarderBugAI __instance)
        {
            if (__instance.movingTowardsTargetPlayer && SharedData.Instance.HoarderBugIsFriendly.ContainsKey(__instance))
            {
                return true;
            }
            return false;
        }

        [HarmonyPatch("ExitChaseMode")]
        [HarmonyPrefix]
        static bool StepExitChaseMode(HoarderBugAI __instance)
        {
            if (__instance.movingTowardsTargetPlayer && SharedData.Instance.HoarderBugIsFriendly.ContainsKey(__instance))
            {
                return true;
            }
            return false;
        }


        [HarmonyPatch("DoAIInterval")]
        [HarmonyPrefix]
        static void DoAIIntervalPatch(HoarderBugAI __instance)
        {
            if ((__instance.movingTowardsTargetPlayer ||
                (__instance.heldItem != null &&
                SharedData.Instance.HoarderBugFavoriteItem.ContainsKey(__instance) &&
                SharedData.Instance.HoarderBugFavoriteItem[__instance].itemGrabbableObject != __instance.heldItem.itemGrabbableObject)) &&
                SharedData.Instance.HoarderBugIsFriendly.ContainsKey(__instance))
            {
                __instance.currentBehaviourStateIndex = 0;
                CheckForFriend(__instance);
            }
        }


        public static void GiftFriendRoutine(ref HoarderBugAI __instance)
        {
            if (
                SharedData.Instance.HoarderBugBestFriend.ContainsKey(__instance) &&
                SharedData.Instance.HoarderBugFavoriteItem.ContainsKey(__instance) &&
                SharedData.Instance.HoarderBugFavoriteItem[__instance] != __instance.heldItem
                )
            {
                UpdateGiftsList(__instance);

                Debug.Log("GiftFriend");
                if (__instance.heldItem == null && __instance.targetItem == null)
                {
                    SetToMoveTowardsGift(ref __instance);
                }
                else if (__instance.heldItem == null)
                {
                    GrabGift(__instance);
                }



                if (__instance.heldItem != null && __instance.heldItem != SharedData.Instance.HoarderBugFavoriteItem[__instance])
                {
                    CheckForFriend(__instance);
                }

            }
        }

        private static void CheckForFriend(HoarderBugAI __instance)
        {
            Debug.Log("GetAllPlayersInLineOfSight");

            var PlayersNear = __instance.GetAllPlayersInLineOfSight(70f, 30, __instance.eye, proximityCheck: 1f);
            Debug.Log("PlayersNear? " + PlayersNear != null);

            if (PlayersNear != null && PlayersNear.Any(player => SharedData.Instance.HoarderBugBestFriend[__instance]))
            {
                Debug.Log("Achei meu amigo");
                DropItem(__instance);
            }
            else
            {
                MoveTowardsPlayer(__instance);
            }
        }

        private static void DropItem(HoarderBugAI __instance)
        {
            if (__instance.heldItem != null)
            {
                var heldItem = __instance.heldItem.itemGrabbableObject.GetComponent<NetworkObject>();

                SharedData.UpsertFriendGiftsList(__instance.heldItem.itemGrabbableObject, giftStatus: HoarderBugGiftStatus.GiftedToPlayer);
                var dropItem = AccessTools.Method(typeof(HoarderBugAI), "DropItemAndCallDropRPC");
                dropItem.Invoke(__instance, new object[] { heldItem, false });
                var exitChaseMode = AccessTools.Method(typeof(HoarderBugAI), "ExitChaseMode");
                exitChaseMode.Invoke(__instance, new object[] { });
                return;
            }
        }

        private static void MoveTowardsPlayer(HoarderBugAI __instance)
        {
            if (SharedData.Instance.HoarderBugBestFriend[__instance].isInsideFactory && !__instance.movingTowardsTargetPlayer)
            {
                __instance.StopSearch(__instance.searchForItems, clear: false);
                __instance.SetMovingTowardsTargetPlayer(SharedData.Instance.HoarderBugBestFriend[__instance]);
                var SetGoTowardsTargetObject = AccessTools.Method(typeof(HoarderBugAI), "SetGoTowardsTargetObject");
                SetGoTowardsTargetObject.Invoke(__instance, new object[] { SharedData.Instance.HoarderBugBestFriend[__instance].gameObject });
                __instance.SetMovingTowardsTargetPlayer(SharedData.Instance.HoarderBugBestFriend[__instance]);
            }
        }

        private static void GrabGift(HoarderBugAI __instance)
        {
            if (__instance.targetItem != null)
            {
                var grabItem = AccessTools.Method(typeof(HoarderBugAI), "GrabTargetItemIfClose");
                var canGrabItem = (bool)grabItem.Invoke(__instance, new object[] { });

                if (!canGrabItem)
                {
                    SetToMoveTowardsGift(ref __instance);
                }
                else
                {
                    CheckForFriend(__instance);
                }
            }

        }

        private static void SetToMoveTowardsGift(ref HoarderBugAI __instance)
        {
            var targetItem = __instance.targetItem;
            if (targetItem == null && SharedData.GetRandomGift() != null)
            {
                targetItem = SharedData.GetRandomGift().itemGrabbableObject;
            }
            else
            {
                return;
            }

            __instance.targetItem = targetItem;
            var SetGoTowardsTargetObject = AccessTools.Method(typeof(HoarderBugAI), "SetGoTowardsTargetObject");
            SetGoTowardsTargetObject.Invoke(__instance, new object[] { targetItem.gameObject });

            if (__instance.targetItem == null)
            {
                SharedData.UpsertFriendGiftsList(targetItem, giftStatus: HoarderBugGiftStatus.Blocked);
                GiftFriendRoutine(ref __instance);
            }


        }

        private static void UpdateGiftsList(HoarderBugAI __instance)
        {
            if (SharedData.Instance.HoarderBugBestFriend.ContainsKey(__instance))
            {
                List<HoarderBugGifts> Gifted = new List<HoarderBugGifts>();
                List<GrabbableObject> notGifted = new List<GrabbableObject>();
                GrabbableObject[] array = Object.FindObjectsOfType<GrabbableObject>();
                if (!array.Any()) return;

                if (SharedData.Instance.GiftList.Any())
                {
                    Gifted = SharedData.Instance.GiftList.Where(gift => gift.status == HoarderBugGiftStatus.GiftedToPlayer || gift.status == HoarderBugGiftStatus.Blocked).ToList();
                    notGifted = array.Where(item => !SharedData.Instance.GiftList.Any(gift => gift.itemGrabbableObject == item)).ToList();
                    SharedData.Instance.GiftList.Clear();
                }

                var NotGiftable = new string[] { "Hive", "Apparatus", "clipboard", "Sticky note" };
                foreach (var grabbableObject in notGifted)
                {

                    if (grabbableObject.grabbableToEnemies &&
                        grabbableObject.isInFactory &&
                        !NotGiftable.Contains(grabbableObject.itemProperties.itemName)
                        )
                    {
                        Gifted.Add(new HoarderBugGifts(grabbableObject));
                    }
                }

                SharedData.Instance.GiftList = Gifted;
            }
        }
    }

}

