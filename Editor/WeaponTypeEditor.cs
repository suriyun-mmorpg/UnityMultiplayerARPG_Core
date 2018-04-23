using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(WeaponType))]
[CanEditMultipleObjects]
public class WeaponTypeEditor : BaseCustomEditor
{
    private static WeaponType cacheWeaponType;
    protected override void SetFieldCondition()
    {
        if (cacheWeaponType == null)
            cacheWeaponType = CreateInstance<WeaponType>();
        ShowOnEnum(cacheWeaponType.GetMemberName(a => a.equipType), WeaponItemEquipType.OneHandCanDual.ToString(), cacheWeaponType.GetMemberName(a => a.leftHandAttackAnimations));
    }
}
