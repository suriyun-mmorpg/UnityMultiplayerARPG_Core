using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(WeaponType))]
    [CanEditMultipleObjects]
    public class WeaponTypeEditor : BaseCustomEditor
    {
        private static WeaponType cacheWeaponType;
        protected override void SetFieldCondition()
        {
            if (cacheWeaponType == null)
                cacheWeaponType = CreateInstance<WeaponType>();
        }
    }
}
