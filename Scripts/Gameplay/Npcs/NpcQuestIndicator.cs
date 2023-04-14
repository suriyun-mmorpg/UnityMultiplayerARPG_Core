using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public class NpcQuestIndicator : MonoBehaviour
    {
        [Tooltip("This will activate when has a quest which done all tasks")]
        public GameObject haveTasksDoneQuestsIndicator;
        [Tooltip("This will activate when there are in progress quests")]
        [FormerlySerializedAs("haveInProgressQuestIndicator")]
        public GameObject haveInProgressQuestsIndicator;
        [Tooltip("This will activate when there are new quests")]
        [FormerlySerializedAs("haveNewQuestIndicator")]
        public GameObject haveNewQuestsIndicator;
        public float updateWithinRange = 30f;
        public float updateRepeatRate = 0.5f;
        [HideInInspector, System.NonSerialized]
        public NpcEntity npcEntity;
        private float _lastUpdateTime;

        private void Awake()
        {
            if (npcEntity == null)
                npcEntity = GetComponentInParent<NpcEntity>();
        }

        private void Update()
        {
            if (npcEntity == null ||
                GameInstance.PlayingCharacterEntity == null ||
                Vector3.Distance(npcEntity.EntityTransform.position, GameInstance.PlayingCharacterEntity.EntityTransform.position) > updateWithinRange)
            {
                if (haveTasksDoneQuestsIndicator != null && haveTasksDoneQuestsIndicator.activeSelf)
                    haveTasksDoneQuestsIndicator.SetActive(false);
                if (haveInProgressQuestsIndicator != null && haveInProgressQuestsIndicator.activeSelf)
                    haveInProgressQuestsIndicator.SetActive(false);
                if (haveNewQuestsIndicator != null && haveNewQuestsIndicator.activeSelf)
                    haveNewQuestsIndicator.SetActive(false);
                return;
            }

            if (Time.unscaledTime - _lastUpdateTime >= updateRepeatRate)
            {
                _lastUpdateTime = Time.unscaledTime;
                // Indicator priority haveTasksDoneQuests > haveInProgressQuests > haveNewQuests
                bool isIndicatorShown = false;
                bool tempVisibleState;
                tempVisibleState = !isIndicatorShown && npcEntity.HaveTasksDoneQuests(GameInstance.PlayingCharacterEntity);
                isIndicatorShown = isIndicatorShown || tempVisibleState;
                if (haveTasksDoneQuestsIndicator != null && haveTasksDoneQuestsIndicator.activeSelf != tempVisibleState)
                    haveTasksDoneQuestsIndicator.SetActive(tempVisibleState);

                tempVisibleState = !isIndicatorShown && npcEntity.HaveInProgressQuests(GameInstance.PlayingCharacterEntity);
                isIndicatorShown = isIndicatorShown || tempVisibleState;
                if (haveInProgressQuestsIndicator != null && haveInProgressQuestsIndicator.activeSelf != tempVisibleState)
                    haveInProgressQuestsIndicator.SetActive(tempVisibleState);

                tempVisibleState = !isIndicatorShown && npcEntity.HaveNewQuests(GameInstance.PlayingCharacterEntity);
                isIndicatorShown = isIndicatorShown || tempVisibleState;
                if (haveNewQuestsIndicator != null && haveNewQuestsIndicator.activeSelf != tempVisibleState)
                    haveNewQuestsIndicator.SetActive(tempVisibleState);
            }
        }
    }
}
