using UnityEngine;

namespace MultiplayerARPG
{
    public class LadderEntry : MonoBehaviour
    {
        public Ladder ladder;
        public LadderEntryType type = LadderEntryType.Bottom;

        private void Awake()
        {
            if (ladder == null)
                ladder = GetComponentInParent<Ladder>();
        }
    }
}