using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MissileDamageEntity : DamageEntity
{
    protected float missileDistance;
    protected float missileSpeed;

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

    public void SetupDamage(CharacterEntity attacker,
        Dictionary<DamageElement, DamageAmount> allDamageAttributes,
        CharacterBuff debuff,
        float missileDistance,
        float missileSpeed)
    {
        SetupDamage(attacker, allDamageAttributes, debuff);
        this.missileDistance = missileDistance;
        this.missileSpeed = missileSpeed;
        CacheRigidbody.velocity = attacker.CacheTransform.forward * missileSpeed;

        if (missileDistance > 0)
            NetworkDestroy(missileSpeed / missileDistance);
        else
            NetworkDestroy();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        var characterEntity = other.GetComponent<CharacterEntity>();
        if (characterEntity == null)
            return;

        ApplyDamageTo(characterEntity);
        NetworkDestroy();
    }
}
