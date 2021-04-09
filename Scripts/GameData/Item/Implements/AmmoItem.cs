using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Ammo Item", menuName = "Create GameData/Item/Ammo Item", order = -4880)]
    public class AmmoItem : BaseItem, IAmmoItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_AMMO.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Ammo; }
        }

        [Header("Ammo Configs")]
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

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddAmmoTypes(AmmoType);
        }
    }
}
