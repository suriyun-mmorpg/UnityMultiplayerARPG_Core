using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public System.Action onDead;
        public System.Action onRespawn;
        public System.Action onLevelUp;

        protected void NetFuncSetAimPosition(Vector3 aimPosition)
        {
            HasAimPosition = true;
            AimPosition = aimPosition;
        }

        protected void NetFuncUnsetAimPosition()
        {
            HasAimPosition = false;
            AimPosition = Vector3.zero;
        }

        protected void NetFuncAttackWithoutAimPosition(bool isLeftHand)
        {
            NetFuncAttack(isLeftHand, false, Vector3.zero);
        }

        protected void NetFuncAttackWithAimPosition(bool isLeftHand, Vector3 aimPosition)
        {
            NetFuncAttack(isLeftHand, true, aimPosition);
        }

        protected void NetFuncUseSkillWithoutAimPosition(int dataId, bool isLeftHand)
        {
            NetFuncUseSkill(dataId, isLeftHand, false, Vector3.zero);
        }

        protected void NetFuncUseSkillWithAimPosition(int dataId, bool isLeftHand, Vector3 aimPosition)
        {
            NetFuncUseSkill(dataId, isLeftHand, true, aimPosition);
        }

        /// <summary>
        /// This will be called at every clients to play any action animation
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="animActionType"></param>
        protected virtual void NetFuncPlayActionAnimation(byte byteAnimActionType, int dataId, byte index)
        {
            if (IsDead())
                return;
            StartCoroutine(PlayActionAnimationRoutine((AnimActionType)byteAnimActionType, dataId, index));
        }

        private IEnumerator PlayActionAnimationRoutine(AnimActionType animActionType, int skillOrWeaponTypeDataId, int index)
        {
            this.animActionType = animActionType;
            float playSpeedMultiplier = 1f;
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    playSpeedMultiplier = CacheAtkSpeed;
                    // Set doing action state at clients and server
                    isAttackingOrUsingSkill = true;
                    // Calculate move speed rate while doing action at clients and server
                    moveSpeedRateWhileAttackOrUseSkill = 1f;
                    if (EquipWeapons != null && EquipWeapons.rightHand != null && EquipWeapons.rightHand.GetWeaponItem() != null)
                        moveSpeedRateWhileAttackOrUseSkill = EquipWeapons.rightHand.GetWeaponItem().moveSpeedRateWhileAttacking;
                    else
                        moveSpeedRateWhileAttackOrUseSkill = gameInstance.DefaultWeaponItem.moveSpeedRateWhileAttacking;
                    break;
                case AnimActionType.AttackLeftHand:
                    playSpeedMultiplier = CacheAtkSpeed;
                    // Set doing action state at clients and server
                    isAttackingOrUsingSkill = true;
                    // Calculate move speed rate while doing action at clients and server
                    moveSpeedRateWhileAttackOrUseSkill = 1f;
                    if (EquipWeapons != null && EquipWeapons.leftHand != null && EquipWeapons.leftHand.GetWeaponItem() != null)
                        moveSpeedRateWhileAttackOrUseSkill = EquipWeapons.leftHand.GetWeaponItem().moveSpeedRateWhileAttacking;
                    else
                        moveSpeedRateWhileAttackOrUseSkill = gameInstance.DefaultWeaponItem.moveSpeedRateWhileAttacking;
                    break;
                case AnimActionType.Skill:
                    // Set doing action state at clients and server
                    isAttackingOrUsingSkill = true;
                    // Calculate move speed rate while doing action at clients and server
                    moveSpeedRateWhileAttackOrUseSkill = 1f;
                    if (GameInstance.Skills.ContainsKey(skillOrWeaponTypeDataId))
                        moveSpeedRateWhileAttackOrUseSkill = GameInstance.Skills[skillOrWeaponTypeDataId].moveSpeedRateWhileUsingSkill;
                    break;
                case AnimActionType.ReloadLeftHand:
                case AnimActionType.ReloadRightHand:
                    // Set doing action state at clients and server
                    isAttackingOrUsingSkill = true;
                    // Calculate move speed rate while doing action at clients and server
                    moveSpeedRateWhileAttackOrUseSkill = 1f;
                    break;
            }
            if (CharacterModel != null)
                yield return CharacterModel.PlayActionAnimation(animActionType, skillOrWeaponTypeDataId, index, playSpeedMultiplier);
            // Set doing action state at clients and server
            this.animActionType = AnimActionType.None;
            isAttackingOrUsingSkill = false;
        }

        /// <summary>
        /// This will be called at server to order character to pickup items
        /// </summary>
        /// <param name="objectId"></param>
        protected virtual void NetFuncPickupItem(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            ItemDropEntity itemDropEntity = null;
            if (!TryGetEntityByObjectId(objectId, out itemDropEntity))
                return;

            if (Vector3.Distance(CacheTransform.position, itemDropEntity.CacheTransform.position) > gameInstance.pickUpItemDistance + 5f)
                return;

            if (!itemDropEntity.IsAbleToLoot(this))
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotAbleToLoot);
                return;
            }

            CharacterItem itemDropData = itemDropEntity.dropData;
            if (!itemDropData.NotEmptySlot())
            {
                // Destroy item drop entity without item add because this is not valid
                itemDropEntity.MarkAsPickedUp();
                itemDropEntity.NetworkDestroy();
                return;
            }
            if (!this.IncreasingItemsWillOverwhelming(itemDropData.dataId, itemDropData.amount) && this.IncreaseItems(itemDropData))
            {
                itemDropEntity.MarkAsPickedUp();
                itemDropEntity.NetworkDestroy();
            }
        }

        /// <summary>
        /// This will be called at server to order character to drop items
        /// </summary>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        protected virtual void NetFuncDropItem(short index, short amount)
        {
            if (!CanDoActions() ||
                index >= nonEquipItems.Count)
                return;

            CharacterItem nonEquipItem = nonEquipItems[index];
            if (!nonEquipItem.NotEmptySlot() || amount > nonEquipItem.amount)
                return;

            if (this.DecreaseItemsByIndex(index, amount))
            {
                // Drop item to the ground
                CharacterItem dropData = nonEquipItem.Clone();
                dropData.amount = amount;
                ItemDropEntity.DropItem(this, dropData, new uint[] { ObjectId });
            }
        }

        /// <summary>
        /// This will be called at server to order character to equip equipments
        /// </summary>
        /// <param name="nonEquipIndex"></param>
        /// <param name="equipPosition"></param>
        protected virtual void NetFuncEquipItem(short nonEquipIndex, byte byteInventoryType, short oldEquipIndex)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= nonEquipItems.Count)
                return;

            CharacterItem equippingItem = nonEquipItems[nonEquipIndex];

            GameMessage.Type gameMessageType;
            bool shouldUnequipRightHand;
            bool shouldUnequipLeftHand;
            if (!CanEquipItem(equippingItem, (InventoryType)byteInventoryType, oldEquipIndex, out gameMessageType, out shouldUnequipRightHand, out shouldUnequipLeftHand))
            {
                gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                return;
            }
            // Unequip equipped item that already equipped
            if (shouldUnequipRightHand)
                NetFuncUnEquipItem((byte)InventoryType.EquipWeaponRight, 0);
            if (shouldUnequipLeftHand)
                NetFuncUnEquipItem((byte)InventoryType.EquipWeaponLeft, 0);
            // Equipping items
            EquipWeapons tempEquipWeapons = EquipWeapons;
            switch ((InventoryType)byteInventoryType)
            {
                case InventoryType.EquipWeaponRight:
                    tempEquipWeapons.rightHand = equippingItem;
                    EquipWeapons = tempEquipWeapons;
                    break;
                case InventoryType.EquipWeaponLeft:
                    tempEquipWeapons.leftHand = equippingItem;
                    EquipWeapons = tempEquipWeapons;
                    break;
                case InventoryType.EquipItems:
                    if (oldEquipIndex < 0)
                        oldEquipIndex = (short)this.IndexOfEquipItemByEquipPosition(equippingItem.GetArmorItem().EquipPosition);
                    if (oldEquipIndex >= 0)
                        NetFuncUnEquipItem((byte)InventoryType.EquipItems, oldEquipIndex);
                    equipItems.Add(equippingItem);
                    equipItemIndexes.Add(equippingItem.GetArmorItem().EquipPosition, equipItems.Count - 1);
                    break;
            }
            nonEquipItems.RemoveAt(nonEquipIndex);
            this.FillEmptySlots();
        }

        /// <summary>
        /// This will be called at server to order character to unequip equipments
        /// </summary>
        /// <param name="fromEquipPosition"></param>
        protected virtual void NetFuncUnEquipItem(byte byteInventoryType, short index)
        {
            if (!CanDoActions())
                return;

            EquipWeapons tempEquipWeapons = EquipWeapons;
            CharacterItem unEquipItem = CharacterItem.Empty;
            switch ((InventoryType)byteInventoryType)
            {
                case InventoryType.EquipWeaponRight:
                    unEquipItem = tempEquipWeapons.rightHand;
                    tempEquipWeapons.rightHand = CharacterItem.Empty;
                    EquipWeapons = tempEquipWeapons;
                    break;
                case InventoryType.EquipWeaponLeft:
                    unEquipItem = tempEquipWeapons.leftHand;
                    tempEquipWeapons.leftHand = CharacterItem.Empty;
                    EquipWeapons = tempEquipWeapons;
                    break;
                case InventoryType.EquipItems:
                    unEquipItem = equipItems[index];
                    equipItems.RemoveAt(index);
                    UpdateEquipItemIndexes();
                    break;
            }

            if (unEquipItem.NotEmptySlot())
                this.AddOrInsertNonEquipItems(unEquipItem);
            this.FillEmptySlots();
        }

        protected virtual void NetFuncOnDead()
        {
            animActionType = AnimActionType.None;
            if (onDead != null)
                onDead.Invoke();
        }

        protected virtual void NetFuncOnRespawn()
        {
            animActionType = AnimActionType.None;
            if (onRespawn != null)
                onRespawn.Invoke();
        }

        protected virtual void NetFuncOnLevelUp()
        {
            if (gameInstance.levelUpEffect != null && CharacterModel != null)
                CharacterModel.InstantiateEffect(new GameEffect[] { gameInstance.levelUpEffect });
            if (onLevelUp != null)
                onLevelUp.Invoke();
        }

        protected virtual void NetFuncUnSummon(PackedUInt objectId)
        {
            int index = this.IndexOfSummon(objectId);
            if (index < 0)
                return;

            CharacterSummon summon = Summons[index];
            if (summon.type != SummonType.Pet)
                return;

            Summons.RemoveAt(index);
            summon.UnSummon(this);
        }

        protected virtual void NetFuncSwapOrMergeNonEquipItems(short index1, short index2)
        {
            if (!CanDoActions() ||
                index1 >= nonEquipItems.Count ||
                index2 >= nonEquipItems.Count)
                return;

            CharacterItem nonEquipItem1 = nonEquipItems[index1];
            CharacterItem nonEquipItem2 = nonEquipItems[index2];

            if (nonEquipItem1.dataId == nonEquipItem2.dataId &&
                nonEquipItem1.NotEmptySlot() && nonEquipItem2.NotEmptySlot())
            {
                // Merge or swap
                if (nonEquipItem1.IsFull() || nonEquipItem2.IsFull())
                {
                    // Swap
                    NetFuncSwapNonEquipItems(index1, index2);
                }
                else
                {
                    // Merge
                    NetFuncMergeNonEquipItems(index1, index2);
                }
            }
            else
            {
                // Swap
                NetFuncSwapNonEquipItems(index1, index2);
            }
        }

        protected void NetFuncMergeNonEquipItems(short index1, short index2)
        {
            if (!CanDoActions() ||
                index1 >= nonEquipItems.Count ||
                index2 >= nonEquipItems.Count)
                return;

            CharacterItem nonEquipItem1 = nonEquipItems[index1];
            CharacterItem nonEquipItem2 = nonEquipItems[index2];
            short maxStack = nonEquipItem2.GetMaxStack();
            if (nonEquipItem2.amount + nonEquipItem1.amount <= maxStack)
            {
                nonEquipItem2.amount += nonEquipItem1.amount;
                nonEquipItems[index2] = nonEquipItem2;
                nonEquipItems.RemoveAt(index1);
            }
            else
            {
                short mergeAmount = (short)(maxStack - nonEquipItem2.amount);
                nonEquipItem2.amount = maxStack;
                nonEquipItem1.amount -= mergeAmount;
                nonEquipItems[index1] = nonEquipItem1;
                nonEquipItems[index2] = nonEquipItem2;
            }
        }

        protected void NetFuncSwapNonEquipItems(short index1, short index2)
        {
            if (!CanDoActions() ||
                index1 >= nonEquipItems.Count ||
                index2 >= nonEquipItems.Count)
                return;

            CharacterItem nonEquipItem1 = nonEquipItems[index1];
            CharacterItem nonEquipItem2 = nonEquipItems[index2];

            nonEquipItems[index2] = nonEquipItem1;
            nonEquipItems[index1] = nonEquipItem2;
        }
    }
}
