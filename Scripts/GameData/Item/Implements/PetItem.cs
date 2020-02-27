using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Pet Item", menuName = "Create GameData/Item/Pet Item", order = -4884)]
    public class PetItem : BaseItem, IPetItem
    {
        public override ItemType ItemType
        {
            get { return ItemType.Pet; }
        }

        [SerializeField]
        private BaseMonsterCharacterEntity petEntity;
        public BaseMonsterCharacterEntity PetEntity
        {
            get { return petEntity; }
        }

        public void UseItem(BaseCharacterEntity characterEntity, short itemIndex, CharacterItem characterItem)
        {
            if (!characterEntity.CanUseItem() || characterItem.level <= 0 || !characterEntity.DecreaseItemsByIndex(itemIndex, 1))
                return;
            // Clear all summoned pets
            CharacterSummon tempSummon;
            for (int i = 0; i < characterEntity.Summons.Count; ++i)
            {
                tempSummon = characterEntity.Summons[i];
                if (tempSummon.type != SummonType.Pet)
                    continue;
                characterEntity.Summons.RemoveAt(i);
                tempSummon.UnSummon(characterEntity);
            }
            // Summon new pet
            CharacterSummon newSummon = CharacterSummon.Create(SummonType.Pet, DataId);
            newSummon.Summon(characterEntity, characterItem.level, 0f, characterItem.exp);
            characterEntity.Summons.Add(newSummon);
        }
    }
}
