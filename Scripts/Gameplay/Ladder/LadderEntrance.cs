using UnityEngine;

namespace MultiplayerARPG
{
    public class LadderEntrance : MonoBehaviour
    {
        public Ladder ladder;
        public LadderEntranceType type = LadderEntranceType.Bottom;
        public Transform TipTransform
        {
            get
            {
                switch (type)
                {
                    case LadderEntranceType.Bottom:
                        return ladder.bottomTransform;
                    case LadderEntranceType.Top:
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

        private void OnTriggerStay(Collider other)
        {
            OnTrigger(other);
        }

        private void OnTrigger(Collider other)
        {
            if (!other.transform.root.TryGetComponent(out BaseCharacterEntity characterEntity) ||
                !characterEntity.LadderComponent)
            {
                return;
            }
            characterEntity.LadderComponent.TriggeredLadderEntry = this;
        }
    }
}