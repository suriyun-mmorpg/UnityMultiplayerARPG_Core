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

        protected void NetFuncAttackWithoutAimPosition()
        {
            NetFuncAttack(false, Vector3.zero);
        }

        protected void NetFuncAttackWithAimPosition(Vector3 aimPosition)
        {
            NetFuncAttack(true, aimPosition);
        }

        /// <summary>
        /// Is function will be called at server to order character to attack
        /// </summary>
        protected virtual void NetFuncAttack(bool hasAimPosition, Vector3 aimPosition)
        {
            if (!CanAttack())
                return;

            // Prepare requires data
            AnimActionType animActionType;
            int dataId;
            int animationIndex;
            bool isLeftHand;
            CharacterItem weapon;
            float triggerDuration;
            float totalDuration;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetAttackingData(
                out animActionType,
                out dataId,
                out animationIndex,
                out isLeftHand,
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
                    Dictionary<CharacterItem, short> decreaseAmmoItems;
                    if (!this.DecreaseAmmos(weaponType.requireAmmoType, 1, out decreaseAmmoItems))
                        return;
                    KeyValuePair<CharacterItem, short> firstEntry = decreaseAmmoItems.FirstOrDefault();
                    CharacterItem ammoCharacterItem = firstEntry.Key;
                    Item ammoItem = ammoCharacterItem.GetItem();
                    if (ammoItem != null && firstEntry.Value > 0)
                        allDamageAmounts = GameDataHelpers.CombineDamages(allDamageAmounts, ammoItem.GetIncreaseDamages(ammoCharacterItem.level, ammoCharacterItem.GetEquipmentBonusRate()));
                }
            }

            // Play animation on clients
            RequestPlayActionAnimation(animActionType, dataId, (byte)animationIndex);

            // Start attack routine
            isAttackingOrUsingSkill = true;
            moveSpeedRateWhileAttackOrUseSkill = weapon != null ? weapon.GetWeaponItem().moveSpeedRateWhileAttacking : 0f;
            StartCoroutine(AttackRoutine(triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition));
        }

        private IEnumerator AttackRoutine(
            float triggerDuration,
            float totalDuration,
            bool isLeftHand,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            bool hasAimPosition,
            Vector3 aimPosition)
        {
            yield return new WaitForSecondsRealtime(triggerDuration);
            LaunchDamageEntity(isLeftHand, weapon, damageInfo, allDamageAmounts, CharacterBuff.Empty, 0, hasAimPosition, aimPosition);
            yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
            isAttackingOrUsingSkill = false;
        }

        protected void NetFuncUseSkillWithoutAimPosition(int dataId)
        {
            NetFuncUseSkill(dataId, false, Vector3.zero);
        }

        protected void NetFuncUseSkillWithAimPosition(int dataId, Vector3 aimPosition)
        {
            NetFuncUseSkill(dataId, true, aimPosition);
        }

        /// <summary>
        /// Is function will be called at server to order character to use skill
        /// </summary>
        protected virtual void NetFuncUseSkill(int dataId, bool hasAimPosition, Vector3 aimPosition)
        {
            if (!CanUseSkill())
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
            bool isLeftHand;
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
                out isLeftHand,
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
            isAttackingOrUsingSkill = true;
            moveSpeedRateWhileAttackOrUseSkill = characterSkill.GetSkill().moveSpeedRateWhileUsingSkill;
            StartCoroutine(UseSkillRoutine(characterSkill, triggerDuration, totalDuration, skillAttackType, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition));
        }

        private IEnumerator UseSkillRoutine(
            CharacterSkill characterSkill,
            float triggerDuration,
            float totalDuration,
            SkillAttackType skillAttackType,
            bool isLeftHand,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            bool hasAimPosition,
            Vector3 aimPosition)
        {
            // Update skill usage states
            CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.Skill, characterSkill.dataId);
            newSkillUsage.Use(this, characterSkill.level);
            skillUsages.Add(newSkillUsage);

            yield return new WaitForSecondsRealtime(triggerDuration);
            ApplySkill(characterSkill, skillAttackType, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition);
            yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
            isAttackingOrUsingSkill = false;
        }

        /// <summary>
        /// This will be called on server to use item
        /// </summary>
        /// <param name="dataId"></param>
        protected virtual void NetFuncUseItem(short itemIndex)
        {
            if (!CanUseItem())
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
            if (!itemDropData.IsValid())
            {
                // Destroy item drop entity without item add because this is not valid
                itemDropEntity.NetworkDestroy();
                return;
            }
            if (!IncreasingItemsWillOverwhelming(itemDropData.dataId, itemDropData.amount) && this.IncreaseItems(itemDropData))
                itemDropEntity.NetworkDestroy();
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
                        oldEquipIndex = (short)this.IndexOfEquipItem(equippingItem.GetArmorItem().EquipPosition);
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
