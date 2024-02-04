using GameNetcodeStuff;
using HoarderFriendlyBug.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FriendlyBug.Data
{
    internal class SharedData
    {
        private static SharedData _instance;
        public static SharedData Instance => _instance ?? (_instance = new SharedData());

        private readonly Random random = new Random();

        //HoarderBug Data
        public Dictionary<HoarderBugAI, bool> HoarderBugIsFriendly = new Dictionary<HoarderBugAI, bool>();

        public Dictionary<HoarderBugAI, List<PlayerControllerB>> HoarderBugFriendsList = new Dictionary<HoarderBugAI, List<PlayerControllerB>>();

        public Dictionary<HoarderBugAI, PlayerControllerB> HoarderBugBestFriend = new Dictionary<HoarderBugAI, PlayerControllerB>();

        public Dictionary<HoarderBugAI, HoarderBugItem> HoarderBugFavoriteItem = new Dictionary<HoarderBugAI, HoarderBugItem>();

        public Dictionary<HoarderBugAI, HoarderBugGifts> TargetGift = new Dictionary<HoarderBugAI, HoarderBugGifts>();

        public List<HoarderBugGifts> GiftList = new List<HoarderBugGifts>();


        //Player Data
        public Dictionary<PlayerControllerB, bool> isHoarderBugFriendly = new Dictionary<PlayerControllerB, bool>();

        //GrabbableObject Data
        public Dictionary<GrabbableObject, PlayerControllerB> LastPlayerWhoHeld { get; } = new Dictionary<GrabbableObject, PlayerControllerB>();

        public Dictionary<ulong, GrabbableObject> GrabbableObjectId = new Dictionary<ulong, GrabbableObject>();

        public static HoarderBugGifts GetRandomGift()
        {
            return Instance.GiftList.OrderBy(x => Instance.random.Next()).FirstOrDefault();
        }

        public static void UpsertFriendGiftsList(GrabbableObject grabbableObject, HoarderBugGiftStatus giftStatus = HoarderBugGiftStatus.Unknown)
        {
            HoarderBugGifts itemReceived = new HoarderBugGifts(grabbableObject, giftStatus);
            int itemInListIndex = Instance.GiftList.FindIndex(it => it.itemGrabbableObject == grabbableObject);

            if (itemInListIndex == -1)
            {
                Instance.GiftList.Add(itemReceived);
                return;
            }
            if (Instance.GiftList[itemInListIndex].status == HoarderBugGiftStatus.HoarderBugOwned) return;
            Instance.GiftList[itemInListIndex] = itemReceived;
        }

        public static void UpsertFriendList(HoarderBugAI hoarderBugAI)
        {
            if (!Instance.LastPlayerWhoHeld.ContainsKey(hoarderBugAI.heldItem.itemGrabbableObject)) return;
            PlayerControllerB newFriend = Instance.LastPlayerWhoHeld[hoarderBugAI.heldItem.itemGrabbableObject];

            if (!Instance.HoarderBugFriendsList.ContainsKey(hoarderBugAI))
            {
                List<PlayerControllerB> bugFriendsList = new List<PlayerControllerB> { newFriend };
                Instance.HoarderBugFriendsList.Add(hoarderBugAI, bugFriendsList);
                return;
            }

            int playerInListIndex = Instance.HoarderBugFriendsList[hoarderBugAI].FindIndex(it => it == newFriend);

            if (playerInListIndex == -1)
            {
                Instance.HoarderBugFriendsList[hoarderBugAI].Append(newFriend);
            }
            else
            {
                Instance.HoarderBugFriendsList[hoarderBugAI][playerInListIndex] = newFriend;
            }

        }

        public static void FlushDictionaries()
        {
            Instance.HoarderBugIsFriendly.Clear();
            Instance.HoarderBugFriendsList.Clear();
            Instance.HoarderBugBestFriend.Clear();
            Instance.HoarderBugFavoriteItem.Clear();
            Instance.TargetGift.Clear();
            Instance.GiftList.Clear();
            Instance.isHoarderBugFriendly.Clear();
            Instance.GrabbableObjectId.Clear();
            Instance.LastPlayerWhoHeld.Clear();
        }
    }
}
