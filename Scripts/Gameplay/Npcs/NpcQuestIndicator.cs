using Cysharp.Threading.Tasks;
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
        [HideInInspector, System.NonSerialized]
        public NpcEntity npcEntity;
        protected bool _isUpdating = true;

        private void Awake()
        {
            if (npcEntity == null)
                npcEntity = GetComponentInParent<NpcEntity>();

            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += PlayingCharacterEntity_onNonEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onQuestsOperation += PlayingCharacterEntity_onQuestsOperation;
        }

        private void OnDestroy()
        {
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation -= PlayingCharacterEntity_onNonEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onQuestsOperation -= PlayingCharacterEntity_onQuestsOperation;
        }

        private void PlayingCharacterEntity_onNonEquipItemsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation op, int index)
        {
            UpdateStatus().Forget();
        }

        private void PlayingCharacterEntity_onQuestsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation op, int index)
        {
            UpdateStatus().Forget();
        }

        private async UniTaskVoid UpdateStatus()
        {
            if (_isUpdating)
                return;
            // Indicator priority haveTasksDoneQuests > haveInProgressQuests > haveNewQuests
            bool isIndicatorShown = false;
            bool tempVisibleState;
            tempVisibleState = !isIndicatorShown && await npcEntity.HaveTasksDoneQuests(GameInstance.PlayingCharacterEntity);
            isIndicatorShown = isIndicatorShown || tempVisibleState;
            if (haveTasksDoneQuestsIndicator != null && haveTasksDoneQuestsIndicator.activeSelf != tempVisibleState)
                haveTasksDoneQuestsIndicator.SetActive(tempVisibleState);

            tempVisibleState = !isIndicatorShown && await npcEntity.HaveInProgressQuests(GameInstance.PlayingCharacterEntity);
            isIndicatorShown = isIndicatorShown || tempVisibleState;
            if (haveInProgressQuestsIndicator != null && haveInProgressQuestsIndicator.activeSelf != tempVisibleState)
                haveInProgressQuestsIndicator.SetActive(tempVisibleState);

            tempVisibleState = !isIndicatorShown && await npcEntity.HaveNewQuests(GameInstance.PlayingCharacterEntity);
            isIndicatorShown = isIndicatorShown || tempVisibleState;
            if (haveNewQuestsIndicator != null && haveNewQuestsIndicator.activeSelf != tempVisibleState)
                haveNewQuestsIndicator.SetActive(tempVisibleState);
            _isUpdating = false;
        }
    }
}
