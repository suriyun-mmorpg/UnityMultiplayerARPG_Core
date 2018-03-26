using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct EquipWeapons
{
    public CharacterItem rightHand;
    public CharacterItem leftHand;

    public float GetAttackDistance()
    {
        float? minDistance = null;
        Damage tempDamage;
        float tempDistance = 0f;
        var rightHandWeapon = rightHand.GetWeaponItem();
        var leftHandWeapon = leftHand.GetWeaponItem();
        if (rightHandWeapon != null)
        {
            tempDamage = rightHandWeapon.WeaponType.damage;
            if (TryGetAttackDistance(tempDamage, out tempDistance) && (!minDistance.HasValue || minDistance.Value > tempDistance))
                minDistance = tempDistance;
        }
        if (leftHandWeapon)
        {
            tempDamage = leftHandWeapon.WeaponType.damage;
            if (TryGetAttackDistance(tempDamage, out tempDistance) && (!minDistance.HasValue || minDistance.Value > tempDistance))
                minDistance = tempDistance;
        }
        if (!minDistance.HasValue)
        {
            tempDamage = GameInstance.Singleton.DefaultWeaponType.damage;
            if (TryGetAttackDistance(tempDamage, out tempDistance) && (!minDistance.HasValue || minDistance.Value > tempDistance))
                minDistance = tempDistance;
        }
        return minDistance.Value;
    }

    private bool TryGetAttackDistance(Damage weaponDamage, out float distance)
    {
        distance = 0f;
        switch (weaponDamage.damageType)
        {
            case DamageType.Melee:
                distance = weaponDamage.hitDistance;
                return true;
            case DamageType.Missile:
                distance = weaponDamage.missileDistance;
                return true;
        }
        return false;
    }

    public CharacterItem GetRandomedItem(out bool isLeftHand)
    {
        isLeftHand = false;
        var resultItem = CharacterItem.Create(GameInstance.Singleton.defaultWeaponItem, 1);
        var rightWeaponItem = rightHand.GetWeaponItem();
        var leftWeaponItem = leftHand.GetWeaponItem();
        if (rightWeaponItem != null && leftWeaponItem != null)
        {
            isLeftHand = Random.Range(0, 1) == 1;
            resultItem = !isLeftHand ? rightHand : leftHand;
        }
        else if (rightWeaponItem != null)
        {
            resultItem = rightHand;
            isLeftHand = false;
        }
        else if (leftWeaponItem != null)
        {
            resultItem = leftHand;
            isLeftHand = true;
        }
        return resultItem;
    }
}

public class NetFieldEquipWeapons : LiteNetLibNetField<EquipWeapons>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new EquipWeapons();
        var rightHand = new CharacterItem();
        rightHand.itemId = reader.GetString();
        rightHand.level = reader.GetInt();
        rightHand.amount = reader.GetInt();
        var leftHand = new CharacterItem();
        leftHand.itemId = reader.GetString();
        leftHand.level = reader.GetInt();
        leftHand.amount = reader.GetInt();
        newValue.rightHand = rightHand;
        newValue.leftHand = leftHand;
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.rightHand.itemId);
        writer.Put(Value.rightHand.level);
        writer.Put(Value.rightHand.amount);
        writer.Put(Value.leftHand.itemId);
        writer.Put(Value.leftHand.level);
        writer.Put(Value.leftHand.amount);
    }

    public override bool IsValueChanged(EquipWeapons newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncFieldEquipWeapons : LiteNetLibSyncField<NetFieldEquipWeapons, EquipWeapons> { }