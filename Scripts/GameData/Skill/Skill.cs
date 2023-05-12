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
        }

        [Category("Skill Settings")]
        public SkillType skillType;

        [Category(3, "Attacking")]
        public SkillAttackType skillAttackType;
        public DamageInfo damageInfo;
        public DamageIncremental damageAmount;
        public DamageEffectivenessAttribute[] effectivenessAttributes;
        public DamageInflictionIncremental[] weaponDamageInflictions;
        public DamageIncremental[] additionalDamageAmounts;
        [FormerlySerializedAs("increaseDamageWithBuffs")]
        public bool increaseDamageAmountsWithBuffs;
        public bool isDebuff;
        public Buff debuff;
        public HarvestType harvestType;
        public IncrementalMinMaxFloat harvestDamageAmount;

        [Category(4, "Buff")]
        public SkillBuffType skillBuffType;
        public IncrementalFloat buffDistance;
        public bool buffToUserIfNoTarget = true;
        [FormerlySerializedAs("canBuffEnemy")]
        public bool canBuffEnemy = false;
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

        protected override void ApplySkillImplement(
            BaseCharacterEntity skillUser,
            int skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            uint targetObjectId,
            AimPosition aimPosition,
            DamageOriginPreparedDelegate onDamageOriginPrepared,
            DamageHitDelegate onDamageHit)
        {
            // Craft item
            if (skillType == SkillType.CraftItem &&
                skillUser is BasePlayerCharacterEntity)
            {
                // Apply craft skill at server only
                if (!skillUser.IsServer)
                    return;
                BasePlayerCharacterEntity castedCharacter = skillUser as BasePlayerCharacterEntity;
                UITextKeys gameMessage;
                if (!itemCraft.CanCraft(castedCharacter, out gameMessage))
                {
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(skillUser.ConnectionId, gameMessage);
                    return;
                }
                itemCraft.CraftItem(castedCharacter);
                return;
            }

            // Apply skills only when it's active skill
            if (skillType != SkillType.Active)
                return;

            // Apply buff, summons at server only
            if (skillUser.IsServer)
            {
                ApplySkillBuff(skillUser, skillLevel, weapon, targetObjectId);
                ApplySkillSummon(skillUser, skillLevel);
                ApplySkillMount(skillUser, skillLevel);
            }

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
                    aimPosition,
                    onDamageOriginPrepared,
                    onDamageHit);
            }
        }

        protected void ApplySkillBuff(BaseCharacterEntity skillUser, int skillLevel, CharacterItem weapon, uint targetObjectId)
        {
            if (skillUser.IsDead() || !skillUser.IsServer || skillLevel <= 0)
                return;
            int overlapMask = GameInstance.Singleton.playerLayer.Mask | GameInstance.Singleton.playingLayer.Mask | GameInstance.Singleton.monsterLayer.Mask;
            EntityInfo instigator = skillUser.GetInfo();
            List<BaseCharacterEntity> tempCharacters;
            switch (skillBuffType)
            {
                case SkillBuffType.BuffToUser:
                    skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, instigator, weapon);
                    break;
                case SkillBuffType.BuffToNearbyAllies:
                    tempCharacters = skillUser.FindAliveEntities<BaseCharacterEntity>(buffDistance.GetAmount(skillLevel), true, false, false, overlapMask);
                    foreach (BaseCharacterEntity applyBuffCharacter in tempCharacters)
                    {
                        applyBuffCharacter.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, instigator, weapon);
                    }
                    skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, instigator, weapon);
                    break;
                case SkillBuffType.BuffToNearbyCharacters:
                    tempCharacters = skillUser.FindAliveEntities<BaseCharacterEntity>(buffDistance.GetAmount(skillLevel), true, false, true, overlapMask);
                    foreach (BaseCharacterEntity applyBuffCharacter in tempCharacters)
                    {
                        applyBuffCharacter.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, instigator, weapon);
                    }
                    skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, instigator, weapon);
                    break;
                case SkillBuffType.BuffToTarget:
                    BaseCharacterEntity targetEntity = null;
                    if (buffToUserIfNoTarget && !skillUser.CurrentGameManager.TryGetEntityByObjectId(targetObjectId, out targetEntity))
                        targetEntity = skillUser;
                    if (targetEntity != null && !targetEntity.IsDead())
                        targetEntity.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, instigator, weapon);
                    break;
                case SkillBuffType.Toggle:
                    int indexOfBuff = skillUser.IndexOfBuff(DataId, BuffType.SkillBuff);
                    if (indexOfBuff >= 0)
                        skillUser.Buffs.RemoveAt(indexOfBuff);
                    else
                        skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, instigator, weapon);
                    break;
            }
        }

        protected void ApplySkillSummon(BaseCharacterEntity skillUser, int skillLevel)
        {
            if (skillUser.IsDead() || !skillUser.IsServer || skillLevel <= 0)
                return;
            int i;
            int amountEachTime = summon.AmountEachTime.GetAmount(skillLevel);
            for (i = 0; i < amountEachTime; ++i)
            {
                CharacterSummon newSummon = CharacterSummon.Create(SummonType.Skill, DataId);
                newSummon.Summon(skillUser, summon.Level.GetAmount(skillLevel), summon.Duration.GetAmount(skillLevel));
                skillUser.Summons.Add(newSummon);
            }
            int count = 0;
            for (i = 0; i < skillUser.Summons.Count; ++i)
            {
                if (skillUser.Summons[i].dataId == DataId)
                    ++count;
            }
            int maxStack = summon.MaxStack.GetAmount(skillLevel);
            int unSummonAmount = count > maxStack ? count - maxStack : 0;
            CharacterSummon tempSummon;
            for (i = unSummonAmount; i > 0; --i)
            {
                int summonIndex = skillUser.IndexOfSummon(DataId, SummonType.Skill);
                tempSummon = skillUser.Summons[summonIndex];
                if (summonIndex >= 0)
                {
                    skillUser.Summons.RemoveAt(summonIndex);
                    tempSummon.UnSummon(skillUser);
                }
            }
        }

        protected void ApplySkillMount(BaseCharacterEntity skillUser, int skillLevel)
        {
            if (skillUser.IsDead() || !skillUser.IsServer || skillLevel <= 0)
                return;

            skillUser.Mount(mount.MountEntity);
        }

        public override SkillType SkillType
        {
            get { return skillType; }
        }

        public override bool IsAttack
        {
            get { return skillAttackType != SkillAttackType.None; }
        }

        public override bool IsBuff
        {
            get { return skillType == SkillType.Passive || skillBuffType != SkillBuffType.None; }
        }

        public override bool IsDebuff
        {
            get { return IsAttack && isDebuff; }
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

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, int skillLevel, bool isLeftHand)
        {
            switch (skillAttackType)
            {
                case SkillAttackType.Normal:
                    return damageAmount.ToKeyValuePair(skillLevel, 1f, GetEffectivenessDamage(skillUser));
                case SkillAttackType.BasedOnWeapon:
                    return skillUser.GetWeaponDamages(ref isLeftHand);
            }
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        }

        public override Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, int skillLevel)
        {
            if (!IsAttack)
                return new Dictionary<DamageElement, float>();
            return GameDataHelpers.CombineDamageInflictions(weaponDamageInflictions, new Dictionary<DamageElement, float>(), skillLevel);
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, int skillLevel)
        {
            if (!IsAttack)
                return new Dictionary<DamageElement, MinMaxFloat>();
            return GameDataHelpers.CombineDamages(additionalDamageAmounts, new Dictionary<DamageElement, MinMaxFloat>(), skillLevel, 1f);
        }

        public override bool IsIncreaseAttackDamageAmountsWithBuffs(ICharacterData skillUser, int skillLevel)
        {
            return increaseDamageAmountsWithBuffs;
        }

        public override HarvestType HarvestType
        {
            get { return harvestType; }
        }

        public override IncrementalMinMaxFloat HarvestDamageAmount
        {
            get { return harvestDamageAmount; }
        }

        protected float GetEffectivenessDamage(ICharacterData skillUser)
        {
            return GameDataHelpers.GetEffectivenessDamage(CacheEffectivenessAttributes, skillUser);
        }

        public override Buff Buff
        {
            get
            {
                if (!IsBuff)
                    return Buff.Empty;
                return buff;
            }
        }

        public override Buff Debuff
        {
            get
            {
                if (!IsDebuff)
                    return Buff.Empty;
                return debuff;
            }
        }

        public override SkillSummon Summon
        {
            get { return summon; }
        }

        public override SkillMount Mount
        {
            get { return mount; }
        }

        public override ItemCraft ItemCraft
        {
            get { return itemCraft; }
        }

        public override bool RequiredTarget
        {
            get { return skillBuffType == SkillBuffType.BuffToTarget; }
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
            return true;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddCharacterEntities(summon.MonsterEntity);
            GameInstance.AddVehicleEntities(mount.MountEntity);
            GameInstance.AddItems(itemCraft.CraftingItem);
            GameInstance.AddItems(itemCraft.RequireItems);
            GameInstance.AddCurrencies(itemCraft.RequireCurrencies);
            damageInfo.PrepareRelatesData();
        }

        public override Transform GetApplyTransform(BaseCharacterEntity skillUser, bool isLeftHand)
        {
            if (TryGetDamageInfo(skillUser, isLeftHand, out DamageInfo damageInfo))
                return damageInfo.GetDamageTransform(skillUser, isLeftHand);
            return base.GetApplyTransform(skillUser, isLeftHand);
        }

        public override bool CanUse(BaseCharacterEntity character, int level, bool isLeftHand, uint targetObjectId, out UITextKeys gameMessage, bool isItem = false)
        {
            BaseCharacterEntity targetEntity;
            if (RequiredTarget && !canBuffEnemy && character.CurrentGameManager.TryGetEntityByObjectId(targetObjectId, out targetEntity) && targetEntity.IsEnemy(character.GetInfo()))
            {
                // Cannot buff enemy
                gameMessage = UITextKeys.UI_ERROR_NO_SKILL_TARGET;
                return false;
            }
            bool canUse = base.CanUse(character, level, isLeftHand, targetObjectId, out gameMessage, isItem);
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
