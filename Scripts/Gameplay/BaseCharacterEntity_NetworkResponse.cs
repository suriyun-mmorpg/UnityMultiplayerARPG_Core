using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiteNetLibManager;
using System.Threading.Tasks;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public System.Action<bool> onDead;
        public System.Action<bool> onRespawn;
        public System.Action onLevelUp;

        /// <summary>
        /// Is function will be called at server to order character to attack
        /// </summary>
        protected virtual void NetFuncAttack()
        {
            if (Time.unscaledTime - lastActionCommandReceivedTime < ACTION_COMMAND_DELAY)
                return;
            lastActionCommandReceivedTime = Time.unscaledTime;

            if (IsDead() || IsPlayingActionAnimation())
                return;

            // Prepare requires data
            CharacterItem weapon;
            int actionId;
            float triggerDuration;
            float totalDuration;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetAttackingData(
                out weapon,
                out actionId,
                out triggerDuration,
                out totalDuration,
                out damageInfo,
                out allDamageAmounts);

            // Reduce ammo amount
            if (weapon != null)
            {
                var weaponItem = weapon.GetWeaponItem();
                if (weaponItem.WeaponType.requireAmmoType != null)
                {
                    Dictionary<CharacterItem, short> decreaseItems;
                    if (!this.DecreaseAmmos(weaponItem.WeaponType.requireAmmoType, 1, out decreaseItems))
                        return;
                    var firstEntry = decreaseItems.FirstOrDefault();
                    var characterItem = firstEntry.Key;
                    var item = characterItem.GetItem();
                    if (item != null && firstEntry.Value > 0)
                        allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(allDamageAmounts, item.GetIncreaseDamages(characterItem.level, characterItem.GetEquipmentBonusRate()));
                }
            }

            // Play animation on clients
            RequestPlayActionAnimation(actionId, AnimActionType.Attack);
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
            LaunchDamageEntity(position, weapon, damageInfo, allDamageAmounts, CharacterBuff.Empty, -1);
            yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
        }

        /// <summary>
        /// Is function will be called at server to order character to use skill
        /// </summary>
        /// <param name="position">Target position to apply skill at</param>
        /// <param name="skillIndex">Index in `characterSkills` list which will be used</param>
        protected virtual void NetFuncUseSkill(Vector3 position, int skillIndex)
        {
            if (Time.unscaledTime - lastActionCommandReceivedTime < ACTION_COMMAND_DELAY)
                return;
            lastActionCommandReceivedTime = Time.unscaledTime;

            if (IsDead() ||
                IsPlayingActionAnimation() ||
                skillIndex < 0 ||
                skillIndex >= skills.Count)
                return;

            var characterSkill = skills[skillIndex];
            if (!characterSkill.CanUse(this))
                return;

            // Prepare requires data
            CharacterItem weapon;
            int actionId;
            float triggerDuration;
            float totalDuration;
            bool isAttack;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetUsingSkillData(
                characterSkill,
                out weapon,
                out actionId,
                out triggerDuration,
                out totalDuration,
                out isAttack,
                out damageInfo,
                out allDamageAmounts);

            if (weapon != null)
            {
                var weaponItem = weapon.GetWeaponItem();
                // Reduce ammo amount
                if (characterSkill.GetSkill().IsAttack() && weaponItem.WeaponType.requireAmmoType != null)
                {
                    Dictionary<CharacterItem, short> decreaseItems;
                    if (!this.DecreaseAmmos(weaponItem.WeaponType.requireAmmoType, 1, out decreaseItems))
                        return;
                    var firstEntry = decreaseItems.FirstOrDefault();
                    var characterItem = firstEntry.Key;
                    var item = characterItem.GetItem();
                    if (item != null && firstEntry.Value > 0)
                        allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(allDamageAmounts, item.GetIncreaseDamages(characterItem.level, characterItem.GetEquipmentBonusRate()));
                }
            }

            // Play animation on clients
            RequestPlayActionAnimation(actionId, AnimActionType.Skill);
            // Start use skill routine
            StartCoroutine(UseSkillRoutine(skillIndex, position, triggerDuration, totalDuration, isAttack, weapon, damageInfo, allDamageAmounts));
        }

        private IEnumerator UseSkillRoutine(
            int skillIndex,
            Vector3 position,
            float triggerDuration,
            float totalDuration,
            bool isAttack,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            // Update skill states
            var characterSkill = skills[skillIndex];
            characterSkill.Used();
            characterSkill.ReduceMp(this);
            skills[skillIndex] = characterSkill;
            yield return new WaitForSecondsRealtime(triggerDuration);
            var skill = characterSkill.GetSkill();
            switch (skill.skillType)
            {
                case SkillType.Active:
                    ApplySkillBuff(characterSkill);
                    if (isAttack)
                    {
                        CharacterBuff debuff = CharacterBuff.Empty;
                        if (skill.isDebuff)
                            debuff = CharacterBuff.Create(Id, BuffType.SkillDebuff, skill.DataId, characterSkill.level);
                        LaunchDamageEntity(position, weapon, damageInfo, allDamageAmounts, debuff, skill.hitEffects.Id);
                    }
                    break;
                case SkillType.CraftItem:
                    if (skill.CanCraft(this))
                    {
                        var craftRequirements = skill.craftRequirements;
                        foreach (var craftRequirement in craftRequirements)
                        {
                            if (craftRequirement.item != null && craftRequirement.amount > 0)
                                this.DecreaseItems(craftRequirement.item.DataId, craftRequirement.amount);
                        }
                        this.IncreaseItems(skill.craftingItem.DataId, 1, 1);
                    }
                    break;
            }
            yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
        }

        /// <summary>
        /// This will be called on server to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        protected virtual void NetFuncUseItem(int itemIndex)
        {
            if (IsDead() ||
                itemIndex < 0 ||
                itemIndex >= nonEquipItems.Count)
                return;

            var item = nonEquipItems[itemIndex];
            var potionItem = item.GetPotionItem();
            if (potionItem != null && this.DecreaseItemsByIndex(itemIndex, 1))
                ApplyPotionBuff(item);
        }

        /// <summary>
        /// This will be called at every clients to play any action animation
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="animActionType"></param>
        protected virtual void NetFuncPlayActionAnimation(int actionId, AnimActionType animActionType)
        {
            if (IsDead())
                return;
            this.animActionType = animActionType;
            StartCoroutine(PlayActionAnimationRoutine(actionId, animActionType));
        }

        private IEnumerator PlayActionAnimationRoutine(int actionId, AnimActionType animActionType)
        {
            var playSpeedMultiplier = 1f;
            switch (animActionType)
            {
                case AnimActionType.Attack:
                    playSpeedMultiplier = CacheAtkSpeed;
                    break;
            }
            if (Model != null)
                yield return Model.PlayActionAnimation(actionId, animActionType, playSpeedMultiplier);
            this.animActionType = AnimActionType.None;
        }

        /// <summary>
        /// This will be called at every clients to play any effect
        /// </summary>
        /// <param name="effectId"></param>
        protected virtual void NetFuncPlayEffect(int effectId)
        {
            GameEffectCollection gameEffectCollection;
            if (Model == null || !GameInstance.GameEffectCollections.TryGetValue(effectId, out gameEffectCollection))
                return;
            Model.InstantiateEffect(gameEffectCollection.effects);
        }

        /// <summary>
        /// This will be called at server to order character to pickup items
        /// </summary>
        /// <param name="objectId"></param>
        protected virtual void NetFuncPickupItem(uint objectId)
        {
            if (IsDead() || IsPlayingActionAnimation())
                return;

            LiteNetLibIdentity identity;
            if (!Manager.Assets.TryGetSpawnedObject(objectId, out identity))
                return;

            var itemDropEntity = identity.GetComponent<ItemDropEntity>();
            if (itemDropEntity == null)
                return;

            if (Vector3.Distance(CacheTransform.position, itemDropEntity.CacheTransform.position) > gameInstance.pickUpItemDistance + 5f)
                return;

            var itemDropData = itemDropEntity.dropData;
            if (!itemDropData.IsValid())
            {
                // Destroy item drop entity without item add because this is not valid
                itemDropEntity.NetworkDestroy();
                return;
            }
            var itemDataId = itemDropData.dataId;
            var level = itemDropData.level;
            var amount = itemDropData.amount;
            if (!IncreasingItemsWillOverwhelming(itemDataId, amount) && this.IncreaseItems(itemDataId, level, amount))
                itemDropEntity.NetworkDestroy();
        }

        /// <summary>
        /// This will be called at server to order character to drop items
        /// </summary>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        protected virtual void NetFuncDropItem(int index, short amount)
        {
            if (IsDead() ||
                IsPlayingActionAnimation() ||
                index < 0 ||
                index >= nonEquipItems.Count)
                return;

            var nonEquipItem = nonEquipItems[index];
            if (!nonEquipItem.IsValid() || amount > nonEquipItem.amount)
                return;

            var itemDataId = nonEquipItem.dataId;
            var level = nonEquipItem.level;
            if (this.DecreaseItemsByIndex(index, amount))
                ItemDropEntity.DropItem(this, itemDataId, level, amount);
        }

        /// <summary>
        /// This will be called at server to order character to equip equipments
        /// </summary>
        /// <param name="nonEquipIndex"></param>
        /// <param name="equipPosition"></param>
        protected virtual void NetFuncEquipItem(int nonEquipIndex, string equipPosition)
        {
            if (IsDead() ||
                IsPlayingActionAnimation() ||
                nonEquipIndex < 0 ||
                nonEquipIndex >= nonEquipItems.Count)
                return;

            var equippingItem = nonEquipItems[nonEquipIndex];

            string reasonWhyCannot;
            HashSet<string> shouldUnequipPositions;
            if (!CanEquipItem(equippingItem, equipPosition, out reasonWhyCannot, out shouldUnequipPositions))
            {
                Debug.LogError("Cannot equip item " + nonEquipIndex + " " + equipPosition + " " + reasonWhyCannot);
                return;
            }

            // Unequip equipped item if exists
            foreach (var shouldUnequipPosition in shouldUnequipPositions)
            {
                NetFuncUnEquipItem(shouldUnequipPosition);
            }
            // Equipping items
            var tempEquipWeapons = EquipWeapons;
            if (equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
            {
                tempEquipWeapons.rightHand = equippingItem;
                EquipWeapons = tempEquipWeapons;
            }
            else if (equipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
            {
                tempEquipWeapons.leftHand = equippingItem;
                EquipWeapons = tempEquipWeapons;
            }
            else
            {
                equipItems.Add(equippingItem);
                equipItemIndexes.Add(equipPosition, equipItems.Count - 1);
            }
            nonEquipItems.RemoveAt(nonEquipIndex);
        }

        /// <summary>
        /// This will be called at server to order character to unequip equipments
        /// </summary>
        /// <param name="fromEquipPosition"></param>
        protected virtual void NetFuncUnEquipItem(string fromEquipPosition)
        {
            if (IsDead() || IsPlayingActionAnimation())
                return;

            var equippedArmorIndex = -1;
            var tempEquipWeapons = EquipWeapons;
            var unEquipItem = CharacterItem.Empty;
            if (fromEquipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
            {
                unEquipItem = tempEquipWeapons.rightHand;
                tempEquipWeapons.rightHand = CharacterItem.Empty;
                EquipWeapons = tempEquipWeapons;
            }
            else if (fromEquipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
            {
                unEquipItem = tempEquipWeapons.leftHand;
                tempEquipWeapons.leftHand = CharacterItem.Empty;
                EquipWeapons = tempEquipWeapons;
            }
            else if (equipItemIndexes.TryGetValue(fromEquipPosition, out equippedArmorIndex))
            {
                unEquipItem = equipItems[equippedArmorIndex];
                equipItems.RemoveAt(equippedArmorIndex);
                UpdateEquipItemIndexes();
            }
            if (unEquipItem.IsValid())
                nonEquipItems.Add(unEquipItem);
        }

        protected virtual void NetFuncOnDead(bool isInitialize)
        {
            animActionType = AnimActionType.None;
            if (onDead != null)
                onDead.Invoke(isInitialize);
        }

        protected virtual void NetFuncOnRespawn(bool isInitialize)
        {
            animActionType = AnimActionType.None;
            if (onRespawn != null)
                onRespawn.Invoke(isInitialize);
        }

        protected virtual void NetFuncOnLevelUp()
        {
            if (gameInstance.levelUpEffect != null && Model != null)
                Model.InstantiateEffect(new GameEffect[] { gameInstance.levelUpEffect });
            if (onLevelUp != null)
                onLevelUp.Invoke();
        }
    }
}
