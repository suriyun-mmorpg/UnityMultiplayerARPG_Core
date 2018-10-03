using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public System.Action<int> onShowNpcDialog;
        public System.Action<BasePlayerCharacterEntity> onShowDealingRequestDialog;
        public System.Action<BasePlayerCharacterEntity> onShowDealingDialog;
        public System.Action<DealingState> onUpdateDealingState;
        public System.Action<DealingState> onUpdateAnotherDealingState;
        public System.Action<int> onUpdateDealingGold;
        public System.Action<int> onUpdateAnotherDealingGold;
        public System.Action<DealingCharacterItems> onUpdateDealingItems;
        public System.Action<DealingCharacterItems> onUpdateAnotherDealingItems;
        public System.Action<BasePlayerCharacterEntity> onShowPartyInvitationDialog;
        public System.Action<BasePlayerCharacterEntity> onShowGuildInvitationDialog;

        protected virtual void NetFuncSwapOrMergeItem(int fromIndex, int toIndex)
        {
            if (!CanMoveOrDoActions() ||
                fromIndex < 0 ||
                fromIndex >= NonEquipItems.Count ||
                toIndex < 0 ||
                toIndex >= NonEquipItems.Count)
                return;

            var fromItem = NonEquipItems[fromIndex];
            var toItem = NonEquipItems[toIndex];
            if (!fromItem.IsValid() || !toItem.IsValid())
                return;

            if (fromItem.dataId.Equals(toItem.dataId) && !fromItem.IsFull() && !toItem.IsFull())
            {
                // Merge if same id and not full
                short maxStack = toItem.GetMaxStack();
                if (toItem.amount + fromItem.amount <= maxStack)
                {
                    toItem.amount += fromItem.amount;
                    NonEquipItems[fromIndex] = CharacterItem.Empty;
                    NonEquipItems[toIndex] = toItem;
                }
                else
                {
                    short remains = (short)(toItem.amount + fromItem.amount - maxStack);
                    toItem.amount = maxStack;
                    fromItem.amount = remains;
                    NonEquipItems[fromIndex] = fromItem;
                    NonEquipItems[toIndex] = toItem;
                }
            }
            else
            {
                // Swap
                NonEquipItems[fromIndex] = toItem;
                NonEquipItems[toIndex] = fromItem;
            }
        }

        protected virtual void NetFuncAddAttribute(int attributeIndex, short amount)
        {
            if (IsDead() ||
                attributeIndex < 0 ||
                attributeIndex >= Attributes.Count ||
                amount <= 0 ||
                amount > StatPoint)
                return;

            var attribute = Attributes[attributeIndex];
            if (!attribute.CanIncrease(this))
                return;

            attribute.Increase(amount);
            Attributes[attributeIndex] = attribute;

            StatPoint -= amount;
        }

        protected virtual void NetFuncAddSkill(int skillIndex, short amount)
        {
            if (IsDead() ||
                skillIndex < 0 ||
                skillIndex >= Skills.Count ||
                amount <= 0 ||
                amount > SkillPoint)
                return;

            var skill = Skills[skillIndex];
            if (!skill.CanLevelUp(this))
                return;

            skill.LevelUp(amount);
            Skills[skillIndex] = skill;

            SkillPoint -= amount;
        }

        protected virtual void NetFuncRespawn()
        {
            Respawn();
        }

        protected virtual void NetFuncAssignHotkey(string hotkeyId, byte type, int dataId)
        {
            var characterHotkey = new CharacterHotkey();
            characterHotkey.hotkeyId = hotkeyId;
            characterHotkey.type = (HotkeyType)type;
            characterHotkey.dataId = dataId;
            var hotkeyIndex = this.IndexOfHotkey(hotkeyId);
            if (hotkeyIndex >= 0)
                hotkeys[hotkeyIndex] = characterHotkey;
            else
                hotkeys.Add(characterHotkey);
        }

        protected virtual void NetFuncNpcActivate(uint objectId)
        {
            if (!CanMoveOrDoActions())
                return;

            NpcEntity npcEntity = null;
            if (!TryGetEntityByObjectId(objectId, out npcEntity))
                return;

            if (Vector3.Distance(CacheTransform.position, npcEntity.CacheTransform.position) > gameInstance.conversationDistance + 5f)
                return;

            currentNpcDialog = npcEntity.StartDialog;
            if (currentNpcDialog != null)
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected virtual void NetFuncShowNpcDialog(int npcDialogDataId)
        {
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(npcDialogDataId);
        }

        protected virtual void NetFuncSelectNpcDialogMenu(int menuIndex)
        {
            if (currentNpcDialog == null)
                return;
            var menus = currentNpcDialog.menus;
            NpcDialogMenu selectedMenu;
            switch (currentNpcDialog.type)
            {
                case NpcDialogType.Normal:
                    if (menuIndex < 0 || menuIndex >= menus.Length)
                        return;
                    selectedMenu = menus[menuIndex];
                    if (!selectedMenu.IsPassConditions(this) || selectedMenu.dialog == null || selectedMenu.isCloseMenu)
                    {
                        currentNpcDialog = null;
                        RequestShowNpcDialog(0);
                        return;
                    }
                    currentNpcDialog = selectedMenu.dialog;
                    RequestShowNpcDialog(currentNpcDialog.DataId);
                    break;
                case NpcDialogType.Quest:
                    NetFuncSelectNpcDialogQuestMenu(menuIndex);
                    break;
                case NpcDialogType.CraftItem:
                    NetFuncSelectNpcDialogCraftItemMenu(menuIndex);
                    break;
            }
        }

        protected virtual void NetFuncSelectNpcDialogQuestMenu(int menuIndex)
        {
            if (currentNpcDialog == null || currentNpcDialog.type != NpcDialogType.Quest || currentNpcDialog.quest == null)
            {
                currentNpcDialog = null;
                RequestShowNpcDialog(0);
                return;
            }
            switch (menuIndex)
            {
                case NpcDialog.QUEST_ACCEPT_MENU_INDEX:
                    NetFuncAcceptQuest(currentNpcDialog.quest.DataId);
                    currentNpcDialog = currentNpcDialog.questAcceptedDialog;
                    break;
                case NpcDialog.QUEST_DECLINE_MENU_INDEX:
                    currentNpcDialog = currentNpcDialog.questDeclinedDialog;
                    break;
                case NpcDialog.QUEST_ABANDON_MENU_INDEX:
                    NetFuncAbandonQuest(currentNpcDialog.quest.DataId);
                    currentNpcDialog = currentNpcDialog.questAbandonedDialog;
                    break;
                case NpcDialog.QUEST_COMPLETE_MENU_INDEX:
                    NetFuncCompleteQuest(currentNpcDialog.quest.DataId);
                    currentNpcDialog = currentNpcDialog.questCompletedDailog;
                    break;
            }
            if (currentNpcDialog == null)
                RequestShowNpcDialog(0);
            else
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected virtual void NetFuncSelectNpcDialogCraftItemMenu(int menuIndex)
        {
            if (currentNpcDialog == null || currentNpcDialog.type != NpcDialogType.CraftItem || currentNpcDialog.itemCraft == null)
            {
                currentNpcDialog = null;
                RequestShowNpcDialog(0);
                return;
            }
            switch (menuIndex)
            {
                case NpcDialog.CRAFT_ITEM_START_MENU_INDEX:
                    if (currentNpcDialog.itemCraft.CanCraft(this))
                    {
                        currentNpcDialog.itemCraft.CraftItem(this);
                        currentNpcDialog = currentNpcDialog.craftDoneDialog;
                    }
                    else
                        currentNpcDialog = currentNpcDialog.craftNotMeetRequirementsDialog;
                    break;
                case NpcDialog.CRAFT_ITEM_CANCEL_MENU_INDEX:
                    currentNpcDialog = currentNpcDialog.craftCancelDialog;
                    break;
            }
            if (currentNpcDialog == null)
                RequestShowNpcDialog(0);
            else
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected virtual void NetFuncBuyNpcItem(int itemIndex, short amount)
        {
            if (currentNpcDialog == null)
                return;
            var sellItems = currentNpcDialog.sellItems;
            if (sellItems == null || itemIndex < 0 || itemIndex >= sellItems.Length)
                return;
            var sellItem = sellItems[itemIndex];
            if (Gold < sellItem.sellPrice * amount)
            {
                // TODO: May send not enough gold message
                return;
            }
            var dataId = sellItem.item.DataId;
            if (IncreasingItemsWillOverwhelming(dataId, amount))
            {
                // TODO: May send overwhelming message
                return;
            }
            Gold -= sellItem.sellPrice * amount;
            this.IncreaseItems(dataId, 1, amount);
        }

        protected virtual void NetFuncAcceptQuest(int questDataId)
        {
            var indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest >= 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            var characterQuest = CharacterQuest.Create(quest);
            quests.Add(characterQuest);
        }

        protected virtual void NetFuncAbandonQuest(int questDataId)
        {
            var indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            var characterQuest = quests[indexOfQuest];
            if (characterQuest.isComplete)
                return;
            quests.RemoveAt(indexOfQuest);
        }

        protected virtual void NetFuncCompleteQuest(int questDataId)
        {
            var indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            var characterQuest = quests[indexOfQuest];
            if (!characterQuest.IsAllTasksDone(this))
                return;
            if (characterQuest.isComplete)
                return;
            var tasks = quest.tasks;
            foreach (var task in tasks)
            {
                switch (task.taskType)
                {
                    case QuestTaskType.CollectItem:
                        this.DecreaseItems(task.itemAmount.item.DataId, task.itemAmount.amount);
                        break;
                }
            }
            IncreaseExp(quest.rewardExp);
            Gold += quest.rewardGold;
            var rewardItems = quest.rewardItems;
            if (rewardItems != null && rewardItems.Length > 0)
            {
                foreach (var rewardItem in rewardItems)
                {
                    if (rewardItem.item != null && rewardItem.amount > 0)
                        this.IncreaseItems(rewardItem.item.DataId, 1, rewardItem.amount);
                }
            }
            characterQuest.isComplete = true;
            if (!quest.canRepeat)
                quests[indexOfQuest] = characterQuest;
            else
                quests.RemoveAt(indexOfQuest);
        }

        protected virtual void NetFuncEnterWarp()
        {
            if (!CanMoveOrDoActions() || warpingPortal == null)
                return;
            warpingPortal.EnterWarp(this);
        }

        protected virtual void NetFuncBuild(int index, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
            if (!CanMoveOrDoActions() ||
                index < 0 ||
                index >= NonEquipItems.Count)
                return;

            BuildingEntity buildingEntity;
            var nonEquipItem = NonEquipItems[index];
            if (!nonEquipItem.IsValid() ||
                nonEquipItem.GetBuildingItem() == null ||
                nonEquipItem.GetBuildingItem().buildingEntity == null ||
                !GameInstance.BuildingEntities.TryGetValue(nonEquipItem.GetBuildingItem().buildingEntity.DataId, out buildingEntity) ||
                !this.DecreaseItemsByIndex(index, 1))
                return;

            var buildingSaveData = new BuildingSaveData();
            buildingSaveData.Id = GenericUtils.GetUniqueId();
            buildingSaveData.ParentId = string.Empty;
            LiteNetLibIdentity entity;
            if (Manager.Assets.TryGetSpawnedObject(parentObjectId, out entity))
            {
                var parentBuildingEntity = entity.GetComponent<BuildingEntity>();
                if (parentBuildingEntity != null)
                    buildingSaveData.ParentId = parentBuildingEntity.Id;
            }
            buildingSaveData.DataId = buildingEntity.DataId;
            buildingSaveData.CurrentHp = buildingEntity.maxHp;
            buildingSaveData.Position = position;
            buildingSaveData.Rotation = rotation;
            buildingSaveData.CreatorId = Id;
            buildingSaveData.CreatorName = CharacterName;
            GameManager.CreateBuildingEntity(buildingSaveData, false);

        }

        protected virtual void NetFuncDestroyBuild(uint objectId)
        {
            if (!CanMoveOrDoActions())
                return;

            BuildingEntity buildingEntity = null;
            if (!TryGetEntityByObjectId(objectId, out buildingEntity))
                return;
            
            if (buildingEntity != null && buildingEntity.CreatorId.Equals(Id))
                GameManager.DestroyBuildingEntity(buildingEntity.Id);
        }

        protected virtual void NetFuncSellItem(int index, short amount)
        {
            if (IsDead() ||
                index < 0 ||
                index >= nonEquipItems.Count)
                return;

            if (currentNpcDialog == null || currentNpcDialog.type != NpcDialogType.Shop)
                return;

            var nonEquipItem = nonEquipItems[index];
            if (!nonEquipItem.IsValid() || amount > nonEquipItem.amount)
                return;

            var item = nonEquipItem.GetItem();
            if (this.DecreaseItemsByIndex(index, amount))
                Gold += item.sellPrice * amount;
        }

        protected virtual void NetFuncRefineItem(int index)
        {
            if (IsDead() ||
                index < 0 ||
                index >= nonEquipItems.Count)
                return;

            var nonEquipItem = nonEquipItems[index];
            var equipmentItem = nonEquipItem.GetEquipmentItem();
            if (equipmentItem == null)
                return;
            if (nonEquipItem.level >= equipmentItem.MaxLevel)
            {
                // TODO: May warn that item already reached max level
                return;
            }
            var refineLevel = equipmentItem.itemRefineInfo.levels[nonEquipItem.level - 1];
            if (!refineLevel.CanRefine(this))
            {
                // TODO: may warn that cannot refine
            }
            else
            {
                // TODO: may send refine success/fail message
                refineLevel.RefineItem(this, index);
            }
        }

        #region Dealing
        protected virtual void NetFuncSendDealingRequest(uint objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity = null;
            if (!TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                // TODO: May send warn message that character is not found
                return;
            }
            if (targetCharacterEntity.CoCharacter != null)
            {
                // TODO: May send warn message that character is not available
                return;
            }
            if (Vector3.Distance(CacheTransform.position, targetCharacterEntity.CacheTransform.position) > gameInstance.conversationDistance)
            {
                // TODO: May send warn message that character is far from other character
                return;
            }
            CoCharacter = targetCharacterEntity;
            targetCharacterEntity.CoCharacter = this;
            // Send receive dealing request to player
            CoCharacter.RequestReceiveDealingRequest(ObjectId);
        }

        protected virtual void NetFuncReceiveDealingRequest(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingRequestDialog != null)
                onShowDealingRequestDialog.Invoke(playerCharacterEntity);
        }

        protected virtual void NetFuncAcceptDealingRequest()
        {
            if (CoCharacter == null)
            {
                // TODO: May send warn message that can not accept dealing request
                StopDealing();
                return;
            }
            if (Vector3.Distance(CacheTransform.position, CoCharacter.CacheTransform.position) > gameInstance.conversationDistance)
            {
                // TODO: May send warn message that character is far from other character
                StopDealing();
                return;
            }
            // Set dealing state/data for co player character entity
            CoCharacter.ClearDealingData();
            CoCharacter.DealingState = DealingState.Dealing;
            CoCharacter.RequestAcceptedDealingRequest(ObjectId);
            // Set dealing state/data for player character entity
            ClearDealingData();
            DealingState = DealingState.Dealing;
            RequestAcceptedDealingRequest(CoCharacter.ObjectId);
        }

        protected virtual void NetFuncDeclineDealingRequest()
        {
            // TODO: May send decline message
            StopDealing();
        }

        protected virtual void NetFuncAcceptedDealingRequest(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingDialog != null)
                onShowDealingDialog.Invoke(playerCharacterEntity);
        }

        protected virtual void NetFuncSetDealingItem(int itemIndex, short amount)
        {
            if (DealingState != DealingState.Dealing)
            {
                // TODO: May send warn message to start dealing before confirm
                return;
            }

            if (itemIndex < 0 || itemIndex >= nonEquipItems.Count)
                return;

            var dealingItems = DealingItems;
            for (var i = dealingItems.Count - 1; i >= 0; --i)
            {
                if (itemIndex == dealingItems[i].nonEquipIndex)
                {
                    dealingItems.RemoveAt(i);
                    break;
                }
            }
            var characterItem = nonEquipItems[itemIndex];
            var dealingItem = new DealingCharacterItem();
            dealingItem.nonEquipIndex = itemIndex;
            dealingItem.dataId = characterItem.dataId;
            dealingItem.level = characterItem.level;
            dealingItem.amount = amount;
            dealingItem.durability = characterItem.durability;
            dealingItems.Add(dealingItem);
            // Update to clients
            DealingItems = dealingItems;
        }

        protected virtual void NetFuncSetDealingGold(int gold)
        {
            if (DealingState != DealingState.Dealing)
            {
                // TODO: May send warn message to start dealing before doing this
                return;
            }
            if (gold > Gold)
                gold = Gold;
            if (gold < 0)
                gold = 0;
            DealingGold = gold;
        }

        protected virtual void NetFuncLockDealing()
        {
            if (DealingState != DealingState.Dealing)
            {
                // TODO: May send warn message to start dealing before doing this
                return;
            }
            DealingState = DealingState.LockDealing;
        }

        protected virtual void NetFuncConfirmDealing()
        {
            if (DealingState != DealingState.LockDealing || !(CoCharacter.DealingState == DealingState.LockDealing || CoCharacter.DealingState == DealingState.ConfirmDealing))
            {
                // TODO: May send warn message to lock before confirm
                return;
            }
            DealingState = DealingState.ConfirmDealing;
            if (DealingState == DealingState.ConfirmDealing && CoCharacter.DealingState == DealingState.ConfirmDealing)
            {
                ExchangeDealingItemsAndGold();
                CoCharacter.ExchangeDealingItemsAndGold();
                StopDealing();
            }
        }

        protected virtual void NetFuncCancelDealing()
        {
            // TODO: May send cancel message
            StopDealing();
        }

        protected virtual void NetFuncUpdateDealingState(DealingState state)
        {
            if (onUpdateDealingState != null)
                onUpdateDealingState.Invoke(state);
        }

        protected virtual void NetFuncUpdateAnotherDealingState(DealingState state)
        {
            if (onUpdateAnotherDealingState != null)
                onUpdateAnotherDealingState.Invoke(state);
        }

        protected virtual void NetFuncUpdateDealingGold(int gold)
        {
            if (onUpdateDealingGold != null)
                onUpdateDealingGold.Invoke(gold);
        }

        protected virtual void NetFuncUpdateAnotherDealingGold(int gold)
        {
            if (onUpdateAnotherDealingGold != null)
                onUpdateAnotherDealingGold.Invoke(gold);
        }

        protected virtual void NetFuncUpdateDealingItems(DealingCharacterItems items)
        {
            if (onUpdateDealingItems != null)
                onUpdateDealingItems.Invoke(items);
        }

        protected virtual void NetFuncUpdateAnotherDealingItems(DealingCharacterItems items)
        {
            if (onUpdateAnotherDealingItems != null)
                onUpdateAnotherDealingItems.Invoke(items);
        }
        #endregion

        #region Party
        protected virtual void NetFuncCreateParty(bool shareExp, bool shareItem)
        {
            GameManager.CreateParty(this, shareExp, shareItem);
        }

        protected virtual void NetFuncChangePartyLeader(string characterId)
        {

        }

        protected virtual void NetFuncPartySetting(bool shareExp, bool shareItem)
        {
            GameManager.PartySetting(this, shareExp, shareItem);
        }

        protected virtual void NetFuncSendPartyInvitation(uint objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity = null;
            if (!GameManager.CanSendPartyInvitation(this, objectId, out targetCharacterEntity))
                return;
            CoCharacter = targetCharacterEntity;
            targetCharacterEntity.CoCharacter = this;
            // Send receive party invitation request to player
            targetCharacterEntity.RequestReceivePartyInvitation(ObjectId);
        }

        protected virtual void NetFuncReceivePartyInvitation(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowPartyInvitationDialog != null)
                onShowPartyInvitationDialog.Invoke(playerCharacterEntity);
        }

        protected virtual void NetFuncAcceptPartyInvitation()
        {
            GameManager.AddPartyMember(CoCharacter, this);
            StopPartyInvitation();
        }

        protected virtual void NetFuncDeclinePartyInvitation()
        {
            StopPartyInvitation();
            // TODO: May send decline message
        }

        protected virtual void NetFuncKickFromParty(string characterId)
        {
            GameManager.KickFromParty(this, characterId);
        }

        protected virtual void NetFuncLeaveParty()
        {
            GameManager.LeaveParty(this);
        }
        #endregion

        #region Guild
        protected virtual void NetFuncCreateGuild(string guildName)
        {
            GameManager.CreateGuild(this, guildName);
        }

        protected virtual void NetFuncChangeGuildLeader(string characterId)
        {

        }

        protected virtual void NetFuncSetGuildMessage(string guildMessage)
        {
            GameManager.SetGuildMessage(this, guildMessage);
        }

        protected virtual void NetFuncSetGuildMemberRole(string characterId, byte role)
        {
            GameManager.SetGuildMemberRole(this, characterId, role);
        }

        protected virtual void NetFuncSendGuildInvitation(uint objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity;
            if (!GameManager.CanSendGuildInvitation(this, objectId, out targetCharacterEntity))
                return;
            CoCharacter = targetCharacterEntity;
            targetCharacterEntity.CoCharacter = this;
            // Send receive guild invitation request to player
            targetCharacterEntity.RequestReceiveGuildInvitation(ObjectId);
        }

        protected virtual void NetFuncReceiveGuildInvitation(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowGuildInvitationDialog != null)
                onShowGuildInvitationDialog.Invoke(playerCharacterEntity);
        }

        protected virtual void NetFuncAcceptGuildInvitation()
        {
            GameManager.AddGuildMember(CoCharacter, this);
            StopGuildInvitation();
        }

        protected virtual void NetFuncDeclineGuildInvitation()
        {
            StopGuildInvitation();
            // TODO: May send decline message
        }

        protected virtual void NetFuncKickFromGuild(string characterId)
        {
            GameManager.KickFromGuild(this, characterId);
        }

        protected virtual void NetFuncLeaveGuild()
        {
            GameManager.LeaveGuild(this);
        }
        #endregion

        protected virtual void StopDealing()
        {
            if (CoCharacter == null)
            {
                ClearDealingData();
                return;
            }
            // Set dealing state/data for co player character entity
            CoCharacter.ClearDealingData();
            CoCharacter.CoCharacter = null;
            // Set dealing state/data for player character entity
            ClearDealingData();
            CoCharacter = null;
        }

        protected virtual void StopPartyInvitation()
        {
            if (CoCharacter != null)
                CoCharacter.CoCharacter = null;
            CoCharacter = null;
        }

        protected virtual void StopGuildInvitation()
        {
            if (CoCharacter != null)
                CoCharacter.CoCharacter = null;
            CoCharacter = null;
        }
    }
}
