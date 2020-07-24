using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Potion Item", menuName = "Create GameData/Item/Potion Item", order = -4886)]
    public partial class PotionItem : BaseItem, IPotionItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_POTION.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Potion; }
        }

        [Header("Potion Configs")]
        [SerializeField]
        private Buff buff;
        public Buff Buff
        {
            get { return buff; }
        }

        public void UseItem(BaseCharacterEntity characterEntity, short itemIndex, CharacterItem characterItem)
        {
            if (!characterEntity.CanUseItem() || characterItem.level <= 0 || !characterEntity.DecreaseItemsByIndex(itemIndex, 1))
                return;
            characterEntity.FillEmptySlots();
            characterEntity.ApplyBuff(DataId, BuffType.PotionBuff, characterItem.level, characterEntity);
        }

        public bool HasCustomAimControls()
        {
            return false;
        }

        public Vector3? UpdateAimControls(Vector2 aimAxes, params object[] data)
        {
            return null;
        }

        public void FinishAimControls(bool isCancel)
        {

        }
    }
}
