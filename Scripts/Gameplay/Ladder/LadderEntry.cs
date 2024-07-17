using UnityEngine;

namespace MultiplayerARPG
{
    public class LadderEntry : MonoBehaviour
    {
        public Ladder ladder;
        public Transform enterLadderPoint;
        public Transform exitLadderPoint;
        public LadderEntryDirection entryDirection = LadderEntryDirection.Up;

        private void Awake()
        {
            if (ladder == null)
                ladder = GetComponentInParent<Ladder>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.transform.root.TryGetComponent(out BaseCharacterEntity characterEntity))
            {
                return;
            }
            characterEntity.LadderComponent.TriggeredLadderEntry = this;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.transform.root.TryGetComponent(out BaseCharacterEntity characterEntity))
            {
                return;
            }
            if (characterEntity.LadderComponent.TriggeredLadderEntry == this)
            {
                characterEntity.LadderComponent.TriggeredLadderEntry = null;
            }
        }
    }
}