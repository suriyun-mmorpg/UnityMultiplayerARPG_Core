using UnityEngine;

public enum GameEffectCollectionType
{
    WeaponHit,
    SkillHit,
}

[System.Serializable]
public class GameEffectCollection
{
    private const int WEAPON_HIT_ID_START = 0;
    private const int SKILL_HIT_ID_START = 1000;
    private static int weaponHitEffectIdCount = -1;
    private static int skillHitEffectIdCount = -1;
    protected int? id;
    public int Id
    {
        get { return !id.HasValue ? -1 : id.Value; }
    }

    public GameEffect[] effects;

    /// <summary>
    /// Initialize effect id, will return false if it's already initialized
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool Initialize(GameEffectCollectionType type)
    {
        if (effects == null || effects.Length == 0 || id.HasValue)
            return false;
        
        switch (type)
        {
            case GameEffectCollectionType.WeaponHit:
                ++weaponHitEffectIdCount;
                id = WEAPON_HIT_ID_START + weaponHitEffectIdCount;
                break;
            case GameEffectCollectionType.SkillHit:
                ++skillHitEffectIdCount;
                id = SKILL_HIT_ID_START + skillHitEffectIdCount;
                break;
        }
        return true;
    }
}
