using UnityEngine;
using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public sealed class NpcEntity : RpgNetworkEntity
    {
        [Tooltip("Set it to force to not change character model by data Id, when set it model container will not be used")]
        [SerializeField]
        private NpcDialog startDialog;
        [Header("Relates Element Containers")]
        public Transform uiElementTransform;
        public Transform questIndicatorContainer;

        [Header("Sync Lists")]
        [SerializeField]
        private SyncListInt questIds = new SyncListInt();

        private UINpcEntity uiNpcEntity;
        private NpcQuestIndicator questIndicator;

        public NpcDialog StartDialog
        {
            get { return startDialog; }
            set
            {
                if (startDialog != value)
                {
                    startDialog = value;
                    SetupQuestIds();
                }
            }
        }

        public Transform UIElementTransform
        {
            get
            {
                if (uiElementTransform == null)
                    uiElementTransform = CacheTransform;
                return uiElementTransform;
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

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = gameInstance.npcTag;
            gameObject.layer = gameInstance.characterLayer;
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            questIds.forOwnerOnly = false;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            SetupQuestIds();

            // Setup relates elements
            if (gameInstance.npcUI != null)
                InstantiateUI(gameInstance.npcUI);
            if (gameInstance.npcQuestIndicator != null)
                InstantiateQuestIndicator(gameInstance.npcQuestIndicator);
        }

        public void InstantiateUI(UINpcEntity prefab)
        {
            if (prefab == null)
                return;
            if (uiNpcEntity != null)
                Destroy(uiNpcEntity.gameObject);
            uiNpcEntity = Instantiate(prefab);
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

        private void SetupQuestIds()
        {
            if (!IsServer)
                return;
            
            questIds.Clear();
            FindQuestFromDialog(startDialog);
        }

        private void FindQuestFromDialog(NpcDialog dialog, List<NpcDialog> foundDialogs = null)
        {
            if (foundDialogs == null)
                foundDialogs = new List<NpcDialog>();

            if (dialog == null || foundDialogs.Contains(dialog))
                return;

            foundDialogs.Add(dialog);

            switch (dialog.type)
            {
                case NpcDialogType.Normal:
                    foreach (var menu in dialog.menus)
                    {
                        if (menu.isCloseMenu) continue;
                        FindQuestFromDialog(menu.dialog, foundDialogs);
                    }
                    break;
                case NpcDialogType.Quest:
                    if (dialog.quest != null)
                        questIds.Add(dialog.quest.DataId);
                    FindQuestFromDialog(dialog.questAcceptedDialog, foundDialogs);
                    FindQuestFromDialog(dialog.questDeclinedDialog, foundDialogs);
                    FindQuestFromDialog(dialog.questAbandonedDialog, foundDialogs);
                    FindQuestFromDialog(dialog.questCompletedDailog, foundDialogs);
                    break;
                case NpcDialogType.CraftItem:
                    FindQuestFromDialog(dialog.craftNotMeetRequirementsDialog, foundDialogs);
                    FindQuestFromDialog(dialog.craftDoneDialog, foundDialogs);
                    FindQuestFromDialog(dialog.craftCancelDialog, foundDialogs);
                    break;
            }
        }

        public bool HaveNewQuests(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null)
                return false;
            var clearedQuests = new List<int>();
            foreach (var characterQuest in playerCharacterEntity.Quests)
            {
                var quest = characterQuest.GetQuest();
                if (quest != null && characterQuest.isComplete)
                    clearedQuests.Add(quest.DataId);
            }
            foreach (var questId in questIds)
            {
                if (!clearedQuests.Contains(questId))
                    return true;
            }
            return false;
        }

        public bool HaveInProgressQuests(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null)
                return false;
            var inProgressQuests = new List<int>();
            foreach (var characterQuest in playerCharacterEntity.Quests)
            {
                var quest = characterQuest.GetQuest();
                if (quest != null && !characterQuest.isComplete)
                    inProgressQuests.Add(quest.DataId);
            }
            foreach (var questId in questIds)
            {
                if (inProgressQuests.Contains(questId))
                    return true;
            }
            return false;
        }
    }
}
