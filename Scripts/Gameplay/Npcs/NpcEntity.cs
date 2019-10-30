using UnityEngine;
using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public sealed class NpcEntity : BaseGameEntity
    {
        [SerializeField]
        [Tooltip("It will use `startDialog` if `graph` is empty")]
        private NpcDialog startDialog;
        [SerializeField]
        [Tooltip("It will use `graph` start dialog if this is not empty")]
        private NpcDialogGraph graph;
        [Header("Relates Element Containers")]
        public Transform uiElementTransform;
        public Transform miniMapElementContainer;
        public Transform questIndicatorContainer;

        [Header("Sync Lists")]
        [SerializeField]
        private SyncListInt questIds = new SyncListInt();

        private UINpcEntity uiNpcEntity;
        private NpcQuestIndicator questIndicator;

        public NpcDialog StartDialog
        {
            get
            {
                if (graph != null && graph.nodes != null && graph.nodes.Count > 0)
                    return graph.nodes[0] as NpcDialog;
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

        public Transform UIElementTransform
        {
            get
            {
                if (uiElementTransform == null)
                    uiElementTransform = CacheTransform;
                return uiElementTransform;
            }
        }

        public Transform MiniMapElementContainer
        {
            get
            {
                if (miniMapElementContainer == null)
                    miniMapElementContainer = CacheTransform;
                return miniMapElementContainer;
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

        protected override void EntityStart()
        {
            base.EntityStart();
            if (Graph != null)
                GameInstance.AddNpcDialogs(Graph.GetDialogs());
            else if (StartDialog != null)
                GameInstance.AddNpcDialogs(new NpcDialog[] { StartDialog });
            SetupQuestIds();
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
            if (gameInstance.npcMiniMapObjects != null && gameInstance.npcMiniMapObjects.Length > 0)
            {
                foreach (GameObject obj in gameInstance.npcMiniMapObjects)
                {
                    if (obj == null) continue;
                    Instantiate(obj, MiniMapElementContainer.position, MiniMapElementContainer.rotation, MiniMapElementContainer);
                }
            }

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
            uiNpcEntity = Instantiate(prefab, UIElementTransform);
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

        private void SetupQuestIds()
        {
            if (!IsServer)
                return;

            questIds.Clear();
            FindQuestFromDialog(StartDialog);
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
                    foreach (NpcDialogMenu menu in dialog.menus)
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
                    FindQuestFromDialog(dialog.questCompletedDialog, foundDialogs);
                    break;
                case NpcDialogType.CraftItem:
                    FindQuestFromDialog(dialog.craftNotMeetRequirementsDialog, foundDialogs);
                    FindQuestFromDialog(dialog.craftDoneDialog, foundDialogs);
                    FindQuestFromDialog(dialog.craftCancelDialog, foundDialogs);
                    break;
                case NpcDialogType.SaveRespawnPoint:
                    FindQuestFromDialog(dialog.saveRespawnConfirmDialog, foundDialogs);
                    FindQuestFromDialog(dialog.saveRespawnCancelDialog, foundDialogs);
                    break;
                case NpcDialogType.Warp:
                    FindQuestFromDialog(dialog.warpCancelDialog, foundDialogs);
                    break;
            }
        }

        public bool HaveNewQuests(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null)
                return false;
            List<int> clearedQuests = new List<int>();
            foreach (CharacterQuest characterQuest in playerCharacterEntity.Quests)
            {
                Quest quest = characterQuest.GetQuest();
                if (quest != null && characterQuest.isComplete)
                    clearedQuests.Add(quest.DataId);
            }
            foreach (int questId in questIds)
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
            List<int> inProgressQuests = new List<int>();
            foreach (CharacterQuest characterQuest in playerCharacterEntity.Quests)
            {
                Quest quest = characterQuest.GetQuest();
                if (quest != null && !characterQuest.isComplete)
                    inProgressQuests.Add(quest.DataId);
            }
            foreach (int questId in questIds)
            {
                if (inProgressQuests.Contains(questId))
                    return true;
            }
            return false;
        }
    }
}
