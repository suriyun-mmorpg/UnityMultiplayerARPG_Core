using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibHighLevel;

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
        Dictionary<DamageElement, MinMaxFloat> allDamageAttributes,
        CharacterBuff debuff,
        int hitEffectsId,
        float missileDistance,
        float missileSpeed)
    {
        SetupDamage(attacker, allDamageAttributes, debuff, hitEffectsId);
        this.missileDistance = missileDistance;
        this.missileSpeed.Value = missileSpeed;

        if (missileDistance > 0 && missileSpeed > 0)
            NetworkDestroy(missileDistance / missileSpeed);
        else
            NetworkDestroy();
    }

    private void FixedUpdate()
    {
        CacheRigidbody.velocity = CacheTransform.forward * missileSpeed.Value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        var characterEntity = other.GetComponent<BaseCharacterEntity>();
        if (characterEntity == null || characterEntity == attacker || characterEntity.CurrentHp <= 0)
            return;

        ApplyDamageTo(characterEntity);
        NetworkDestroy();
    }
}
