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

            GameMessage.Type gameMessageType;
            if (this.AddAttribute(out gameMessageType, dataId))
                StatPoint -= 1;
            else
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
        }

        protected void NetFuncAddSkill(int dataId)
        {
            if (IsDead())
                return;

            GameMessage.Type gameMessageType;
            if (this.AddSkill(out gameMessageType, dataId))
                SkillPoint -= 1;
            else
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
        }

        protected void NetFuncAddGuildSkill(int dataId)
        {
            if (IsDead())
                return;

            CurrentGameManager.AddGuildSkill(this, dataId);
        }

        protected void NetFuncUseGuildSkill(int dataId)
        {
            if (IsDead())
                return;

            GuildSkill guildSkill;
            if (!GameInstance.GuildSkills.TryGetValue(dataId, out guildSkill) || guildSkill.GetSkillType() != GuildSkillType.Active)
                return;

            GuildData guild;
            if (GuildId <= 0 || !CurrentGameManager.TryGetGuild(GuildId, out guild))
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
            ApplyBuff(dataId, BuffType.GuildSkillBuff, level, this);
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

        protected void NetFuncEnterWarp()
        {
            if (!CanDoActions() || WarpingPortal == null)
                return;
            WarpingPortal.EnterWarp(this);
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
            CurrentGameManager.CreateBuildingEntity(buildingSaveData, false);

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
                CurrentGameManager.DestroyBuildingEntity(buildingEntity.Id);
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

            if (CurrentNpcDialog == null || CurrentNpcDialog.type != NpcDialogType.Shop)
                return;

            CharacterItem nonEquipItem = nonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
                return;

            Item item = nonEquipItem.GetItem();
            if (this.DecreaseItemsByIndex(index, amount))
                CurrentGameplayRule.IncreaseCurrenciesWhenSellItem(this, item, amount);
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
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipItems:
                    Item.RefineEquipItem(this, index, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponRight:
                    Item.RefineRightHandItem(this, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponLeft:
                    Item.RefineLeftHandItem(this, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
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
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipItems:
                    Item.EnhanceSocketEquipItem(this, index, enhancerId, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponRight:
                    Item.EnhanceSocketRightHandItem(this, enhancerId, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponLeft:
                    Item.EnhanceSocketLeftHandItem(this, enhancerId, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
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
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipItems:
                    Item.RepairEquipItem(this, index, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponRight:
                    Item.RepairRightHandItem(this, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponLeft:
                    Item.RepairLeftHandItem(this, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
            }
        }

        #region Dealing
        protected void NetFuncSendDealingRequest(PackedUInt objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotFoundCharacter);
                return;
            }
            if (targetCharacterEntity.DealingCharacter != null)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsInAnotherDeal);
                return;
            }
            if (Vector3.Distance(CacheTransform.position, targetCharacterEntity.CacheTransform.position) > CurrentGameInstance.conversationDistance)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
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
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotAcceptDealingRequest);
                StopDealing();
                return;
            }
            if (Vector3.Distance(CacheTransform.position, DealingCharacter.CacheTransform.position) > CurrentGameInstance.conversationDistance)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
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
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.DealingRequestDeclined);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.DealingRequestDeclined);
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
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
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
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
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
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
                return;
            }
            DealingState = DealingState.LockDealing;
        }

        protected void NetFuncConfirmDealing()
        {
            if (DealingState != DealingState.LockDealing || !(DealingCharacter.DealingState == DealingState.LockDealing || DealingCharacter.DealingState == DealingState.ConfirmDealing))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
                return;
            }
            DealingState = DealingState.ConfirmDealing;
            if (DealingState == DealingState.ConfirmDealing && DealingCharacter.DealingState == DealingState.ConfirmDealing)
            {
                if (ExchangingDealingItemsWillOverwhelming())
                {
                    CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.AnotherCharacterCannotCarryAnymore);
                    CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.CannotCarryAnymore);
                }
                else if (DealingCharacter.ExchangingDealingItemsWillOverwhelming())
                {
                    CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
                    CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.AnotherCharacterCannotCarryAnymore);
                }
                else
                {
                    ExchangeDealingItemsAndGold();
                    DealingCharacter.ExchangeDealingItemsAndGold();
                }
                StopDealing();
            }
        }

        protected void NetFuncCancelDealing()
        {
            if (DealingCharacter != null)
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.DealingCanceled);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.DealingCanceled);
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
            CurrentGameManager.CreateParty(this, shareExp, shareItem);
        }

        protected void NetFuncChangePartyLeader(string characterId)
        {
            CurrentGameManager.ChangePartyLeader(this, characterId);
        }

        protected void NetFuncPartySetting(bool shareExp, bool shareItem)
        {
            CurrentGameManager.PartySetting(this, shareExp, shareItem);
        }

        protected void NetFuncSendPartyInvitation(PackedUInt objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity = null;
            if (!CurrentGameManager.CanSendPartyInvitation(this, objectId, out targetCharacterEntity))
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
            CurrentGameManager.AddPartyMember(DealingCharacter, this);
            StopPartyInvitation();
        }

        protected void NetFuncDeclinePartyInvitation()
        {
            if (DealingCharacter != null)
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.PartyInvitationDeclined);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.PartyInvitationDeclined);
            StopPartyInvitation();
        }

        protected void NetFuncKickFromParty(string characterId)
        {
            CurrentGameManager.KickFromParty(this, characterId);
        }

        protected void NetFuncLeaveParty()
        {
            CurrentGameManager.LeaveParty(this);
        }
        #endregion

        #region Guild
        protected void NetFuncCreateGuild(string guildName)
        {
            CurrentGameManager.CreateGuild(this, guildName);
        }

        protected void NetFuncChangeGuildLeader(string characterId)
        {
            CurrentGameManager.ChangeGuildLeader(this, characterId);
        }

        protected void NetFuncSetGuildMessage(string guildMessage)
        {
            CurrentGameManager.SetGuildMessage(this, guildMessage);
        }

        protected void NetFuncSetGuildRole(byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            CurrentGameManager.SetGuildRole(this, guildRole, name, canInvite, canKick, shareExpPercentage);
        }

        protected void NetFuncSetGuildMemberRole(string characterId, byte guildRole)
        {
            CurrentGameManager.SetGuildMemberRole(this, characterId, guildRole);
        }

        protected void NetFuncSendGuildInvitation(PackedUInt objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity;
            if (!CurrentGameManager.CanSendGuildInvitation(this, objectId, out targetCharacterEntity))
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
            CurrentGameManager.AddGuildMember(DealingCharacter, this);
            StopGuildInvitation();
        }

        protected void NetFuncDeclineGuildInvitation()
        {
            if (DealingCharacter != null)
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.GuildInvitationDeclined);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.GuildInvitationDeclined);
            StopGuildInvitation();
        }

        protected void NetFuncKickFromGuild(string characterId)
        {
            CurrentGameManager.KickFromGuild(this, characterId);
        }

        protected void NetFuncLeaveGuild()
        {
            CurrentGameManager.LeaveGuild(this);
        }
        #endregion

        #region Storage
        protected void NetFuncMoveItemToStorage(short nonEquipIndex, short amount, short storageItemIndex)
        {
            if (IsDead() ||
                nonEquipIndex >= nonEquipItems.Count)
                return;

            CurrentGameManager.MoveItemToStorage(this, currentStorageId, nonEquipIndex, amount, storageItemIndex);
        }

        protected void NetFuncMoveItemFromStorage(short storageItemIndex, short amount, short nonEquipIndex)
        {
            if (IsDead() ||
                storageItemIndex >= storageItems.Count)
                return;

            CurrentGameManager.MoveItemFromStorage(this, currentStorageId, storageItemIndex, amount, nonEquipIndex);
        }

        protected void NetFuncSwapOrMergeStorageItem(short fromIndex, short toIndex)
        {
            if (IsDead() ||
                fromIndex >= storageItems.Count ||
                toIndex >= storageItems.Count)
                return;

            CurrentGameManager.SwapOrMergeStorageItem(this, currentStorageId, fromIndex, toIndex);
        }
        #endregion

        #region Banking
        protected void NetFuncDepositGold(int amount)
        {
            CurrentGameManager.DepositGold(this, amount);
        }

        protected void NetFuncWithdrawGold(int amount)
        {
            CurrentGameManager.WithdrawGold(this, amount);
        }

        protected void NetFuncDepositGuildGold(int amount)
        {
            CurrentGameManager.DepositGuildGold(this, amount);
        }

        protected void NetFuncWithdrawGuildGold(int amount)
        {
            CurrentGameManager.WithdrawGuildGold(this, amount);
        }

        protected void NetFuncOpenStorage(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            StorageEntity storageEntity = null;
            if (!this.TryGetEntityByObjectId(objectId, out storageEntity))
                return;

            if (Vector3.Distance(CacheTransform.position, storageEntity.CacheTransform.position) > CurrentGameInstance.conversationDistance + 5f)
                return;

            OpenStorage(StorageType.Building, storageEntity.Id);
        }

        protected void NetFuncCloseStorage()
        {
            CurrentGameManager.CloseStorage(this);
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

            if (Vector3.Distance(CacheTransform.position, doorEntity.CacheTransform.position) > CurrentGameInstance.conversationDistance + 5f)
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

            if (Vector3.Distance(CacheTransform.position, workbenchEntity.CacheTransform.position) > CurrentGameInstance.conversationDistance + 5f)
                return;

            workbenchEntity.CraftItem(this, dataId);
        }
        #endregion

        #region Social
        protected void NetFuncFindCharacters(string characterName)
        {
            CurrentGameManager.FindCharacters(this, characterName);
        }

        protected void NetFuncAddFriend(string friendCharacterId)
        {
            CurrentGameManager.AddFriend(this, friendCharacterId);
        }

        protected void NetFuncRemoveFriend(string friendCharacterId)
        {
            CurrentGameManager.RemoveFriend(this, friendCharacterId);
        }

        protected void NetFuncGetFriends()
        {
            CurrentGameManager.GetFriends(this);
        }
        #endregion

        public void StopDealing()
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

        public void StopPartyInvitation()
        {
            if (DealingCharacter != null)
                DealingCharacter.DealingCharacter = null;
            DealingCharacter = null;
        }

        public void StopGuildInvitation()
        {
            if (DealingCharacter != null)
                DealingCharacter.DealingCharacter = null;
            DealingCharacter = null;
        }

        public void OpenStorage(StorageType storageType, string ownerId)
        {
            StorageId storageId = new StorageId(storageType, ownerId);
            if (!CurrentGameManager.CanAccessStorage(this, storageId))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            Storage storage = CurrentGameManager.GetStorage(storageId);
            if (!currentStorageId.Equals(storageId))
            {
                CurrentGameManager.CloseStorage(this);
                currentStorageId = storageId;
                CurrentGameManager.OpenStorage(this);
                CallNetFunction(NetFuncShowStorage, ConnectionId, (byte)storageType, storage.weightLimit, storage.slotLimit);
            }
        }
    }
}
