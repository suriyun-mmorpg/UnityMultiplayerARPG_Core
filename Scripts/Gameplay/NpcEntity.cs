using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterModel))]
    public sealed class NpcEntity : RpgNetworkEntity
    {
        [Tooltip("Set it to force to not change character model by data Id, when set it model container will not be used")]
        [SerializeField]
        private NpcDialog startDialog;
        [Header("Relates Element Containers")]
        public Transform uiElementContainer;
        public Transform uiQuestIndicatorContainer;

        [Header("Sync Lists")]
        [SerializeField]
        private SyncListInt questIds = new SyncListInt();

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

        public Transform UIElementContainer
        {
            get
            {
                if (uiElementContainer == null)
                    uiElementContainer = CacheTransform;
                return uiElementContainer;
            }
        }

        public Transform UIQuestIndicatorTransform
        {
            get
            {
                if (uiQuestIndicatorContainer == null)
                    uiQuestIndicatorContainer = CacheTransform;
                return uiQuestIndicatorContainer;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = gameInstance.npcTag;
            gameObject.layer = gameInstance.characterLayer;
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            questIds.forOwnerOnly = false;
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
                        FindQuestFromDialog(menu.dialog);
                    }
                    break;
                case NpcDialogType.Quest:
                    if (dialog.quest == null)
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
