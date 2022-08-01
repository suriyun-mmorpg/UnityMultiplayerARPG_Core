using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public class NpcEntity : BaseGameEntity, IActivatePressActivatableEntity, IClickActivatableEntity
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

        private UINpcEntity uiNpcEntity;
        private NpcQuestIndicator questIndicator;

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
                    characterUiTransform = CacheTransform;
                return characterUiTransform;
            }
        }

        public Transform MiniMapUiTransform
        {
            get
            {
                if (miniMapUiTransform == null)
                    miniMapUiTransform = CacheTransform;
                return miniMapUiTransform;
            }
        }

        public Transform QuestIndicatorContainer
        {
            get
            {
                if (questIndicatorContainer == null)
                    questIndicatorContainer = CacheTransform;
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
            if (uiNpcEntity != null)
                Destroy(uiNpcEntity.gameObject);
            uiNpcEntity = Instantiate(prefab, CharacterUiTransform);
            uiNpcEntity.transform.localPosition = Vector3.zero;
            uiNpcEntity.Data = this;
        }

        public void InstantiateQuestIndicator(NpcQuestIndicator prefab)
        {
            if (prefab == null)
                return;
            if (questIndicator != null)
                Destroy(questIndicator.gameObject);
            questIndicator = Instantiate(prefab, QuestIndicatorContainer);
            questIndicator.npcEntity = this;
        }

        private void FindQuestFromDialog(IPlayerCharacterData playerCharacter, HashSet<int> questIds, BaseNpcDialog baseDialog, List<BaseNpcDialog> foundDialogs = null)
        {
            if (foundDialogs == null)
                foundDialogs = new List<BaseNpcDialog>();

            if (baseDialog == null)
                return;

            NpcDialog dialog = baseDialog as NpcDialog;
            if (dialog == null || foundDialogs.Contains(dialog))
                return;

            foundDialogs.Add(dialog);

            switch (dialog.type)
            {
                case NpcDialogType.Normal:
                    foreach (NpcDialogMenu menu in dialog.menus)
                    {
                        if (menu.isCloseMenu || !menu.IsPassConditions(playerCharacter)) continue;
                        FindQuestFromDialog(playerCharacter, questIds, menu.dialog, foundDialogs);
                    }
                    break;
                case NpcDialogType.Quest:
                    if (dialog.quest != null)
                        questIds.Add(dialog.quest.DataId);
                    FindQuestFromDialog(playerCharacter, questIds, dialog.questAcceptedDialog, foundDialogs);
                    FindQuestFromDialog(playerCharacter, questIds, dialog.questDeclinedDialog, foundDialogs);
                    FindQuestFromDialog(playerCharacter, questIds, dialog.questAbandonedDialog, foundDialogs);
                    FindQuestFromDialog(playerCharacter, questIds, dialog.questCompletedDialog, foundDialogs);
                    break;
                case NpcDialogType.CraftItem:
                    FindQuestFromDialog(playerCharacter, questIds, dialog.craftNotMeetRequirementsDialog, foundDialogs);
                    FindQuestFromDialog(playerCharacter, questIds, dialog.craftDoneDialog, foundDialogs);
                    FindQuestFromDialog(playerCharacter, questIds, dialog.craftCancelDialog, foundDialogs);
                    break;
                case NpcDialogType.SaveRespawnPoint:
                    FindQuestFromDialog(playerCharacter, questIds, dialog.saveRespawnConfirmDialog, foundDialogs);
                    FindQuestFromDialog(playerCharacter, questIds, dialog.saveRespawnCancelDialog, foundDialogs);
                    break;
                case NpcDialogType.Warp:
                    FindQuestFromDialog(playerCharacter, questIds, dialog.warpCancelDialog, foundDialogs);
                    break;
            }
        }

        public bool HaveNewQuests(IPlayerCharacterData playerCharacter)
        {
            if (playerCharacter == null)
                return false;
            HashSet<int> questIds = new HashSet<int>();
            FindQuestFromDialog(playerCharacter, questIds, StartDialog);
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

        public bool HaveInProgressQuests(IPlayerCharacterData playerCharacter)
        {
            if (playerCharacter == null)
                return false;
            HashSet<int> questIds = new HashSet<int>();
            FindQuestFromDialog(playerCharacter, questIds, StartDialog);
            Quest quest;
            int talkToNpcTaskIndex;
            List<int> inProgressQuests = new List<int>();
            foreach (CharacterQuest characterQuest in playerCharacter.Quests)
            {
                quest = characterQuest.GetQuest();
                if (quest == null || characterQuest.isComplete)
                    continue;
                if (quest.HaveToTalkToNpc(playerCharacter, this, out talkToNpcTaskIndex, out _, out _) && !characterQuest.CompletedTasks.Contains(talkToNpcTaskIndex))
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

        public bool HaveTasksDoneQuests(IPlayerCharacterData playerCharacter)
        {
            if (playerCharacter == null)
                return false;
            HashSet<int> questIds = new HashSet<int>();
            FindQuestFromDialog(playerCharacter, questIds, StartDialog);
            Quest quest;
            List<int> tasksDoneQuests = new List<int>();
            foreach (CharacterQuest characterQuest in playerCharacter.Quests)
            {
                quest = characterQuest.GetQuest();
                if (quest == null || characterQuest.isComplete || !characterQuest.IsAllTasksDoneAndIsCompletingTarget(playerCharacter, this))
                    continue;
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

        public virtual bool ShouldBeAttackTarget()
        {
            return false;
        }

        public virtual bool ShouldClearTargetAfterActivated()
        {
            return false;
        }

        public virtual bool CanActivateByActivateKey()
        {
            return true;
        }

        public virtual void OnActivateByActivateKey()
        {
            GameInstance.PlayingCharacterEntity.NpcAction.CallServerNpcActivate(ObjectId);
        }

        public virtual bool CanActivateByClick()
        {
            return CanActivateByActivateKey();
        }

        public virtual void OnActivateByClick()
        {
            OnActivateByActivateKey();
        }
    }
}
