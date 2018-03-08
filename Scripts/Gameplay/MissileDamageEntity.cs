using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MissileDamageEntity : DamageEntity
{
    protected float missileDistance;
    protected float missileSpeed;

    private Rigidbody tempRigidbody;
    public Rigidbody TempRigidbody
    {
        get
        {
            if (tempRigidbody == null)
                tempRigidbody = GetComponent<Rigidbody>();
            return tempRigidbody;
        }
    }

    public void SetupDamage(CharacterEntity attacker,
        KeyValuePair<DamageElement, DamageAmount> baseDamageAttribute,
        Dictionary<DamageElement, DamageAmount> additionalDamageAttributes,
        Dictionary<string, float> effectivenessAttributes,
        CharacterBuff debuff,
        float missileDistance,
        float missileSpeed)
    {
        SetupDamage(attacker, baseDamageAttribute, additionalDamageAttributes, effectivenessAttributes, debuff);
        this.missileDistance = missileDistance;
        this.missileSpeed = missileSpeed;
        TempRigidbody.velocity = attacker.TempTransform.forward * missileSpeed;

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
