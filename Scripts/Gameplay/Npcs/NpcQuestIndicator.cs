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
        protected BasePlayerCharacterEntity _previousEntity;
        protected bool _isUpdating = false;

        private void Awake()
        {
            if (npcEntity == null)
                npcEntity = GetComponentInParent<NpcEntity>();
            GameInstance.onSetPlayingCharacter += GameInstance_onSetPlayingCharacter;
            if (GameInstance.PlayingCharacterEntity != null)
                GameInstance_onSetPlayingCharacter(GameInstance.PlayingCharacterEntity);
        }

        private void OnDestroy()
        {
            GameInstance.onSetPlayingCharacter -= GameInstance_onSetPlayingCharacter;
            if (_previousEntity != null)
            {
                _previousEntity.onNonEquipItemsOperation -= PlayingCharacterEntity_onNonEquipItemsOperation;
                _previousEntity.onQuestsOperation -= PlayingCharacterEntity_onQuestsOperation;
            }
        }

        private void GameInstance_onSetPlayingCharacter(IPlayerCharacterData playingCharacterData)
        {
            if (_previousEntity != null)
            {
                _previousEntity.onNonEquipItemsOperation -= PlayingCharacterEntity_onNonEquipItemsOperation;
                _previousEntity.onQuestsOperation -= PlayingCharacterEntity_onQuestsOperation;
            }

            BasePlayerCharacterEntity playerCharacterEntity = playingCharacterData as BasePlayerCharacterEntity;
            _previousEntity = playerCharacterEntity;

            if (_previousEntity != null)
            {
                _previousEntity.onNonEquipItemsOperation += PlayingCharacterEntity_onNonEquipItemsOperation;
                _previousEntity.onQuestsOperation += PlayingCharacterEntity_onQuestsOperation;
            }
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
            _isUpdating = true;
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
