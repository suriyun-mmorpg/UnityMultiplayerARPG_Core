using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class BaseDamageEntity : RpgNetworkEntity
{
    protected BaseCharacterEntity attacker;
    protected Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;
    protected CharacterBuff debuff;
    protected int hitEffectsId;

    [SerializeField]
    private int dataId;
    public int DataId { get { return dataId; } }

    public virtual void SetupDamage(
        BaseCharacterEntity attacker,
        Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
        CharacterBuff debuff,
        int hitEffectsId)
    {
        this.attacker = attacker;
        this.allDamageAmounts = allDamageAmounts;
        this.debuff = debuff;
        this.hitEffectsId = hitEffectsId;
    }

    public virtual void ApplyDamageTo(BaseCharacterEntity target)
    {
        if (target == null)
            return;
        target.ReceiveDamage(attacker, allDamageAmounts, debuff, hitEffectsId);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && dataId != name.GenerateHashId())
        {
            dataId = name.GenerateHashId();
            EditorUtility.SetDirty(gameObject);
        }
    }
#endif
}
