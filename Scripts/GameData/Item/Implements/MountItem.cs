using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Mount Item", menuName = "Create GameData/Item/Mount Item", order = -4883)]
    public class MountItem : BaseItem, IMountItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_MOUNT.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Mount; }
        }

        [SerializeField]
        private VehicleEntity mountEntity;
        public VehicleEntity MountEntity
        {
            get { return mountEntity; }
        }

        public void UseItem(BaseCharacterEntity characterEntity, short itemIndex, CharacterItem characterItem)
        {
            if (!characterEntity.CanUseItem() || characterItem.level <= 0)
                return;

            characterEntity.Mount(MountEntity);
        }
    }
}
