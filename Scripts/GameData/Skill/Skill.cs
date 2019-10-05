using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
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
    }

    [CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill", order = -4996)]
    public partial class Skill : BaseSkill
    {
        public SkillType skillType;

        [Header("Attack")]
        public SkillAttackType skillAttackType;
        public GameEffectCollection hitEffects;
        public DamageInfo damageInfo;
        public DamageEffectivenessAttribute[] effectivenessAttributes;
        public DamageIncremental damageAmount;
        public DamageInflictionIncremental[] weaponDamageInflictions;
        public DamageIncremental[] additionalDamageAmounts;
        public bool isDebuff;
        public Buff debuff;

        [Header("Buffs")]
        public SkillBuffType skillBuffType;
        public IncrementalFloat buffDistance;
        public Buff buff;

        [Header("Summon")]
        public SkillSummon summon;

        [Header("Mount")]
        public SkillMount mount;

        [Header("Craft")]
        public ItemCraft itemCraft;

        private Dictionary<Attribute, float> cacheEffectivenessAttributes;
        public Dictionary<Attribute, float> CacheEffectivenessAttributes
        {
            get
            {
                if (cacheEffectivenessAttributes == null)
                    cacheEffectivenessAttributes = GameDataHelpers.CombineDamageEffectivenessAttributes(effectivenessAttributes, new Dictionary<Attribute, float>());
                return cacheEffectivenessAttributes;
            }
        }

        public override bool Validate()
        {
            return GameDataMigration.MigrateBuffArmor(buff, out buff) ||
                GameDataMigration.MigrateBuffArmor(debuff, out debuff);
        }

        public override void ApplySkill(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, Vector3 aimPosition)
        {
            // Craft item
            if (skillType == SkillType.CraftItem &&
                skillUser is BasePlayerCharacterEntity)
            {
                BasePlayerCharacterEntity castedCharacter = skillUser as BasePlayerCharacterEntity;
                GameMessage.Type gameMessageType;
                if (!itemCraft.CanCraft(castedCharacter, out gameMessageType))
                    skillUser.gameManager.SendServerGameMessage(skillUser.ConnectionId, gameMessageType);
                else
                    itemCraft.CraftItem(castedCharacter);
                return;
            }

            // Apply skills only when it's active skill
            if (skillType != SkillType.Active)
                return;

            // Apply buff, summons at server only
            if (skillUser.IsServer)
            {
                ApplySkillBuff(skillUser, skillLevel);
                ApplySkillSummon(skillUser, skillLevel);
                ApplySkillMount(skillUser, skillLevel);
            }

            // Apply attack skill
            if (IsAttack())
            {
                if (skillUser.IsServer)
                {
                    // Increase damage with ammo damage
                    Dictionary<DamageElement, MinMaxFloat> increaseDamages;
                    skillUser.ReduceAmmo(weapon, isLeftHand, out increaseDamages);
                    if (increaseDamages != null)
                        damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, increaseDamages);
                }

                // Apply debuff
                CharacterBuff debuff = CharacterBuff.Empty;
                if (isDebuff)
                    debuff = CharacterBuff.Create(BuffType.SkillDebuff, DataId, skillLevel);

                // Get damage info

                // Launch damage entity to apply damage to other characters
                skillUser.LaunchDamageEntity(
                    isLeftHand,
                    weapon,
                    GetDamageInfo(skillUser, isLeftHand),
                    damageAmounts,
                    debuff,
                    this,
                    skillLevel,
                    aimPosition,
                    Vector3.zero);
            }
        }

        protected void ApplySkillBuff(BaseCharacterEntity skillUser, short skillLevel)
        {
            if (skillUser.IsDead() || !skillUser.IsServer || skillLevel <= 0)
                return;

            List<BaseCharacterEntity> tempCharacters;
            switch (skillBuffType)
            {
                case SkillBuffType.BuffToUser:
                    skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel);
                    break;
                case SkillBuffType.BuffToNearbyAllies:
                    tempCharacters = skillUser.FindAliveCharacters<BaseCharacterEntity>(buffDistance.GetAmount(skillLevel), true, false, false);
                    foreach (BaseCharacterEntity applyBuffCharacter in tempCharacters)
                    {
                        applyBuffCharacter.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel);
                    }
                    skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel);
                    break;
                case SkillBuffType.BuffToNearbyCharacters:
                    tempCharacters = skillUser.FindAliveCharacters<BaseCharacterEntity>(buffDistance.GetAmount(skillLevel), true, false, true);
                    foreach (BaseCharacterEntity applyBuffCharacter in tempCharacters)
                    {
                        applyBuffCharacter.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel);
                    }
                    skillUser.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel);
                    break;
            }
        }

        protected void ApplySkillSummon(BaseCharacterEntity skillUser, short skillLevel)
        {
            if (skillUser.IsDead() || !skillUser.IsServer || skillLevel <= 0)
                return;
            int i = 0;
            int amountEachTime = summon.amountEachTime.GetAmount(skillLevel);
            for (i = 0; i < amountEachTime; ++i)
            {
                CharacterSummon newSummon = CharacterSummon.Create(SummonType.Skill, DataId);
                newSummon.Summon(skillUser, summon.level.GetAmount(skillLevel), summon.duration.GetAmount(skillLevel));
                skillUser.Summons.Add(newSummon);
            }
            int count = 0;
            for (i = 0; i < skillUser.Summons.Count; ++i)
            {
                if (skillUser.Summons[i].dataId == DataId)
                    ++count;
            }
            int maxStack = summon.maxStack.GetAmount(skillLevel);
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

        protected void ApplySkillMount(BaseCharacterEntity skillUser, short skillLevel)
        {
            if (skillUser.IsDead() || !skillUser.IsServer || skillLevel <= 0)
                return;

            skillUser.Mount(mount.mountEntity);
        }

        protected DamageInfo GetDamageInfo(BaseCharacterEntity skillUser, bool isLeftHand)
        {
            switch (skillAttackType)
            {
                case SkillAttackType.Normal:
                    // Get damage info from skill
                    return damageInfo;
                case SkillAttackType.BasedOnWeapon:
                    // Assign damage data
                    if (skillUser is BaseMonsterCharacterEntity)
                    {
                        // Monster has its own damage info
                        return (skillUser as BaseMonsterCharacterEntity).MonsterDatabase.damageInfo;
                    }
                    else
                    {
                        // Get damage info from weapon
                        return skillUser.GetAvailableWeapon(ref isLeftHand).GetWeaponItem().WeaponType.damageInfo;
                    }
            }
            return default(DamageInfo);
        }

        public override SkillType GetSkillType()
        {
            return skillType;
        }

        public override bool IsAttack()
        {
            return skillAttackType != SkillAttackType.None;
        }

        public override bool IsBuff()
        {
            return skillBuffType != SkillBuffType.None;
        }

        public override Buff GetBuff()
        {
            if (!IsBuff())
                return default(Buff);
            return buff;
        }

        public override bool IsDebuff()
        {
            return IsAttack() && isDebuff;
        }

        public override Buff GetDebuff()
        {
            if (!IsDebuff())
                return default(Buff);
            return debuff;
        }

        public override BaseMonsterCharacterEntity GetSummonMonsterEntity()
        {
            return summon.monsterEntity;
        }

        public override MountEntity GetMountEntity()
        {
            return mount.mountEntity;
        }

        public override ItemCraft GetItemCraft()
        {
            return itemCraft;
        }

        public override GameEffectCollection GetHitEffect()
        {
            return hitEffects;
        }

        public override float GetAttackDistance(BaseCharacterEntity skillUser, bool isLeftHand, short skillLevel)
        {
            if (!IsAttack())
                return 0f;
            if (skillAttackType == SkillAttackType.Normal)
                return GetDamageInfo(skillUser, isLeftHand).GetDistance();
            return skillUser.GetAttackDistance(isLeftHand);
        }

        public override float GetAttackFov(BaseCharacterEntity skillUser, bool isLeftHand, short skillLevel)
        {
            if (!IsAttack())
                return 0f;
            if (skillAttackType == SkillAttackType.Normal)
                return GetDamageInfo(skillUser, isLeftHand).GetFov();
            return skillUser.GetAttackFov(isLeftHand);
        }

        public override void GetAttackDamages(ICharacterData skillUser, bool isLeftHand, short skillLevel, out Dictionary<DamageElement, MinMaxFloat> damageAmounts)
        {
            damageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // If it is attack skill
            if (IsAttack())
            {
                switch (skillAttackType)
                {
                    case SkillAttackType.Normal:
                        // Sum damage with skill damage because this skill damages based on itself
                        damageAmounts = GameDataHelpers.CombineDamages(
                            damageAmounts,
                            GetAttackAdditionalDamageAmounts(skillUser, skillLevel));
                        // Sum damage with additional damage amounts
                        damageAmounts = GameDataHelpers.CombineDamages(
                            damageAmounts,
                            GetBaseAttackDamageAmount(skillUser, isLeftHand, skillLevel));
                        break;
                    case SkillAttackType.BasedOnWeapon:
                        // Assign damage data
                        if (skillUser is BaseMonsterCharacterEntity)
                        {
                            // Character is monster
                            BaseMonsterCharacterEntity monsterSkillUser = skillUser as BaseMonsterCharacterEntity;
                            // Calculate all damages
                            damageAmounts = GameDataHelpers.MakeDamageWithInflictions(
                                monsterSkillUser.MonsterDatabase.damageAmount,
                                monsterSkillUser.Level, // Monster Level
                                1f, // Equipment Stats Rate, this is not based on equipment so its rate is 1f
                                GetEffectivenessDamage(skillUser),
                                GetAttackWeaponDamageInflictions(skillUser, skillLevel));
                        }
                        else
                        {
                            // Character isn't monster
                            CharacterItem weapon = skillUser.GetAvailableWeapon(ref isLeftHand);
                            // Calculate all damages
                            damageAmounts = GameDataHelpers.MakeDamageWithInflictions(
                                weapon.GetWeaponItem().damageAmount,
                                weapon.level,
                                weapon.GetEquipmentStatsRate(),
                                weapon.GetWeaponItem().GetEffectivenessDamage(skillUser),
                                GetAttackWeaponDamageInflictions(skillUser, skillLevel));
                        }
                        // Sum damage with additional damage amounts
                        damageAmounts = GameDataHelpers.CombineDamages(
                            damageAmounts,
                            GetAttackAdditionalDamageAmounts(skillUser, skillLevel));
                        break;
                }
                // Sum damage with buffs
                damageAmounts = GameDataHelpers.CombineDamages(
                    damageAmounts,
                    skillUser.GetCaches().IncreaseDamages);
            }
        }

        public override Vector3 GetDefaultAimPosition(BaseCharacterEntity skillUser, bool isLeftHand)
        {
            // No aim position set, set aim position to forward direction
            Transform damageTransform = skillUser.GetDamageTransform(GetDamageInfo(skillUser, isLeftHand).damageType, isLeftHand);
            return damageTransform.position + damageTransform.forward;
        }

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, bool isLeftHand, short skillLevel)
        {
            switch (skillAttackType)
            {
                case SkillAttackType.Normal:
                    return GameDataHelpers.MakeDamage(
                        damageAmount,
                        skillLevel,
                        1f, // Equipment Stats Rate, this is not based on equipment so its rate is 1f
                        GetEffectivenessDamage(skillUser));
                case SkillAttackType.BasedOnWeapon:
                    if (skillUser is BaseMonsterCharacterEntity)
                    {
                        // Character is monster
                        BaseMonsterCharacterEntity monsterSkillUser = skillUser as BaseMonsterCharacterEntity;
                        return GameDataHelpers.MakeDamage(
                            monsterSkillUser.MonsterDatabase.damageAmount,
                            monsterSkillUser.Level,
                            1f, // Equipment Stats Rate, this is not based on equipment so its rate is 1f
                            GetEffectivenessDamage(skillUser));
                    }
                    else
                    {
                        // Get damage amount from weapon
                        return skillUser.GetAvailableWeapon(ref isLeftHand).GetDamageAmount(skillUser);
                    }
                default:
                    return new KeyValuePair<DamageElement, MinMaxFloat>();
            }
        }

        public override Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, short skillLevel)
        {
            if (!IsAttack())
                return new Dictionary<DamageElement, float>();
            return GameDataHelpers.CombineDamageInflictions(weaponDamageInflictions, new Dictionary<DamageElement, float>(), skillLevel);
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, short skillLevel)
        {
            if (!IsAttack())
                return new Dictionary<DamageElement, MinMaxFloat>();
            return GameDataHelpers.CombineDamages(additionalDamageAmounts, new Dictionary<DamageElement, MinMaxFloat>(), skillLevel, 1f);
        }

        protected float GetEffectivenessDamage(ICharacterData skillUser)
        {
            return GameDataHelpers.GetEffectivenessDamage(CacheEffectivenessAttributes, skillUser);
        }

        public override bool HasCustomAimControls()
        {
            return false;
        }

        public override Vector3? UpdateAimControls(short skillLevel)
        {
            return null;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddDamageInfos(new DamageInfo[] { damageInfo });
            GameInstance.AddCharacterEntities(new BaseCharacterEntity[] { summon.monsterEntity });
            GameInstance.AddMountEntities(new MountEntity[] { mount.mountEntity });
            GameInstance.AddItems(new Item[] { itemCraft.CraftingItem });
        }
    }
}
