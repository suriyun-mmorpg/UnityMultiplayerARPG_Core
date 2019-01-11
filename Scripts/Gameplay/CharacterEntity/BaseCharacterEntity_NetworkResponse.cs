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

        /// <summary>
        /// Is function will be called at server to order character to attack
        /// </summary>
        protected virtual void NetFuncAttack()
        {
            if (Time.unscaledTime - lastActionCommandReceivedTime < ACTION_COMMAND_DELAY)
                return;
            lastActionCommandReceivedTime = Time.unscaledTime;

            if (!CanMoveOrDoActions())
                return;

            // Prepare requires data
            AnimActionType animActionType;
            int dataId;
            int animationIndex;
            CharacterItem weapon;
            float triggerDuration;
            float totalDuration;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetAttackingData(
                out animActionType,
                out dataId,
                out animationIndex,
                out weapon,
                out triggerDuration,
                out totalDuration,
                out damageInfo,
                out allDamageAmounts);
            
            // Reduce ammo amount
            if (weapon != null)
            {
                WeaponType weaponType = weapon.GetWeaponItem().WeaponType;
                if (weaponType.requireAmmoType != null)
                {
                    Dictionary<CharacterItem, short> decreaseItems;
                    if (!this.DecreaseAmmos(weaponType.requireAmmoType, 1, out decreaseItems))
                        return;
                    KeyValuePair<CharacterItem, short> firstEntry = decreaseItems.FirstOrDefault();
                    CharacterItem characterItem = firstEntry.Key;
                    Item item = characterItem.GetItem();
                    if (item != null && firstEntry.Value > 0)
                        allDamageAmounts = GameDataHelpers.CombineDamages(allDamageAmounts, item.GetIncreaseDamages(characterItem.level, characterItem.GetEquipmentBonusRate()));
                }
            }

            // Play animation on clients
            RequestPlayActionAnimation(animActionType, dataId, (byte)animationIndex);

            // Start attack routine
            StartCoroutine(AttackRoutine(CacheTransform.position, triggerDuration, totalDuration, weapon, damageInfo, allDamageAmounts));
        }

        private IEnumerator AttackRoutine(
            Vector3 position,
            float triggerDuration,
            float totalDuration,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            yield return new WaitForSecondsRealtime(triggerDuration);
            LaunchDamageEntity(position, weapon, damageInfo, allDamageAmounts, CharacterBuff.Empty, 0);
            yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
        }

        /// <summary>
        /// Is function will be called at server to order character to use skill
        /// </summary>
        /// <param name="position">Target position to apply skill at</param>
        /// <param name="dataId">Skill's data id which will be used</param>
        protected virtual void NetFuncUseSkill(Vector3 position, int dataId)
        {
            if (Time.unscaledTime - lastActionCommandReceivedTime < ACTION_COMMAND_DELAY)
                return;
            lastActionCommandReceivedTime = Time.unscaledTime;

            if (!CanMoveOrDoActions())
                return;

            int index = this.IndexOfSkill(dataId);
            if (index < 0)
                return;

            CharacterSkill characterSkill = skills[index];
            if (!characterSkill.CanUse(this))
                return;

            // Prepare requires data
            AnimActionType animActionType;
            int animationIndex;
            SkillAttackType skillAttackType;
            CharacterItem weapon;
            float triggerDuration;
            float totalDuration;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetUsingSkillData(
                characterSkill,
                out animActionType,
                out dataId,
                out animationIndex,
                out skillAttackType,
                out weapon,
                out triggerDuration,
                out totalDuration,
                out damageInfo,
                out allDamageAmounts);
            
            if (weapon != null)
            {
                WeaponType weaponType = weapon.GetWeaponItem().WeaponType;
                // Reduce ammo amount
                if (skillAttackType != SkillAttackType.None && weaponType.requireAmmoType != null)
                {
                    Dictionary<CharacterItem, short> decreaseItems;
                    if (!this.DecreaseAmmos(weaponType.requireAmmoType, 1, out decreaseItems))
                        return;
                    KeyValuePair<CharacterItem, short> firstEntry = decreaseItems.FirstOrDefault();
                    CharacterItem characterItem = firstEntry.Key;
                    Item item = characterItem.GetItem();
                    if (item != null && firstEntry.Value > 0)
                        allDamageAmounts = GameDataHelpers.CombineDamages(allDamageAmounts, item.GetIncreaseDamages(characterItem.level, characterItem.GetEquipmentBonusRate()));
                }
            }

            // Play animation on clients
            RequestPlayActionAnimation(animActionType, dataId, (byte)animationIndex);

            // Start use skill routine
            StartCoroutine(UseSkillRoutine(characterSkill, position, triggerDuration, totalDuration, skillAttackType, weapon, damageInfo, allDamageAmounts));
        }

        private IEnumerator UseSkillRoutine(
            CharacterSkill characterSkill,
            Vector3 position,
            float triggerDuration,
            float totalDuration,
            SkillAttackType skillAttackType,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            // Update skill usage states
            CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.Skill, characterSkill.dataId);
            newSkillUsage.Use(this, characterSkill.level);
            skillUsages.Add(newSkillUsage);

            yield return new WaitForSecondsRealtime(triggerDuration);
            ApplySkill(characterSkill, position, skillAttackType, weapon, damageInfo, allDamageAmounts);
            yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
        }

        /// <summary>
        /// This will be called on server to use item
        /// </summary>
        /// <param name="dataId"></param>
        protected virtual void NetFuncUseItem(short itemIndex)
        {
            if (IsDead())
                return;
            
            if (itemIndex >= nonEquipItems.Count)
                return;

            CharacterItem characterItem = nonEquipItems[itemIndex];
            if (characterItem.IsLock())
                return;

            Item potionItem = characterItem.GetPotionItem();
            if (potionItem != null && this.DecreaseItemsByIndex(itemIndex, 1))
                ApplyPotionBuff(potionItem, characterItem.level);
            Item petItem = characterItem.GetPetItem();
            if (petItem != null && this.DecreaseItemsByIndex(itemIndex, 1))
                ApplyItemPetSummon(petItem, characterItem.level, characterItem.exp);
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
            animActionType = (AnimActionType)byteAnimActionType;
            StartCoroutine(PlayActionAnimationRoutine(animActionType, dataId, index));
        }

        private IEnumerator PlayActionAnimationRoutine(AnimActionType animActionType, int dataId, int index)
        {
            float playSpeedMultiplier = 1f;
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                case AnimActionType.AttackLeftHand:
                    playSpeedMultiplier = CacheAtkSpeed;
                    break;
            }
            if (CharacterModel != null)
                yield return CharacterModel.PlayActionAnimation(animActionType, dataId, index, playSpeedMultiplier);
            this.animActionType = AnimActionType.None;
        }

        /// <summary>
        /// This will be called at server to order character to pickup items
        /// </summary>
        /// <param name="objectId"></param>
        protected virtual void NetFuncPickupItem(PackedUInt objectId)
        {
            if (!CanMoveOrDoActions())
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
            if (!itemDropData.IsValid())
            {
                // Destroy item drop entity without item add because this is not valid
                itemDropEntity.NetworkDestroy();
                return;
            }
            int itemDataId = itemDropData.dataId;
            short level = itemDropData.level;
            short amount = itemDropData.amount;
            if (!IncreasingItemsWillOverwhelming(itemDataId, amount) && this.IncreaseItems(itemDataId, level, amount))
                itemDropEntity.NetworkDestroy();
        }

        /// <summary>
        /// This will be called at server to order character to drop items
        /// </summary>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        protected virtual void NetFuncDropItem(short index, short amount)
        {
            if (!CanMoveOrDoActions() ||
                index >= nonEquipItems.Count)
                return;

            CharacterItem nonEquipItem = nonEquipItems[index];
            if (!nonEquipItem.IsValid() || amount > nonEquipItem.amount)
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
            if (!CanMoveOrDoActions() ||
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
                    if (oldEquipIndex >= 0)
                        NetFuncUnEquipItem((byte)InventoryType.EquipItems, oldEquipIndex);
                    equipItems.Add(equippingItem);
                    equipItemIndexes.Add(equippingItem.GetArmorItem().EquipPosition, equipItems.Count - 1);
                    break;
            }
            nonEquipItems.RemoveAt(nonEquipIndex);
        }

        /// <summary>
        /// This will be called at server to order character to unequip equipments
        /// </summary>
        /// <param name="fromEquipPosition"></param>
        protected virtual void NetFuncUnEquipItem(byte byteInventoryType, short index)
        {
            if (!CanMoveOrDoActions())
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
            if (unEquipItem.IsValid())
                nonEquipItems.Add(unEquipItem);
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
    }
}
