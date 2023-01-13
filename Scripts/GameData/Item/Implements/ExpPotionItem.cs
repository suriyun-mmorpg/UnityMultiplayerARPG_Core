using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.EXP_POTION_ITEM_FILE, menuName = GameDataMenuConsts.EXP_POTION_ITEM_MENU, order = GameDataMenuConsts.EXP_POTION_ITEM_ORDER)]
    public class ExpPotionItem : BaseItem, IPotionItem
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
        private Buff buff = Buff.Empty;
        public Buff Buff
        {
            get { return buff; }
        }

        [SerializeField]
        private int exp = 0;
        public int Exp
        {
            get { return exp; }
        }

        [SerializeField]
        private string autoUseSettingKey;
        public string AutoUseKey
        {
            get { return autoUseSettingKey; }
        }

        [SerializeField]
        private float useItemCooldown = 0f;
        public float UseItemCooldown
        {
            get { return useItemCooldown; }
        }

        public void UseItem(BaseCharacterEntity characterEntity, int itemIndex, CharacterItem characterItem)
        {
            if (!characterEntity.CanUseItem() || characterItem.level <= 0 || !characterEntity.DecreaseItemsByIndex(itemIndex, 1, false))
                return;
            characterEntity.FillEmptySlots();
            characterEntity.ApplyBuff(DataId, BuffType.PotionBuff, characterItem.level, characterEntity.GetInfo(), null);
            characterEntity.RewardExp(new Reward()
            {
                exp = Exp
            }, 1, RewardGivenType.None);
        }

        public bool HasCustomAimControls()
        {
            return false;
        }

        public AimPosition UpdateAimControls(Vector2 aimAxes, params object[] data)
        {
            return default;
        }

        public void FinishAimControls(bool isCancel)
        {

        }

        public bool IsChanneledAbility()
        {
            return false;
        }
    }
}
