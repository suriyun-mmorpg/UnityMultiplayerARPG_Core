using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.SKILL_FILE, menuName = GameDataMenuConsts.SKILL_MENU, order = GameDataMenuConsts.SKILL_ORDER)]
    public partial class Skill : BaseSkill
    {
        public enum SkillAttackType : byte
        {
            None,
            Normal,
            BasedOnWeapon,
        }

        public enum SkillBuffType : byte
        {
            None,
            BuffToUser,
            BuffToNearbyAllies,
            BuffToNearbyCharacters,
            BuffToTarget,
            Toggle,
            BuffToAlly,
            BuffToEnemy,
            BuffToNearbyPartyMembers,
            BuffToPartyMember,
        }

        [Category("Skill Settings")]
        public SkillType skillType;

        [Category(3, "Attacking")]
        public SkillAttackType skillAttackType;
        public DamageInfo damageInfo = new DamageInfo();
        public DamageIncremental damageAmount;
        public DamageEffectivenessAttribute[] effectivenessAttributes;
        public DamageInflictionIncremental[] weaponDamageInflictions;
        public IncrementalFloat weaponDamageMultiplicator;
        public DamageIncremental[] additionalDamageAmounts;
        [FormerlySerializedAs("increaseDamageWithBuffs")]
        public bool increaseDamageAmountsWithBuffs;
        public bool isDebuff;
        public Buff debuff;
        public StatusEffectApplying[] attackStatusEffects;
        public HarvestType harvestType;
        public IncrementalMinMaxFloat harvestDamageAmount;
        public GameEffect[] damageHitEffects;

        [Category(4, "Buff")]
        public SkillBuffType skillBuffType;
        public IncrementalFloat buffDistance;
        public bool buffToUserIfNoTarget = true;
        public Buff buff;

        [Category(5, "Summon/Mount/Item Craft")]
        public SkillSummon summon;
        public SkillMount mount;
        public ItemCraft itemCraft;

        [System.NonSerialized]
        private Dictionary<Attribute, float> _cacheEffectivenessAttributes;
        public Dictionary<Attribute, float> CacheEffectivenessAttributes
        {
            get
            {
                if (_cacheEffectivenessAttributes == null)
                    _cacheEffectivenessAttributes = GameDataHelpers.CombineDamageEffectivenessAttributes(effectivenessAttributes, new Dictionary<Attribute, float>());
                return _cacheEffectivenessAttributes;
            }
        }

        public override GameEffect[] DamageHitEffects
        {
            get
            {
                return damageHitEffects;
            }
        }

        protected override void ApplySkillImplement(
            BaseCharacterEntity skillUser,
            int skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            List<Dictionary<DamageElement, MinMaxFloat>> damageAmounts,
            uint targetObjectId,
            AimPosition aimPosition)
        {
            // Apply skills only when it's active skill
            if (skillType != SkillType.Active)
                return;

            ApplySkillBuff(skillUser, skillLevel, weapon, targetObjectId);

            // Apply attack skill
            if (IsAttack && TryGetDamageInfo(skillUser, isLeftHand, out DamageInfo damageInfo))
            {
                // Launch damage entity to apply damage to other characters
                damageInfo.LaunchDamageEntity(
                    skillUser,
                    isLeftHand,
                    weapon,
                    simulateSeed,
                    triggerIndex,
                    spreadIndex,
                    Vector3.zero,
                    damageAmounts,
                    this,
                    skillLevel,
                    aimPosition);
            }
        }

        protected void ApplySkillBuff(BaseCharacterEntity skillUser, int skillLevel, CharacterItem weapon, uint targetObjectId)
        {
            if (skillUser.IsDead() || !skillUser.IsServer || skillLevel <= 0)
                return;
            int overlapMask = GameInstance.Singleton.playerLayer.Mask | GameInstance.Singleton.playingLayer.Mask | GameInstance.Singleton.monsterLayer.Mask;
            EntityInfo skillUserInfo = skillUser.GetInfo();
            List<BaseCharacterEntity> tempCharacters;
            BaseCharacterEntity targetEntity = null;
            switch (skillBuffType)
            {
                case SkillBuffType.BuffToUser:
                    skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                    break;
                case SkillBuffType.BuffToNearbyAllies:
                    tempCharacters = skillUser.FindAliveEntities<BaseCharacterEntity>(buffDistance.GetAmount(skillLevel), true, false, false, overlapMask);
                    foreach (BaseCharacterEntity applyBuffCharacter in tempCharacters)
                    {
                        applyBuffCharacter.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                    }
                    skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                    break;
                case SkillBuffType.BuffToNearbyCharacters:
                    tempCharacters = skillUser.FindAliveEntities<BaseCharacterEntity>(buffDistance.GetAmount(skillLevel), true, false, true, overlapMask);
                    foreach (BaseCharacterEntity applyBuffCharacter in tempCharacters)
                    {
                        applyBuffCharacter.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                    }
                    skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                    break;
                case SkillBuffType.BuffToTarget:
                    if (!skillUser.CurrentGameManager.TryGetEntityByObjectId(targetObjectId, out targetEntity))
                        targetEntity = null;
                    if (buffToUserIfNoTarget && targetEntity == null)
                        targetEntity = skillUser;
                    if (targetEntity != null && !targetEntity.IsDead())
                        targetEntity.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                    break;
                case SkillBuffType.Toggle:
                    int indexOfBuff = skillUser.IndexOfBuff(BuffType.SkillBuff, DataId);
                    if (indexOfBuff >= 0)
                        skillUser.Buffs.RemoveAt(indexOfBuff);
                    else
                        skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                    break;
                case SkillBuffType.BuffToAlly:
                    if (!skillUser.CurrentGameManager.TryGetEntityByObjectId(targetObjectId, out targetEntity))
                        targetEntity = null;
                    if (buffToUserIfNoTarget && (targetEntity == null || !targetEntity.IsAlly(skillUserInfo)))
                        targetEntity = skillUser;
                    if (targetEntity != null && !targetEntity.IsDead())
                        targetEntity.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                    break;
                case SkillBuffType.BuffToEnemy:
                    if (!skillUser.CurrentGameManager.TryGetEntityByObjectId(targetObjectId, out targetEntity))
                        targetEntity = null;
                    if (targetEntity == null || targetEntity.IsAlly(skillUserInfo))
                        targetEntity = null;
                    if (targetEntity != null && !targetEntity.IsDead())
                        targetEntity.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                    break;
                case SkillBuffType.BuffToNearbyPartyMembers:
                    if (skillUserInfo.PartyId > 0)
                    {
                        tempCharacters = skillUser.FindAliveEntities<BaseCharacterEntity>(buffDistance.GetAmount(skillLevel), true, false, false, overlapMask);
                        foreach (BaseCharacterEntity applyBuffCharacter in tempCharacters)
                        {
                            if (skillUserInfo.PartyId != applyBuffCharacter.GetInfo().PartyId)
                                continue;
                            applyBuffCharacter.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                        }
                    }
                    skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                    break;
                case SkillBuffType.BuffToPartyMember:
                    if (!skillUser.CurrentGameManager.TryGetEntityByObjectId(targetObjectId, out targetEntity))
                        targetEntity = null;
                    if (buffToUserIfNoTarget && (skillUserInfo.PartyId <= 0 || targetEntity == null || skillUserInfo.PartyId != targetEntity.GetInfo().PartyId))
                        targetEntity = skillUser;
                    if (targetEntity != null && !targetEntity.IsDead())
                        targetEntity.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUserInfo, weapon);
                    break;
            }
        }

        public override SkillType SkillType
        {
            get { return skillType; }
        }

        public override bool IsAttack
        {
            get { return skillAttackType != SkillAttackType.None; }
        }

        public override float GetCastDistance(BaseCharacterEntity skillUser, int skillLevel, bool isLeftHand)
        {
            if (TryGetDamageInfo(skillUser, isLeftHand, out DamageInfo damageInfo))
                return damageInfo.GetDistance();
            return buffDistance.GetAmount(skillLevel);
        }

        public override float GetCastFov(BaseCharacterEntity skillUser, int skillLevel, bool isLeftHand)
        {
            if (TryGetDamageInfo(skillUser, isLeftHand, out DamageInfo damageInfo))
                return damageInfo.GetFov();
            return 360f;
        }

        public override bool TryGetBaseAttackDamageAmount(ICharacterData skillUser, int skillLevel, bool isLeftHand, out KeyValuePair<DamageElement, MinMaxFloat> result)
        {
            switch (skillAttackType)
            {
                case SkillAttackType.Normal:
                    result = GameDataHelpers.GetDamageWithEffectiveness(CacheEffectivenessAttributes, skillUser.GetCaches().Attributes, damageAmount.ToKeyValuePair(skillLevel, 1f));
                    return true;
                case SkillAttackType.BasedOnWeapon:
                    if (isLeftHand && skillUser.GetCaches().LeftHandWeaponDamage.HasValue)
                    {
                        result = skillUser.GetCaches().LeftHandWeaponDamage.Value;
                        return true;
                    }
                    result = skillUser.GetCaches().RightHandWeaponDamage.Value;
                    return true;
            }
            return base.TryGetBaseAttackDamageAmount(skillUser, skillLevel, isLeftHand, out result);
        }

        public override bool TryGetAttackWeaponDamageInflictions(ICharacterData skillUser, int skillLevel, out Dictionary<DamageElement, float> result)
        {
            if (IsAttack)
            {
                result = GameDataHelpers.CombineDamageInflictions(weaponDamageInflictions, new Dictionary<DamageElement, float>(), skillLevel);
                return true;
            }
            return base.TryGetAttackWeaponDamageInflictions(skillUser, skillLevel, out result);
        }

        public override bool TryGetAttackWeaponDamageMultiplicator(ICharacterData skillUser, int skillLevel, out float result)
        {
            if (IsAttack)
            {
                result = weaponDamageMultiplicator.GetAmount(skillLevel);
                return true;
            }
            return base.TryGetAttackWeaponDamageMultiplicator(skillUser, skillLevel, out result);
        }

        public override bool TryGetAttackAdditionalDamageAmounts(ICharacterData skillUser, int skillLevel, out Dictionary<DamageElement, MinMaxFloat> result)
        {
            if (IsAttack)
            {
                result = GameDataHelpers.CombineDamages(additionalDamageAmounts, new Dictionary<DamageElement, MinMaxFloat>(), skillLevel, 1f);
                return true;
            }
            return base.TryGetAttackAdditionalDamageAmounts(skillUser, skillLevel, out result);
        }

        public override bool IsIncreaseAttackDamageAmountsWithBuffs(ICharacterData skillUser, int skillLevel)
        {
            return increaseDamageAmountsWithBuffs;
        }

        public override bool TryGetBuff(out Buff buff)
        {
            if (skillType == SkillType.Passive || skillBuffType != SkillBuffType.None)
            {
                buff = this.buff;
                return true;
            }
            return base.TryGetBuff(out buff);
        }

        public override bool TryGetDebuff(out Buff debuff)
        {
            if (IsAttack && isDebuff)
            {
                debuff = this.debuff;
                return true;
            }
            return base.TryGetDebuff(out debuff);
        }

        public override bool TryGetSummon(out SkillSummon summon)
        {
            if (this.summon.MonsterCharacterEntity != null)
            {
                summon = this.summon;
                return true;
            }
            else if (this.summon.AddressableMonsterCharacterEntity.IsDataValid())
            {
                summon = this.summon;
                return true;
            }
            return base.TryGetSummon(out summon);
        }

        public override bool TryGetMount(out SkillMount mount)
        {
            if (this.mount.MountEntity != null)
            {
                mount = this.mount;
                return true;
            }
            else if (this.mount.AddressableMountEntity.IsDataValid())
            {
                mount = this.mount;
                return true;
            }
            return base.TryGetMount(out mount);
        }

        public override bool TryGetItemCraft(out ItemCraft itemCraft)
        {
            if (this.itemCraft.CraftingItem != null)
            {
                itemCraft = this.itemCraft;
                return true;
            }
            return base.TryGetItemCraft(out itemCraft);
        }

        public override bool TryGetAttackStatusEffectApplyings(out StatusEffectApplying[] statusEffectApplyings)
        {
            if (IsAttack)
            {
                statusEffectApplyings = attackStatusEffects;
                return true;
            }
            return base.TryGetAttackStatusEffectApplyings(out statusEffectApplyings);
        }

        public override HarvestType HarvestType
        {
            get { return harvestType; }
        }

        public override IncrementalMinMaxFloat HarvestDamageAmount
        {
            get { return harvestDamageAmount; }
        }

        public override bool RequiredTarget
        {
            get { 
                return skillBuffType == SkillBuffType.BuffToTarget || skillBuffType == SkillBuffType.BuffToAlly || skillBuffType == SkillBuffType.BuffToEnemy || skillBuffType == SkillBuffType.BuffToPartyMember; 
            }
        }

        public override bool TryGetDamageInfo(BaseCharacterEntity skillUser, bool isLeftHand, out DamageInfo damageInfo)
        {
            switch (skillAttackType)
            {
                case SkillAttackType.Normal:
                    damageInfo = this.damageInfo;
                    return true;
                case SkillAttackType.BasedOnWeapon:
                    damageInfo = skillUser.GetWeaponDamageInfo(ref isLeftHand);
                    return true;
            }
            damageInfo = this.damageInfo;
            return false;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            damageInfo.PrepareRelatesData();
        }

        public override bool Validate()
        {
            bool hasChanges = false;
#pragma warning disable CS0612 // Type or member is obsolete
            if (skillType == SkillType.CraftItem)
            {
                skillType = SkillType.Active;
                hasChanges = true;
            }
#pragma warning restore CS0612 // Type or member is obsolete
            return hasChanges || base.Validate();
        }

        public override Transform GetApplyTransform(BaseCharacterEntity skillUser, bool isLeftHand)
        {
            if (TryGetDamageInfo(skillUser, isLeftHand, out DamageInfo damageInfo))
                return damageInfo.GetDamageTransform(skillUser, isLeftHand);
            return base.GetApplyTransform(skillUser, isLeftHand);
        }

        public override bool CanUse(BaseCharacterEntity skillUser, int level, bool isLeftHand, uint targetObjectId, out UITextKeys gameMessage, bool isItem = false)
        {
            EntityInfo instigator = skillUser.GetInfo();
            bool foundTarget = skillUser.CurrentGameManager.TryGetEntityByObjectId(targetObjectId, out BaseCharacterEntity targetEntity) && !targetEntity.IsDead();
            switch (skillBuffType)
            {
                case SkillBuffType.BuffToTarget:
                    if (!foundTarget && !buffToUserIfNoTarget)
                    {
                        // No target to buff
                        gameMessage = UITextKeys.UI_ERROR_NO_SKILL_TARGET;
                        return false;
                    }
                    break;
                case SkillBuffType.BuffToAlly:
                    if ((!foundTarget && !buffToUserIfNoTarget) || (foundTarget && !targetEntity.IsAlly(instigator)))
                    {
                        // No target to buff or it is not a enemy
                        gameMessage = UITextKeys.UI_ERROR_NO_SKILL_TARGET;
                        return false;
                    }
                    break;
                case SkillBuffType.BuffToEnemy:
                    if (!foundTarget || targetEntity.IsAlly(skillUser.GetInfo()))
                    {
                        // No target to buff or it is not a ally
                        gameMessage = UITextKeys.UI_ERROR_NO_SKILL_TARGET;
                        return false;
                    }
                    break;
                case SkillBuffType.BuffToPartyMember:
                    if ((!foundTarget && !buffToUserIfNoTarget) || (foundTarget && (instigator.PartyId <= 0 || instigator.PartyId != targetEntity.GetInfo().PartyId)))
                    {
                        // No target to buff or it is not a party member
                        gameMessage = UITextKeys.UI_ERROR_NO_SKILL_TARGET;
                        return false;
                    }
                    break;
            }
            bool canUse = base.CanUse(skillUser, level, isLeftHand, targetObjectId, out gameMessage, isItem);
            if (!canUse && gameMessage == UITextKeys.UI_ERROR_NO_SKILL_TARGET && buffToUserIfNoTarget)
            {
                // Still allow to use skill but it's going to set applies target to skill user
                gameMessage = UITextKeys.NONE;
                return true;
            }
            return canUse;
        }
    }
}
