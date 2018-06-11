using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

[RequireComponent(typeof(Rigidbody))]
public class MissileDamageEntity : BaseDamageEntity
{
    protected float missileDistance;
    [SerializeField]
    protected SyncFieldFloat missileSpeed = new SyncFieldFloat();

    private Rigidbody cacheRigidbody;
    public Rigidbody CacheRigidbody
    {
        get
        {
            if (cacheRigidbody == null)
                cacheRigidbody = GetComponent<Rigidbody>();
            return cacheRigidbody;
        }
    }

    public void SetupDamage(
        BaseCharacterEntity attacker,
        Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
        CharacterBuff debuff,
        int hitEffectsId,
        float missileDistance,
        float missileSpeed)
    {
        SetupDamage(attacker, allDamageAmounts, debuff, hitEffectsId);
        this.missileDistance = missileDistance;
        this.missileSpeed.Value = missileSpeed;

        if (missileDistance > 0 && missileSpeed > 0)
            NetworkDestroy(missileDistance / missileSpeed);
        else
            NetworkDestroy();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        CacheRigidbody.velocity = CacheTransform.forward * missileSpeed.Value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        var characterEntity = other.GetComponent<BaseCharacterEntity>();
        if (characterEntity == null || characterEntity == attacker || characterEntity.CurrentHp <= 0)
            return;

        if (attacker is MonsterCharacterEntity && characterEntity is MonsterCharacterEntity)
            return;

        ApplyDamageTo(characterEntity);
        NetworkDestroy();
    }
}
