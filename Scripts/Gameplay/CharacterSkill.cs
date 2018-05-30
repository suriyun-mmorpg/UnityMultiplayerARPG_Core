using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;

[System.Serializable]
public struct CharacterSkill
{
    public static readonly CharacterSkill Empty = new CharacterSkill();
    public string skillId;
    public int level;
    public float coolDownRemainsDuration;
    [System.NonSerialized]
    private string dirtySkillId;
    [System.NonSerialized]
    private Skill cacheSkill;

    private void MakeCache()
    {
        if (string.IsNullOrEmpty(skillId))
        {
            cacheSkill = null;
            return;
        }
        if (string.IsNullOrEmpty(dirtySkillId) || !dirtySkillId.Equals(skillId))
        {
            dirtySkillId = skillId;
            cacheSkill = GameInstance.Skills.TryGetValue(skillId, out cacheSkill) ? cacheSkill : null;
        }
    }

    public bool IsEmpty()
    {
        return Equals(Empty);
    }
    
    public Skill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public bool CanLevelUp(IPlayerCharacterData character)
    {
        return GetSkill().CanLevelUp(character, level);
    }

    public void LevelUp(int level)
    {
        this.level += level;
    }

    public bool CanUse(ICharacterData character)
    {
        var skill = GetSkill();
        if (skill == null)
            return false;
        if (skill.IsAttack() && !character.CanAttack())
            return false;
        var availableWeapons = skill.availableWeapons;
        var available = availableWeapons == null || availableWeapons.Length == 0;
        var rightWeaponItem = character.EquipWeapons.rightHand.GetWeaponItem();
        var leftWeaponItem = character.EquipWeapons.leftHand.GetWeaponItem();
        if (!available)
        {
            foreach (var availableWeapon in availableWeapons)
            {
                if (rightWeaponItem != null && rightWeaponItem.WeaponType == availableWeapon)
                {
                    available = true;
                    break;
                }
                else if (leftWeaponItem != null && leftWeaponItem.WeaponType == availableWeapon)
                {
                    available = true;
                    break;
                }
                else if (rightWeaponItem == null && leftWeaponItem == null && GameInstance.Singleton.DefaultWeaponItem.WeaponType == availableWeapon)
                {
                    available = true;
                    break;
                }
            }
        }
        return level >= 1 && coolDownRemainsDuration <= 0f && character.CurrentMp >= skill.GetConsumeMp(level) && available;
    }

    public void ReduceMp(ICharacterData character)
    {
        var consumeMp = GetSkill().GetConsumeMp(level);
        if (character.CurrentMp >= consumeMp)
            character.CurrentMp -= consumeMp;
    }

    public void Used()
    {
        coolDownRemainsDuration = GetSkill().GetCoolDownDuration(level);
    }

    public bool ShouldUpdate()
    {
        return coolDownRemainsDuration > 0f;
    }

    public void Update(float deltaTime)
    {
        coolDownRemainsDuration -= deltaTime;
    }

    public void ClearCoolDown()
    {
        coolDownRemainsDuration = 0;
    }

    public static CharacterSkill Create(Skill skill, int level)
    {
        var newSkill = new CharacterSkill();
        newSkill.skillId = skill.Id;
        newSkill.level = level;
        newSkill.coolDownRemainsDuration = 0f;
        return newSkill;
    }
}

public class NetFieldCharacterSkill : LiteNetLibNetField<CharacterSkill>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterSkill();
        newValue.skillId = reader.GetString();
        newValue.level = reader.GetInt();
        newValue.coolDownRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.skillId);
        writer.Put(Value.level);
        writer.Put(Value.coolDownRemainsDuration);
    }

    public override bool IsValueChanged(CharacterSkill newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterSkill : LiteNetLibSyncList<NetFieldCharacterSkill, CharacterSkill>
{
}
