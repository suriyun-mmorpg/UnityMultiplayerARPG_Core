using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public class NpcEntity : BaseGameEntity, IActivatableEntity
    {
        [Category(5, "NPC Settings")]
        [SerializeField]
        [Tooltip("It will use `startDialog` if `graph` is empty")]
        private BaseNpcDialog startDialog;
        [SerializeField]
        [Tooltip("It will use `graph` start dialog if this is not empty")]
        private NpcDialogGraph graph;

        [Category("Relative GameObjects/Transforms")]
        [SerializeField]
        [FormerlySerializedAs("uiElementTransform")]
        private Transform characterUiTransform = null;
        [SerializeField]
        [FormerlySerializedAs("miniMapElementContainer")]
        private Transform miniMapUiTransform = null;
        [SerializeField]
        private Transform questIndicatorContainer = null;

        private UINpcEntity _uiNpcEntity;
        private NpcQuestIndicator _questIndicator;

        public BaseNpcDialog StartDialog
        {
            get
            {
                if (graph != null && graph.nodes != null && graph.nodes.Count > 0)
                    return graph.nodes[0] as BaseNpcDialog;
                return startDialog;
            }
            set
            {
                startDialog = value;
            }
        }

        public NpcDialogGraph Graph
        {
            get
            {
                return graph;
            }
            set
            {
                graph = value;
            }
        }

        public Transform CharacterUiTransform
        {
            get
            {
                if (characterUiTransform == null)
                    characterUiTransform = EntityTransform;
                return characterUiTransform;
            }
        }

        public Transform MiniMapUiTransform
        {
            get
            {
                if (miniMapUiTransform == null)
                    miniMapUiTransform = EntityTransform;
                return miniMapUiTransform;
            }
        }

        public Transform QuestIndicatorContainer
        {
            get
            {
                if (questIndicatorContainer == null)
                    questIndicatorContainer = EntityTransform;
                return questIndicatorContainer;
            }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            if (startDialog != null)
                GameInstance.AddNpcDialogs(startDialog);
            if (graph != null)
                GameInstance.AddNpcDialogs(graph.GetDialogs());
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.npcTag;
            gameObject.layer = CurrentGameInstance.npcLayer;
        }

        protected override void EntityStart()
        {
            base.EntityStart();
            if (startDialog != null)
                GameInstance.AddNpcDialogs(startDialog);
            if (graph != null)
                GameInstance.AddNpcDialogs(graph.GetDialogs());
        }

        public override void OnSetup()
        {
            base.OnSetup();

            if (IsClient)
            {
                // Setup relates elements
                if (CurrentGameInstance.npcMiniMapObjects != null && CurrentGameInstance.npcMiniMapObjects.Length > 0)
                {
                    foreach (GameObject obj in CurrentGameInstance.npcMiniMapObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, MiniMapUiTransform.position, MiniMapUiTransform.rotation, MiniMapUiTransform);
                    }
                }

                if (CurrentGameInstance.npcUI != null)
                    InstantiateUI(CurrentGameInstance.npcUI);

                if (CurrentGameInstance.npcQuestIndicator != null)
                    InstantiateQuestIndicator(CurrentGameInstance.npcQuestIndicator);
            }
        }

        public void InstantiateUI(UINpcEntity prefab)
        {
            if (prefab == null)
                return;
            if (_uiNpcEntity != null)
                Destroy(_uiNpcEntity.gameObject);
            _uiNpcEntity = Instantiate(prefab, CharacterUiTransform);
            _uiNpcEntity.transform.localPosition = Vector3.zero;
            _uiNpcEntity.Data = this;
        }

        public void InstantiateQuestIndicator(NpcQuestIndicator prefab)
        {
            if (prefab == null)
                return;
            if (_questIndicator != null)
                Destroy(_questIndicator.gameObject);
            _questIndicator = Instantiate(prefab, QuestIndicatorContainer);
            _questIndicator.npcEntity = this;
        }

        private async UniTask FindQuestFromDialog(IPlayerCharacterData playerCharacter, HashSet<int> questIds, BaseNpcDialog baseDialog, List<NpcDialogMenu> foundMenus = null, List<BaseNpcDialog> foundDialogs = null)
        {
            if (foundMenus == null)
                foundMenus = new List<NpcDialogMenu>();

            if (foundDialogs == null)
                foundDialogs = new List<BaseNpcDialog>();

            if (baseDialog == null)
                return;

            NpcDialog dialog = baseDialog as NpcDialog;
            if (dialog == null || foundDialogs.Contains(dialog))
                return;

            foundDialogs.Add(dialog);

            List<UniTask> tasks = new List<UniTask>();
            List<UniTask<bool>> menuConditionTasks = new List<UniTask<bool>>();
            switch (dialog.type)
            {
                case NpcDialogType.Normal:
                    foreach (NpcDialogMenu menu in dialog.menus)
                    {
                        if (menu.isCloseMenu)
                            continue;
                        foundMenus.Add(menu);
                        tasks.Add(FindQuestFromDialog(playerCharacter, questIds, menu.dialog, foundMenus, foundDialogs));
                    }
                    break;
                case NpcDialogType.Quest:
                    if (dialog.quest != null)
                    {

                        if (foundMenus.Count > 0)
                        {
                            foreach (NpcDialogMenu menu in foundMenus)
                            {
                                menuConditionTasks.Add(menu.IsPassConditions(playerCharacter));
                            }
                        }
                        bool[] menuConditionResults = await UniTask.WhenAll(menuConditionTasks);
                        bool isPass = true;
                        foreach (bool menuConditionResult in menuConditionResults)
                        {
                            if (!menuConditionResult)
                            {
                                isPass = false;
                                break;
                            }
                        }
                        if (isPass)
                            questIds.Add(dialog.quest.DataId);
                    }
                    tasks.Add(FindQuestFromDialog(playerCharacter, questIds, dialog.questAcceptedDialog, foundMenus, foundDialogs));
                    tasks.Add(FindQuestFromDialog(playerCharacter, questIds, dialog.questDeclinedDialog, foundMenus, foundDialogs));
                    tasks.Add(FindQuestFromDialog(playerCharacter, questIds, dialog.questAbandonedDialog, foundMenus, foundDialogs));
                    tasks.Add(FindQuestFromDialog(playerCharacter, questIds, dialog.questCompletedDialog, foundMenus, foundDialogs));
                    break;
                case NpcDialogType.CraftItem:
                    tasks.Add(FindQuestFromDialog(playerCharacter, questIds, dialog.craftNotMeetRequirementsDialog, foundMenus, foundDialogs));
                    tasks.Add(FindQuestFromDialog(playerCharacter, questIds, dialog.craftDoneDialog, foundMenus, foundDialogs));
                    tasks.Add(FindQuestFromDialog(playerCharacter, questIds, dialog.craftCancelDialog, foundMenus, foundDialogs));
                    break;
                case NpcDialogType.SaveRespawnPoint:
                    tasks.Add(FindQuestFromDialog(playerCharacter, questIds, dialog.saveRespawnConfirmDialog, foundMenus, foundDialogs));
                    tasks.Add(FindQuestFromDialog(playerCharacter, questIds, dialog.saveRespawnCancelDialog, foundMenus, foundDialogs));
                    break;
                case NpcDialogType.Warp:
                    tasks.Add(FindQuestFromDialog(playerCharacter, questIds, dialog.warpCancelDialog, foundMenus, foundDialogs));
                    break;
            }

            try
            {
                await UniTask.WhenAll(tasks);
            }
            catch
            {
            }
        }

        public async UniTask<bool> HaveNewQuests(IPlayerCharacterData playerCharacter)
        {
            if (playerCharacter == null)
                return false;
            HashSet<int> questIds = new HashSet<int>();
            await FindQuestFromDialog(playerCharacter, questIds, StartDialog);
            Quest quest;
            List<int> clearedQuests = new List<int>();
            foreach (CharacterQuest characterQuest in playerCharacter.Quests)
            {
                quest = characterQuest.GetQuest();
                if (quest == null || characterQuest.isComplete)
                    continue;
                clearedQuests.Add(quest.DataId);
            }
            foreach (int questId in questIds)
            {
                if (!clearedQuests.Contains(questId) &&
                    GameInstance.Quests.ContainsKey(questId) &&
                    GameInstance.Quests[questId].CanReceiveQuest(playerCharacter))
                    return true;
            }
            return false;
        }

        public async UniTask<bool> HaveInProgressQuests(IPlayerCharacterData playerCharacter)
        {
            if (playerCharacter == null)
                return false;
            HashSet<int> questIds = new HashSet<int>();
            await FindQuestFromDialog(playerCharacter, questIds, StartDialog);
            Quest quest;
            List<int> inProgressQuests = new List<int>();
            foreach (CharacterQuest characterQuest in playerCharacter.Quests)
            {
                quest = characterQuest.GetQuest();
                if (quest == null || characterQuest.isComplete)
                    continue;
                if (quest.HaveToTalkToNpc(playerCharacter, this, out int talkToNpcTaskIndex, out _, out bool completeAfterTalked) && !completeAfterTalked && !characterQuest.CompletedTasks.Contains(talkToNpcTaskIndex))
                    return true;
                inProgressQuests.Add(quest.DataId);
            }
            foreach (int questId in questIds)
            {
                if (inProgressQuests.Contains(questId))
                    return true;
            }
            return false;
        }

        public async UniTask<bool> HaveTasksDoneQuests(IPlayerCharacterData playerCharacter)
        {
            if (playerCharacter == null)
                return false;
            HashSet<int> questIds = new HashSet<int>();
            await FindQuestFromDialog(playerCharacter, questIds, StartDialog);
            Quest quest;
            List<int> tasksDoneQuests = new List<int>();
            foreach (CharacterQuest characterQuest in playerCharacter.Quests)
            {
                quest = characterQuest.GetQuest();
                if (quest == null || characterQuest.isComplete || !characterQuest.IsAllTasksDoneAndIsCompletingTarget(playerCharacter, this))
                    continue;
                if (quest.HaveToTalkToNpc(playerCharacter, this, out _, out _, out bool completeAfterTalked) && completeAfterTalked)
                    return true;
                tasksDoneQuests.Add(quest.DataId);
            }
            foreach (int questId in questIds)
            {
                if (tasksDoneQuests.Contains(questId))
                    return true;
            }
            return false;
        }

        public virtual float GetActivatableDistance()
        {
            return GameInstance.Singleton.conversationDistance;
        }

        public virtual bool ShouldClearTargetAfterActivated()
        {
            return false;
        }

        public virtual bool ShouldNotActivateAfterFollowed()
        {
            return false;
        }

        public virtual bool ShouldBeAttackTarget()
        {
            return false;
        }

        public virtual bool CanActivate()
        {
            return true;
        }

        public virtual void OnActivate()
        {
            GameInstance.PlayingCharacterEntity.NpcAction.CallServerNpcActivate(ObjectId);
        }
    }
}
