using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

[System.Serializable]
public class CharacterSkill
{
    public static readonly CharacterSkill Empty = new CharacterSkill();
    public int dataId;
    public short level;
    public float coolDownRemainsDuration;
    [System.NonSerialized]
    private int dirtyDataId;
    [System.NonSerialized]
    private Skill cacheSkill;

    private void MakeCache()
    {
        if (!GameInstance.Skills.ContainsKey(dataId))
        {
            cacheSkill = null;
            return;
        }
        if (dirtyDataId != dataId)
        {
            dirtyDataId = dataId;
            cacheSkill = GameInstance.Skills.TryGetValue(dataId, out cacheSkill) ? cacheSkill : null;
        }
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

    public void LevelUp(short level)
    {
        this.level += level;
    }

    public bool CanUse(ICharacterData character)
    {
        var skill = GetSkill();
        if (skill == null)
            return false;
        var available = true;
        switch (skill.skillType)
        {
            case SkillType.Active:
                var availableWeapons = skill.availableWeapons;
                available = availableWeapons == null || availableWeapons.Length == 0;
                if (!available)
                {
                    var rightWeaponItem = character.EquipWeapons.rightHand.GetWeaponItem();
                    var leftWeaponItem = character.EquipWeapons.leftHand.GetWeaponItem();
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
                break;
            case SkillType.CraftItem:
                if (!skill.itemCraft.CanCraft(character))
                    return false;
                break;
            default:
                return false;
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

    public static CharacterSkill Create(Skill skill, short level)
    {
        var newSkill = new CharacterSkill();
        newSkill.dataId = skill.DataId;
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
        newValue.dataId = reader.GetInt();
        newValue.level = reader.GetShort();
        newValue.coolDownRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.dataId);
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
