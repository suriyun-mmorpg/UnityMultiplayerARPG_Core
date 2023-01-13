using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.PET_ITEM_FILE, menuName = GameDataMenuConsts.PET_ITEM_MENU, order = GameDataMenuConsts.PET_ITEM_ORDER)]
    public partial class PetItem : BaseItem, IPetItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_PET.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Pet; }
        }

        [Category(3, "Pet Settings")]
        [SerializeField]
        private BaseMonsterCharacterEntity petEntity = null;
        public BaseMonsterCharacterEntity PetEntity
        {
            get { return petEntity; }
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
            // Clear all summoned pets
            CharacterSummon tempSummon;
            for (int i = characterEntity.Summons.Count - 1; i >= 0; --i)
            {
                tempSummon = characterEntity.Summons[i];
                if (tempSummon.type != SummonType.PetItem)
                    continue;
                characterEntity.Summons.RemoveAt(i);
                tempSummon.UnSummon(characterEntity);
            }
            // Summon new pet
            CharacterSummon newSummon = CharacterSummon.Create(SummonType.PetItem, DataId);
            newSummon.Summon(characterEntity, characterItem.level, 0f, characterItem.exp);
            characterEntity.Summons.Add(newSummon);
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

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddCharacterEntities(PetEntity);
        }
    }
}
