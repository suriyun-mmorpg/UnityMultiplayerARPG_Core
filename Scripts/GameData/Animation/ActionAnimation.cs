using System.Collections.Generic;
using UnityEngine;

public enum ActionAnimationType
{
    MonsterAttack,
    WeaponAttack,
    SkillCast,
}

[System.Serializable]
public struct ActionAnimationOverrideData
{
    public static readonly ActionAnimationOverrideData Empty = new ActionAnimationOverrideData();
    public CharacterModel target;
    [Tooltip("Must set it to override default animation data")]
    public AnimationClip clip;
    [Tooltip("Set it more than zero to override default trigger duration rate")]
    [Range(0f, 1f)]
    public float triggerDurationRate;
    public float extraDuration;
    [Tooltip("Set it length more than zero to override default audio clips")]
    public AudioClip[] audioClips;
    public bool IsEmpty()
    {
        return Equals(Empty);
    }
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
    protected int? id;
    public int Id
    {
        get { return !id.HasValue ? -1 : id.Value; }
    }

    [SerializeField]
    private AnimationClip clip;
    [Range(0.01f, 1f)]
    [SerializeField]
    private float triggerDurationRate;
    [Tooltip("Extra duration after played animation clip")]
    [SerializeField]
    private float extraDuration;
    [Tooltip("Audio clips playing randomly while play this animation (not loop)")]
    [SerializeField]
    private AudioClip[] audioClips;
    [Tooltip("Override clip for target model")]
    [SerializeField]
    private ActionAnimationOverrideData[] overrideData;

    private Dictionary<int, ActionAnimationOverrideData> cacheOverrideData;
    public Dictionary<int, ActionAnimationOverrideData> CacheOverrideData
    {
        get
        {
            if (cacheOverrideData == null)
            {
                cacheOverrideData = new Dictionary<int, ActionAnimationOverrideData>();
                if (overrideData != null)
                {
                    foreach (var overrideDataEntry in overrideData)
                    {
                        if (overrideDataEntry.target == null || overrideDataEntry.clip == null)
                            continue;
                        cacheOverrideData[overrideDataEntry.target.OverrideActionClipId] = overrideDataEntry;
                    }
                }
            }
            return cacheOverrideData;
        }
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

    private AudioClip GetRandomAudioClip(AudioClip[] audioClips)
    {
        AudioClip clip = null;
        if (audioClips != null && audioClips.Length > 0)
            clip = audioClips[Random.Range(0, audioClips.Length)];
        return clip;
    }

    public bool GetData(CharacterModel model, out AnimationClip clip, out float triggerDuration, out float extraDuration, out AudioClip audioClip)
    {
        clip = this.clip;
        extraDuration = this.extraDuration;
        var triggerDurationRate = this.triggerDurationRate;
        var audioClips = this.audioClips;
        ActionAnimationOverrideData overrideData;
        if (CacheOverrideData.TryGetValue(model.OverrideActionClipId, out overrideData))
        {
            clip = overrideData.clip;
            if (overrideData.triggerDurationRate > 0)
                triggerDurationRate = overrideData.triggerDurationRate;
            extraDuration = overrideData.extraDuration;
            if (overrideData.audioClips != null && overrideData.audioClips.Length > 0)
                audioClips = overrideData.audioClips;
        }
        triggerDuration = (clip != null ? clip.length : 0) * triggerDurationRate;
        audioClip = GetRandomAudioClip(audioClips);
        return clip != null;
    }
}
