using FriendlyHoarderBug.src.Enum;

namespace FriendlyHoarderBug.src.Components
{
    public class HoarderBugGifts
    {
        public GrabbableObject itemGrabbableObject;

        public HoarderBugGiftStatus status;

        public HoarderBugGifts(GrabbableObject newObject, HoarderBugGiftStatus newStatus)
        {
            itemGrabbableObject = newObject;
            status = newStatus;
        }

        public HoarderBugGifts(GrabbableObject newObject)
        {
            itemGrabbableObject = newObject;
            status = HoarderBugGiftStatus.Unknown;
        }
    }
}
