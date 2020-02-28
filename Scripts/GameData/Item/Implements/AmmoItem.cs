using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Socket Enhancer Item", menuName = "Create GameData/Item/Socket Enhancer Item", order = -4880)]
    public class AmmoItem : BaseItem, IAmmoItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_AMMO.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Armor; }
        }

        [SerializeField]
        private AmmoType ammoType;
        public AmmoType AmmoType
        {
            get { return ammoType; }
        }

        [SerializeField]
        private DamageIncremental[] increaseDamages;
        public DamageIncremental[] IncreaseDamages
        {
            get { return increaseDamages; }
        }
    }
}
