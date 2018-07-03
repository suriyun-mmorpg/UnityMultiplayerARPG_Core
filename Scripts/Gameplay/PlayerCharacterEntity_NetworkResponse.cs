using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterEntity
    {
        public System.Action<int> onShowNpcDialog;

        protected void NetFuncSwapOrMergeItem(int fromIndex, int toIndex)
        {
            if (IsDead() ||
                IsPlayingActionAnimation() ||
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

        protected void NetFuncAddAttribute(int attributeIndex, short amount)
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

        protected void NetFuncAddSkill(int skillIndex, short amount)
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

        protected void NetFuncRespawn()
        {
            Respawn();
        }

        protected void NetFuncAssignHotkey(string hotkeyId, byte type, int dataId)
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

        protected void NetFuncNpcActivate(uint objectId)
        {
            if (IsDead() || IsPlayingActionAnimation())
                return;

            LiteNetLibIdentity identity;
            if (!Manager.Assets.TryGetSpawnedObject(objectId, out identity))
                return;

            var npcEntity = identity.GetComponent<NpcEntity>();
            if (npcEntity == null)
                return;

            if (Vector3.Distance(CacheTransform.position, npcEntity.CacheTransform.position) > gameInstance.conversationDistance + 5f)
                return;

            currentNpcDialog = npcEntity.startDialog;
            if (currentNpcDialog != null)
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected void NetFuncShowNpcDialog(int npcDialogDataId)
        {
            if (onShowNpcDialog != null)
                onShowNpcDialog(npcDialogDataId);
        }

        protected void NetFuncSelectNpcDialogMenu(int menuIndex)
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
                    currentNpcDialog = currentNpcDialog.questCompletedDailog;
                    break;
            }
            if (currentNpcDialog == null)
                RequestShowNpcDialog(0);
            else
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected void NetFuncBuyNpcItem(int itemIndex, short amount)
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

        protected void NetFuncAcceptQuest(int questDataId)
        {
            var indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest >= 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            var characterQuest = CharacterQuest.Create(quest);
            quests.Add(characterQuest);
        }

        protected void NetFuncAbandonQuest(int questDataId)
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

        protected void NetFuncCompleteQuest(int questDataId)
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
            IncreaseGold(quest.rewardGold);
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

        protected void NetFuncEnterWarp()
        {
            if (IsDead() || IsPlayingActionAnimation() || warpingPortal == null)
                return;

            warpingPortal.EnterWarp(this);
        }

        protected void NetFuncBuild(int index, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
            if (IsDead() ||
                IsPlayingActionAnimation() ||
                index < 0 ||
                index >= NonEquipItems.Count)
                return;

            BuildingObject buildingObject;
            var nonEquipItem = NonEquipItems[index];
            if (!nonEquipItem.IsValid() ||
                nonEquipItem.GetBuildingItem() == null ||
                nonEquipItem.GetBuildingItem().buildingObject == null ||
                !GameInstance.BuildingObjects.TryGetValue(nonEquipItem.GetBuildingItem().buildingObject.DataId, out buildingObject) ||
                !this.DecreaseItemsByIndex(index, 1))
                return;

            var manager = Manager as BaseGameNetworkManager;
            if (manager != null)
            {
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
                buildingSaveData.DataId = buildingObject.DataId;
                buildingSaveData.CurrentHp = buildingObject.maxHp;
                buildingSaveData.Position = position;
                buildingSaveData.Rotation = rotation;
                buildingSaveData.CreatorId = Id;
                buildingSaveData.CreatorName = CharacterName;
                manager.CreateBuildingEntity(buildingSaveData, false);
            }
        }

        protected void NetFuncDestroyBuild(uint objectId)
        {
            if (IsDead() ||
                IsPlayingActionAnimation())
                return;

            LiteNetLibIdentity identity;
            if (Manager.Assets.TryGetSpawnedObject(objectId, out identity))
            {
                var manager = Manager as BaseGameNetworkManager;
                var buildingEntity = identity.GetComponent<BuildingEntity>();
                if (buildingEntity != null && buildingEntity.CreatorId.Equals(Id) && manager != null)
                    manager.DestroyBuildingEntity(buildingEntity.Id);
            }
        }

        protected void NetFuncSellItem(int index, short amount)
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
    }
}
