using UnityEngine;

namespace MultiplayerARPG
{
    public class LadderEntry : MonoBehaviour
    {
        public Ladder ladder;
        public LadderEntryType type = LadderEntryType.Bottom;
        public Transform TipTransform
        {
            get
            {
                switch (type)
                {
                    case LadderEntryType.Bottom:
                        return ladder.bottomTransform;
                    case LadderEntryType.Top:
                        return ladder.topTransform;
                }
                return null;
            }
        }

        private void Awake()
        {
            if (ladder == null)
                ladder = GetComponentInParent<Ladder>();
        }
    }
}