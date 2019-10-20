using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public System.Action<int> onShowNpcDialog;
        public System.Action onShowNpcRefine;
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
        public System.Action<StorageType, short, short> onShowStorage;

        protected void NetFuncSetTargetEntity(PackedUInt objectId)
        {
            if (objectId == 0)
                SetTargetEntity(null);
            BaseGameEntity tempEntity;
            if (!this.TryGetEntityByObjectId(objectId, out tempEntity))
                return;
            SetTargetEntity(tempEntity);
        }

        protected void NetFuncSwapOrMergeItem(short fromIndex, short toIndex)
        {
            if (!CanDoActions() ||
                fromIndex >= NonEquipItems.Count ||
                toIndex >= NonEquipItems.Count)
                return;

            CharacterItem fromItem = NonEquipItems[fromIndex];
            CharacterItem toItem = NonEquipItems[toIndex];

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

        protected void NetFuncAddAttribute(int dataId)
        {
            if (IsDead())
                return;

            Attribute attributeData;
            if (!GameInstance.Attributes.TryGetValue(dataId, out attributeData))
                return;

            CharacterAttribute attribute;
            int index = this.IndexOfAttribute(dataId);
            if (index < 0)
            {
                attribute = CharacterAttribute.Create(attributeData, 0);
                if (!attribute.CanIncrease(this))
                    return;
                attribute.amount += 1;
                Attributes.Add(attribute);
            }
            else
            {
                attribute = Attributes[index];
                if (!attribute.CanIncrease(this))
                    return;
                attribute.amount += 1;
                Attributes[index] = attribute;
            }

            StatPoint -= 1;
        }

        protected void NetFuncAddSkill(int dataId)
        {
            if (IsDead())
                return;

            BaseSkill skillData;
            if (!GameInstance.Skills.TryGetValue(dataId, out skillData))
                return;

            CharacterSkill skill;
            int index = this.IndexOfSkill(dataId);
            if (index < 0)
            {
                skill = CharacterSkill.Create(skillData, 0);
                if (!skill.CanLevelUp(this))
                    return;
                skill.level += 1;
                Skills.Add(skill);
            }
            else
            {
                skill = Skills[index];
                if (!skill.CanLevelUp(this))
                    return;
                skill.level += 1;
                Skills[index] = skill;
            }

            SkillPoint -= 1;
        }

        protected void NetFuncAddGuildSkill(int dataId)
        {
            if (IsDead())
                return;

            gameManager.AddGuildSkill(this, dataId);
        }

        protected void NetFuncUseGuildSkill(int dataId)
        {
            if (IsDead())
                return;

            GuildSkill guildSkill;
            if (!GameInstance.GuildSkills.TryGetValue(dataId, out guildSkill) || guildSkill.skillType != GuildSkillType.Active)
                return;

            GuildData guild;
            if (GuildId <= 0 || !gameManager.TryGetGuild(GuildId, out guild))
                return;

            short level = guild.GetSkillLevel(dataId);
            if (level <= 0)
                return;

            if (this.IndexOfSkillUsage(dataId, SkillUsageType.GuildSkill) >= 0)
                return;

            // Apply guild skill
            CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.GuildSkill, dataId);
            newSkillUsage.Use(this, level);
            skillUsages.Add(newSkillUsage);
            ApplyBuff(dataId, BuffType.GuildSkillBuff, level);
        }

        protected void NetFuncRespawn()
        {
            Respawn();
        }

        protected void NetFuncAssignHotkey(string hotkeyId, byte type, string relateId)
        {
            CharacterHotkey characterHotkey = new CharacterHotkey();
            characterHotkey.hotkeyId = hotkeyId;
            characterHotkey.type = (HotkeyType)type;
            characterHotkey.relateId = relateId;
            int hotkeyIndex = this.IndexOfHotkey(hotkeyId);
            if (hotkeyIndex >= 0)
                hotkeys[hotkeyIndex] = characterHotkey;
            else
                hotkeys.Add(characterHotkey);
        }

        protected void NetFuncNpcActivate(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            NpcEntity npcEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out npcEntity))
                return;

            if (Vector3.Distance(CacheTransform.position, npcEntity.CacheTransform.position) > gameInstance.conversationDistance + 5f)
                return;

            currentNpcDialog = npcEntity.StartDialog;
            if (currentNpcDialog != null)
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected void NetFuncShowNpcDialog(int dataId)
        {
            // Show npc dialog by dataId, if dataId = 0 it will hide
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(dataId);
        }

        protected void NetFuncShowNpcRefine()
        {
            // Hide npc dialog
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(0);

            // Show refine dialog
            if (onShowNpcRefine != null)
                onShowNpcRefine.Invoke();
        }

        protected void NetFuncSelectNpcDialogMenu(byte menuIndex)
        {
            if (currentNpcDialog == null)
                return;
            NpcDialogMenu[] menus = currentNpcDialog.menus;
            NpcDialogMenu selectedMenu;
            switch (currentNpcDialog.type)
            {
                case NpcDialogType.Normal:
                    if (menuIndex >= menus.Length)
                        return;
                    // Changing current npc dialog
                    selectedMenu = menus[menuIndex];
                    if (!selectedMenu.IsPassConditions(this) || selectedMenu.dialog == null || selectedMenu.isCloseMenu)
                    {
                        currentNpcDialog = null;
                        RequestShowNpcDialog(0);
                        return;
                    }
                    currentNpcDialog = selectedMenu.dialog;
                    // Show Npc dialog on client
                    RequestShowNpcDialog(currentNpcDialog.DataId);
                    break;
                case NpcDialogType.Quest:
                    NetFuncSelectNpcDialogQuestMenu(menuIndex);
                    break;
                case NpcDialogType.CraftItem:
                    NetFuncSelectNpcDialogCraftItemMenu(menuIndex);
                    break;
                case NpcDialogType.SaveRespawnPoint:
                    NetFuncSelectNpcDialogSaveRespawnPointMenu(menuIndex);
                    break;
                case NpcDialogType.Warp:
                    NetFuncSelectNpcDialogWarpMenu(menuIndex);
                    break;
                case NpcDialogType.PlayerStorage:
                    NetFuncSelectNpcDialogPlayerStorageMenu(menuIndex);
                    break;
                case NpcDialogType.GuildStorage:
                    NetFuncSelectNpcDialogGuildStorageMenu(menuIndex);
                    break;
            }

            // `currentNpcDialog` have changed after select menu, then proceed new dialog activity if needed
            if (currentNpcDialog != null)
            {
                switch (currentNpcDialog.type)
                {
                    case NpcDialogType.RefineItem:
                        // If dialog is refine dialog, show refine dialog at client
                        RequestShowNpcRefine();
                        currentNpcDialog = null;
                        break;
                }
            }
        }

        protected void NetFuncSelectNpcDialogQuestMenu(int menuIndex)
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
                    currentNpcDialog = currentNpcDialog.questCompletedDialog;
                    break;
            }
            if (currentNpcDialog == null)
                RequestShowNpcDialog(0);
            else
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected void NetFuncSelectNpcDialogCraftItemMenu(int menuIndex)
        {
            if (currentNpcDialog == null || currentNpcDialog.type != NpcDialogType.CraftItem || currentNpcDialog.itemCraft.CraftingItem == null)
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

        protected void NetFuncSelectNpcDialogSaveRespawnPointMenu(int menuIndex)
        {
            if (currentNpcDialog == null ||
                currentNpcDialog.type != NpcDialogType.SaveRespawnPoint)
            {
                currentNpcDialog = null;
                RequestShowNpcDialog(0);
                return;
            }
            switch (menuIndex)
            {
                case NpcDialog.SAVE_SPAWN_POINT_CONFIRM_MENU_INDEX:
                    RespawnMapName = currentNpcDialog.saveRespawnMap.Id;
                    RespawnPosition = currentNpcDialog.saveRespawnPosition;
                    currentNpcDialog = currentNpcDialog.saveRespawnConfirmDialog;
                    break;
                case NpcDialog.SAVE_SPAWN_POINT_CANCEL_MENU_INDEX:
                    currentNpcDialog = currentNpcDialog.saveRespawnCancelDialog;
                    break;
            }
            if (currentNpcDialog == null)
                RequestShowNpcDialog(0);
            else
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected void NetFuncSelectNpcDialogWarpMenu(int menuIndex)
        {
            if (currentNpcDialog == null ||
                currentNpcDialog.type != NpcDialogType.Warp)
            {
                currentNpcDialog = null;
                RequestShowNpcDialog(0);
                return;
            }
            switch (menuIndex)
            {
                case NpcDialog.WARP_CONFIRM_MENU_INDEX:
                    gameManager.WarpCharacter(currentNpcDialog.warpPortalType, this, currentNpcDialog.warpMap.Id, currentNpcDialog.warpPosition);
                    currentNpcDialog = null;
                    break;
                case NpcDialog.WARP_CANCEL_MENU_INDEX:
                    currentNpcDialog = currentNpcDialog.warpCancelDialog;
                    break;
            }
            if (currentNpcDialog == null)
                RequestShowNpcDialog(0);
            else
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected void NetFuncSelectNpcDialogPlayerStorageMenu(int menuIndex)
        {
            if (currentNpcDialog == null ||
                currentNpcDialog.type != NpcDialogType.PlayerStorage)
            {
                currentNpcDialog = null;
                RequestShowNpcDialog(0);
                return;
            }
            switch (menuIndex)
            {
                case NpcDialog.STORAGE_CONFIRM_MENU_INDEX:
                    OpenStorage(StorageType.Player, UserId);
                    currentNpcDialog = null;
                    break;
                case NpcDialog.STORAGE_CANCEL_MENU_INDEX:
                    currentNpcDialog = currentNpcDialog.storageCancelDialog;
                    break;
            }
            if (currentNpcDialog == null)
                RequestShowNpcDialog(0);
            else
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected void NetFuncSelectNpcDialogGuildStorageMenu(int menuIndex)
        {
            if (currentNpcDialog == null ||
                currentNpcDialog.type != NpcDialogType.GuildStorage)
            {
                currentNpcDialog = null;
                RequestShowNpcDialog(0);
                return;
            }
            switch (menuIndex)
            {
                case NpcDialog.STORAGE_CONFIRM_MENU_INDEX:
                    OpenStorage(StorageType.Guild, GuildId.ToString());
                    currentNpcDialog = null;
                    break;
                case NpcDialog.STORAGE_CANCEL_MENU_INDEX:
                    currentNpcDialog = currentNpcDialog.storageCancelDialog;
                    break;
            }
            if (currentNpcDialog == null)
                RequestShowNpcDialog(0);
            else
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected void NetFuncBuyNpcItem(short itemIndex, short amount)
        {
            if (currentNpcDialog == null)
                return;
            NpcSellItem[] sellItems = currentNpcDialog.sellItems;
            if (sellItems == null || itemIndex >= sellItems.Length)
                return;
            NpcSellItem sellItem = sellItems[itemIndex];
            if (!gameplayRule.CurrenciesEnoughToBuyItem(this, sellItem, amount))
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotEnoughGold);
                return;
            }
            int dataId = sellItem.item.DataId;
            if (this.IncreasingItemsWillOverwhelming(dataId, amount))
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
                return;
            }
            gameplayRule.DecreaseCurrenciesWhenBuyItem(this, sellItem, amount);
            this.IncreaseItems(CharacterItem.Create(dataId, 1, amount));
        }

        protected void NetFuncAcceptQuest(int questDataId)
        {
            int indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest >= 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            CharacterQuest characterQuest = CharacterQuest.Create(quest);
            quests.Add(characterQuest);
        }

        protected void NetFuncAbandonQuest(int questDataId)
        {
            int indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            CharacterQuest characterQuest = quests[indexOfQuest];
            if (characterQuest.isComplete)
                return;
            quests.RemoveAt(indexOfQuest);
        }

        protected void NetFuncCompleteQuest(int questDataId)
        {
            int indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            CharacterQuest characterQuest = quests[indexOfQuest];
            if (!characterQuest.IsAllTasksDone(this))
                return;
            if (characterQuest.isComplete)
                return;
            QuestTask[] tasks = quest.tasks;
            foreach (QuestTask task in tasks)
            {
                switch (task.taskType)
                {
                    case QuestTaskType.CollectItem:
                        this.DecreaseItems(task.itemAmount.item.DataId, task.itemAmount.amount);
                        break;
                }
            }
            Reward reward = gameplayRule.MakeQuestReward(quest);
            RewardExp(reward, 1f, RewardGivenType.Quest);
            RewardCurrencies(reward, 1f, RewardGivenType.Quest);
            ItemAmount[] rewardItems = quest.rewardItems;
            if (rewardItems != null && rewardItems.Length > 0)
            {
                foreach (ItemAmount rewardItem in rewardItems)
                {
                    if (rewardItem.item != null && rewardItem.amount > 0)
                        this.IncreaseItems(CharacterItem.Create(rewardItem.item, 1, rewardItem.amount));
                }
            }
            characterQuest.isComplete = true;
            if (!quest.canRepeat)
                quests[indexOfQuest] = characterQuest;
            else
                quests.RemoveAt(indexOfQuest);
        }

        protected void NetFuncEnterWarp()
        {
            if (!CanDoActions() || warpingPortal == null)
                return;
            warpingPortal.EnterWarp(this);
        }

        protected void NetFuncBuild(short itemIndex, Vector3 position, Quaternion rotation, PackedUInt parentObjectId)
        {
            if (!CanDoActions() ||
                itemIndex >= NonEquipItems.Count)
                return;

            BuildingEntity buildingEntity;
            CharacterItem nonEquipItem = NonEquipItems[itemIndex];
            if (nonEquipItem.IsEmptySlot() ||
                nonEquipItem.GetBuildingItem() == null ||
                nonEquipItem.GetBuildingItem().buildingEntity == null ||
                !GameInstance.BuildingEntities.TryGetValue(nonEquipItem.GetBuildingItem().buildingEntity.DataId, out buildingEntity) ||
                !this.DecreaseItemsByIndex(itemIndex, 1))
                return;

            BuildingSaveData buildingSaveData = new BuildingSaveData();
            buildingSaveData.Id = GenericUtils.GetUniqueId();
            buildingSaveData.ParentId = string.Empty;
            BuildingEntity parentBuildingEntity;
            if (this.TryGetEntityByObjectId(parentObjectId, out parentBuildingEntity))
                buildingSaveData.ParentId = parentBuildingEntity.Id;
            buildingSaveData.DataId = buildingEntity.DataId;
            buildingSaveData.CurrentHp = buildingEntity.maxHp;
            buildingSaveData.Position = position;
            buildingSaveData.Rotation = rotation;
            buildingSaveData.CreatorId = Id;
            buildingSaveData.CreatorName = CharacterName;
            gameManager.CreateBuildingEntity(buildingSaveData, false);

        }

        protected void NetFuncDestroyBuilding(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out buildingEntity))
                return;

            // TODO: For now only creator can destroy building
            if (buildingEntity != null && buildingEntity.CreatorId.Equals(Id))
                gameManager.DestroyBuildingEntity(buildingEntity.Id);
        }

        protected void NetFuncOpenBuildingStorage(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out buildingEntity))
                return;

            // TODO: For now only creator can open storage
            if (buildingEntity != null &&
                buildingEntity.CreatorId.Equals(Id) &&
                buildingEntity is StorageEntity)
                OpenStorage(StorageType.Building, buildingEntity.Id);
        }

        protected void NetFuncShowStorage(byte byteStorageType, short weightLimit, short slotLimit)
        {
            if (onShowStorage != null)
                onShowStorage.Invoke((StorageType)byteStorageType, weightLimit, slotLimit);
        }

        protected void NetFuncSellItem(short index, short amount)
        {
            if (IsDead() ||
                index >= nonEquipItems.Count)
                return;

            if (currentNpcDialog == null || currentNpcDialog.type != NpcDialogType.Shop)
                return;

            CharacterItem nonEquipItem = nonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
                return;

            Item item = nonEquipItem.GetItem();
            if (this.DecreaseItemsByIndex(index, amount))
                gameplayRule.IncreaseCurrenciesWhenSellItem(this, item, amount);
        }

        protected void NetFuncRefineItem(byte byteInventoryType, short index)
        {
            if (IsDead())
                return;

            GameMessage.Type gameMessageType;
            switch ((InventoryType)byteInventoryType)
            {
                case InventoryType.NonEquipItems:
                    Item.RefineNonEquipItem(this, index, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipItems:
                    Item.RefineEquipItem(this, index, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponRight:
                    Item.RefineRightHandItem(this, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponLeft:
                    Item.RefineLeftHandItem(this, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
            }
        }

        protected void NetFuncEnhanceSocketItem(byte byteInventoryType, short index, int enhancerId)
        {
            if (IsDead())
                return;

            GameMessage.Type gameMessageType;
            switch ((InventoryType)byteInventoryType)
            {
                case InventoryType.NonEquipItems:
                    Item.EnhanceSocketNonEquipItem(this, index, enhancerId, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipItems:
                    Item.EnhanceSocketEquipItem(this, index, enhancerId, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponRight:
                    Item.EnhanceSocketRightHandItem(this, enhancerId, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponLeft:
                    Item.EnhanceSocketLeftHandItem(this, enhancerId, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
            }
        }

        protected void NetFuncRepairItem(byte byteInventoryType, short index)
        {
            if (IsDead())
                return;

            GameMessage.Type gameMessageType;
            switch ((InventoryType)byteInventoryType)
            {
                case InventoryType.NonEquipItems:
                    Item.RepairNonEquipItem(this, index, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipItems:
                    Item.RepairEquipItem(this, index, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponRight:
                    Item.RepairRightHandItem(this, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponLeft:
                    Item.RepairLeftHandItem(this, out gameMessageType);
                    gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
            }
        }

        #region Dealing
        protected void NetFuncSendDealingRequest(PackedUInt objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotFoundCharacter);
                return;
            }
            if (targetCharacterEntity.DealingCharacter != null)
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsInAnotherDeal);
                return;
            }
            if (Vector3.Distance(CacheTransform.position, targetCharacterEntity.CacheTransform.position) > gameInstance.conversationDistance)
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.DealingCharacter = this;
            // Send receive dealing request to player
            DealingCharacter.RequestReceiveDealingRequest(ObjectId);
        }

        protected void NetFuncReceiveDealingRequest(PackedUInt objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingRequestDialog != null)
                onShowDealingRequestDialog.Invoke(playerCharacterEntity);
        }

        protected void NetFuncAcceptDealingRequest()
        {
            if (DealingCharacter == null)
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotAcceptDealingRequest);
                StopDealing();
                return;
            }
            if (Vector3.Distance(CacheTransform.position, DealingCharacter.CacheTransform.position) > gameInstance.conversationDistance)
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                StopDealing();
                return;
            }
            // Set dealing state/data for co player character entity
            DealingCharacter.ClearDealingData();
            DealingCharacter.DealingState = DealingState.Dealing;
            DealingCharacter.RequestAcceptedDealingRequest(ObjectId);
            // Set dealing state/data for player character entity
            ClearDealingData();
            DealingState = DealingState.Dealing;
            RequestAcceptedDealingRequest(DealingCharacter.ObjectId);
        }

        protected void NetFuncDeclineDealingRequest()
        {
            if (DealingCharacter != null)
                gameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.DealingRequestDeclined);
            gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.DealingRequestDeclined);
            StopDealing();
        }

        protected void NetFuncAcceptedDealingRequest(PackedUInt objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingDialog != null)
                onShowDealingDialog.Invoke(playerCharacterEntity);
        }

        protected void NetFuncSetDealingItem(short itemIndex, short amount)
        {
            if (DealingState != DealingState.Dealing)
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
                return;
            }

            if (itemIndex >= nonEquipItems.Count)
                return;

            DealingCharacterItems dealingItems = DealingItems;
            for (int i = dealingItems.Count - 1; i >= 0; --i)
            {
                if (itemIndex == dealingItems[i].nonEquipIndex)
                {
                    dealingItems.RemoveAt(i);
                    break;
                }
            }
            CharacterItem characterItem = nonEquipItems[itemIndex].Clone();
            characterItem.amount = amount;
            DealingCharacterItem dealingItem = new DealingCharacterItem();
            dealingItem.nonEquipIndex = itemIndex;
            dealingItem.characterItem = characterItem;
            dealingItems.Add(dealingItem);
            // Update to clients
            DealingItems = dealingItems;
        }

        protected void NetFuncSetDealingGold(int gold)
        {
            if (DealingState != DealingState.Dealing)
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
                return;
            }
            if (gold > Gold)
                gold = Gold;
            if (gold < 0)
                gold = 0;
            DealingGold = gold;
        }

        protected void NetFuncLockDealing()
        {
            if (DealingState != DealingState.Dealing)
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
                return;
            }
            DealingState = DealingState.LockDealing;
        }

        protected void NetFuncConfirmDealing()
        {
            if (DealingState != DealingState.LockDealing || !(DealingCharacter.DealingState == DealingState.LockDealing || DealingCharacter.DealingState == DealingState.ConfirmDealing))
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
                return;
            }
            DealingState = DealingState.ConfirmDealing;
            if (DealingState == DealingState.ConfirmDealing && DealingCharacter.DealingState == DealingState.ConfirmDealing)
            {
                ExchangeDealingItemsAndGold();
                DealingCharacter.ExchangeDealingItemsAndGold();
                StopDealing();
            }
        }

        protected void NetFuncCancelDealing()
        {
            if (DealingCharacter != null)
                gameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.DealingCanceled);
            gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.DealingCanceled);
            StopDealing();
        }

        protected void NetFuncUpdateDealingState(byte byteDealingState)
        {
            if (onUpdateDealingState != null)
                onUpdateDealingState.Invoke((DealingState)byteDealingState);
        }

        protected void NetFuncUpdateAnotherDealingState(byte byteDealingState)
        {
            if (onUpdateAnotherDealingState != null)
                onUpdateAnotherDealingState.Invoke((DealingState)byteDealingState);
        }

        protected void NetFuncUpdateDealingGold(int gold)
        {
            if (onUpdateDealingGold != null)
                onUpdateDealingGold.Invoke(gold);
        }

        protected void NetFuncUpdateAnotherDealingGold(int gold)
        {
            if (onUpdateAnotherDealingGold != null)
                onUpdateAnotherDealingGold.Invoke(gold);
        }

        protected void NetFuncUpdateDealingItems(DealingCharacterItems items)
        {
            if (onUpdateDealingItems != null)
                onUpdateDealingItems.Invoke(items);
        }

        protected void NetFuncUpdateAnotherDealingItems(DealingCharacterItems items)
        {
            if (onUpdateAnotherDealingItems != null)
                onUpdateAnotherDealingItems.Invoke(items);
        }
        #endregion

        #region Party
        protected void NetFuncCreateParty(bool shareExp, bool shareItem)
        {
            gameManager.CreateParty(this, shareExp, shareItem);
        }

        protected void NetFuncChangePartyLeader(string characterId)
        {
            gameManager.ChangePartyLeader(this, characterId);
        }

        protected void NetFuncPartySetting(bool shareExp, bool shareItem)
        {
            gameManager.PartySetting(this, shareExp, shareItem);
        }

        protected void NetFuncSendPartyInvitation(PackedUInt objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity = null;
            if (!gameManager.CanSendPartyInvitation(this, objectId, out targetCharacterEntity))
                return;
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.DealingCharacter = this;
            // Send receive party invitation request to player
            targetCharacterEntity.RequestReceivePartyInvitation(ObjectId);
        }

        protected void NetFuncReceivePartyInvitation(PackedUInt objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowPartyInvitationDialog != null)
                onShowPartyInvitationDialog.Invoke(playerCharacterEntity);
        }

        protected void NetFuncAcceptPartyInvitation()
        {
            gameManager.AddPartyMember(DealingCharacter, this);
            StopPartyInvitation();
        }

        protected void NetFuncDeclinePartyInvitation()
        {
            if (DealingCharacter != null)
                gameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.PartyInvitationDeclined);
            gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.PartyInvitationDeclined);
            StopPartyInvitation();
        }

        protected void NetFuncKickFromParty(string characterId)
        {
            gameManager.KickFromParty(this, characterId);
        }

        protected void NetFuncLeaveParty()
        {
            gameManager.LeaveParty(this);
        }
        #endregion

        #region Guild
        protected void NetFuncCreateGuild(string guildName)
        {
            gameManager.CreateGuild(this, guildName);
        }

        protected void NetFuncChangeGuildLeader(string characterId)
        {
            gameManager.ChangeGuildLeader(this, characterId);
        }

        protected void NetFuncSetGuildMessage(string guildMessage)
        {
            gameManager.SetGuildMessage(this, guildMessage);
        }

        protected void NetFuncSetGuildRole(byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            gameManager.SetGuildRole(this, guildRole, name, canInvite, canKick, shareExpPercentage);
        }

        protected void NetFuncSetGuildMemberRole(string characterId, byte guildRole)
        {
            gameManager.SetGuildMemberRole(this, characterId, guildRole);
        }

        protected void NetFuncSendGuildInvitation(PackedUInt objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity;
            if (!gameManager.CanSendGuildInvitation(this, objectId, out targetCharacterEntity))
                return;
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.DealingCharacter = this;
            // Send receive guild invitation request to player
            targetCharacterEntity.RequestReceiveGuildInvitation(ObjectId);
        }

        protected void NetFuncReceiveGuildInvitation(PackedUInt objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowGuildInvitationDialog != null)
                onShowGuildInvitationDialog.Invoke(playerCharacterEntity);
        }

        protected void NetFuncAcceptGuildInvitation()
        {
            gameManager.AddGuildMember(DealingCharacter, this);
            StopGuildInvitation();
        }

        protected void NetFuncDeclineGuildInvitation()
        {
            if (DealingCharacter != null)
                gameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.GuildInvitationDeclined);
            gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.GuildInvitationDeclined);
            StopGuildInvitation();
        }

        protected void NetFuncKickFromGuild(string characterId)
        {
            gameManager.KickFromGuild(this, characterId);
        }

        protected void NetFuncLeaveGuild()
        {
            gameManager.LeaveGuild(this);
        }
        #endregion

        #region Storage
        protected void NetFuncMoveItemToStorage(short nonEquipIndex, short amount, short storageItemIndex)
        {
            if (IsDead() ||
                nonEquipIndex >= nonEquipItems.Count)
                return;

            gameManager.MoveItemToStorage(this, currentStorageId, nonEquipIndex, amount, storageItemIndex);
        }

        protected void NetFuncMoveItemFromStorage(short storageItemIndex, short amount, short nonEquipIndex)
        {
            if (IsDead() ||
                storageItemIndex >= storageItems.Count)
                return;

            gameManager.MoveItemFromStorage(this, currentStorageId, storageItemIndex, amount, nonEquipIndex);
        }

        protected void NetFuncSwapOrMergeStorageItem(short fromIndex, short toIndex)
        {
            if (IsDead() ||
                fromIndex >= storageItems.Count ||
                toIndex >= storageItems.Count)
                return;

            gameManager.SwapOrMergeStorageItem(this, currentStorageId, fromIndex, toIndex);
        }
        #endregion

        #region Banking
        protected void NetFuncDepositGold(int amount)
        {
            gameManager.DepositGold(this, amount);
        }

        protected void NetFuncWithdrawGold(int amount)
        {
            gameManager.WithdrawGold(this, amount);
        }

        protected void NetFuncDepositGuildGold(int amount)
        {
            gameManager.DepositGuildGold(this, amount);
        }

        protected void NetFuncWithdrawGuildGold(int amount)
        {
            gameManager.WithdrawGuildGold(this, amount);
        }

        protected void NetFuncOpenStorage(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            StorageEntity storageEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out storageEntity))
                return;

            if (Vector3.Distance(CacheTransform.position, storageEntity.CacheTransform.position) > gameInstance.conversationDistance + 5f)
                return;

            OpenStorage(StorageType.Building, storageEntity.Id);
        }

        protected void NetFuncCloseStorage()
        {
            gameManager.CloseStorage(this);
            currentStorageId = StorageId.Empty;
        }
        #endregion

        #region Building Entities
        protected void NetFuncToggleDoor(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            DoorEntity doorEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out doorEntity))
                return;

            if (Vector3.Distance(CacheTransform.position, doorEntity.CacheTransform.position) > gameInstance.conversationDistance + 5f)
                return;

            doorEntity.IsOpen = !doorEntity.IsOpen;
        }

        protected void NetFuncCraftItemByWorkbench(PackedUInt objectId, int dataId)
        {
            if (!CanDoActions())
                return;

            WorkbenchEntity workbenchEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out workbenchEntity))
                return;

            if (Vector3.Distance(CacheTransform.position, workbenchEntity.CacheTransform.position) > gameInstance.conversationDistance + 5f)
                return;

            workbenchEntity.CraftItem(this, dataId);
        }
        #endregion

        #region Social
        protected void NetFuncFindCharacters(string characterName)
        {
            gameManager.FindCharacters(this, characterName);
        }

        protected void NetFuncAddFriend(string friendCharacterId)
        {
            gameManager.AddFriend(this, friendCharacterId);
        }

        protected void NetFuncRemoveFriend(string friendCharacterId)
        {
            gameManager.RemoveFriend(this, friendCharacterId);
        }

        protected void NetFuncGetFriends()
        {
            gameManager.GetFriends(this);
        }
        #endregion

        protected void StopDealing()
        {
            if (DealingCharacter == null)
            {
                ClearDealingData();
                return;
            }
            // Set dealing state/data for co player character entity
            DealingCharacter.ClearDealingData();
            DealingCharacter.DealingCharacter = null;
            // Set dealing state/data for player character entity
            ClearDealingData();
            DealingCharacter = null;
        }

        protected void StopPartyInvitation()
        {
            if (DealingCharacter != null)
                DealingCharacter.DealingCharacter = null;
            DealingCharacter = null;
        }

        protected void StopGuildInvitation()
        {
            if (DealingCharacter != null)
                DealingCharacter.DealingCharacter = null;
            DealingCharacter = null;
        }

        protected void OpenStorage(StorageType storageType, string ownerId)
        {
            StorageId storageId = new StorageId(storageType, ownerId);
            if (!gameManager.CanAccessStorage(this, storageId))
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            Storage storage = gameManager.GetStorage(storageId);
            if (!currentStorageId.Equals(storageId))
            {
                gameManager.CloseStorage(this);
                currentStorageId = storageId;
                gameManager.OpenStorage(this);
                CallNetFunction(NetFuncShowStorage, ConnectionId, (byte)storageType, storage.weightLimit, storage.slotLimit);
            }
        }
    }
}
