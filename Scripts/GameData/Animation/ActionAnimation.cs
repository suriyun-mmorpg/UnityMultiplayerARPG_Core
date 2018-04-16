using UnityEngine;

public enum ActionAnimationType
{
    MonsterAttack,
    WeaponAttack,
    SkillCast,
}

[System.Serializable]
public class ActionAnimation
{
    private const int MONSTER_ATTACK_ID_START = 0;
    private const int WEAPON_ATTACK_ID_START = 1000;
    private const int SKILL_CAST_ID_START = 2000;
    private static int monsterAttackIdCount = -1;
    private static int weaponAttackIdCount = -1;
    private static int skillCastIdCount = -1;
    public AnimationClip clip;
    [Range(0f, 1f)]
    public float triggerDurationRate;
    [Tooltip("Extra duration after played animation clip")]
    public float extraDuration;
    protected int? id;
    public int Id
    {
        get { return !id.HasValue ? -1 : id.Value; }
    }

    public float TriggerDuration
    {
        get { return ClipLength * triggerDurationRate; }
    }

    public float ClipLength
    {
        get { return clip == null ? 0f : clip.length; }
    }

    /// <summary>
    /// Initialize action id, will return false if it's already initialized
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool Initialize(ActionAnimationType type)
    {
        if (id.HasValue)
            return false;

        switch (type)
        {
            case ActionAnimationType.MonsterAttack:
                ++monsterAttackIdCount;
                id = MONSTER_ATTACK_ID_START + monsterAttackIdCount;
                break;
            case ActionAnimationType.WeaponAttack:
                ++weaponAttackIdCount;
                id = WEAPON_ATTACK_ID_START + weaponAttackIdCount;
                break;
            case ActionAnimationType.SkillCast:
                ++skillCastIdCount;
                id = SKILL_CAST_ID_START + skillCastIdCount;
                break;
        }
        return true;
    }
}
