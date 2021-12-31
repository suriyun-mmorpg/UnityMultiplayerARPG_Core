using System.Collections.Generic;

namespace MultiplayerARPG
{
    public struct UICharacterAttributeData
    {
        public CharacterAttribute characterAttribute;
        public float targetAmount;
        public UICharacterAttributeData(CharacterAttribute characterAttribute, float targetAmount)
        {
            this.characterAttribute = characterAttribute;
            this.targetAmount = targetAmount;
        }
        public UICharacterAttributeData(CharacterAttribute characterAttribute) : this(characterAttribute, characterAttribute.amount)
        {
        }
        public UICharacterAttributeData(Attribute attribute, short targetLevel) : this(CharacterAttribute.Create(attribute, targetLevel), targetLevel)
        {
        }
    }

    public struct UICharacterCurrencyData
    {
        public CharacterCurrency characterCurrency;
        public float targetAmount;
        public UICharacterCurrencyData(CharacterCurrency characterCurrency, float targetAmount)
        {
            this.characterCurrency = characterCurrency;
            this.targetAmount = targetAmount;
        }
        public UICharacterCurrencyData(CharacterCurrency characterCurrency) : this(characterCurrency, characterCurrency.amount)
        {
        }
        public UICharacterCurrencyData(Currency attribute, int targetAmount) : this(CharacterCurrency.Create(attribute, targetAmount), targetAmount)
        {
        }
    }

    public struct UICharacterSkillData
    {
        public CharacterSkill characterSkill;
        public short targetLevel;
        public UICharacterSkillData(CharacterSkill characterSkill, short targetLevel)
        {
            this.characterSkill = characterSkill;
            this.targetLevel = targetLevel;
        }
        public UICharacterSkillData(CharacterSkill characterSkill) : this(characterSkill, characterSkill.level)
        {
        }
        public UICharacterSkillData(BaseSkill skill, short targetLevel) : this(CharacterSkill.Create(skill, targetLevel), targetLevel)
        {
        }
    }

    public struct UICharacterItemData
    {
        public CharacterItem characterItem;
        public short targetLevel;
        public InventoryType inventoryType;
        public UICharacterItemData(CharacterItem characterItem, short targetLevel, InventoryType inventoryType)
        {
            this.characterItem = characterItem;
            this.targetLevel = targetLevel;
            this.inventoryType = inventoryType;
        }
        public UICharacterItemData(CharacterItem characterItem, InventoryType inventoryType) : this(characterItem, characterItem.level, inventoryType)
        {
        }
        public UICharacterItemData(BaseItem item, short targetLevel, InventoryType inventoryType) : this(CharacterItem.Create(item, targetLevel), targetLevel, inventoryType)
        {
        }
    }

    public struct UIOwningCharacterItemData
    {
        public InventoryType inventoryType;
        public int indexOfData;
        public UIOwningCharacterItemData(InventoryType inventoryType, int indexOfData)
        {
            this.inventoryType = inventoryType;
            this.indexOfData = indexOfData;
        }
    }

    public struct UIBuffData
    {
        public Buff buff;
        public short targetLevel;
        public UIBuffData(Buff buff, short targetLevel)
        {
            this.buff = buff;
            this.targetLevel = targetLevel;
        }
    }

    public struct UIGuildSkillData
    {
        public GuildSkill guildSkill;
        public short targetLevel;
        public UIGuildSkillData(GuildSkill guildSkill, short targetLevel)
        {
            this.guildSkill = guildSkill;
            this.targetLevel = targetLevel;
        }
    }

    public struct UIArmorAmountData
    {
        public DamageElement damageElement;
        public float amount;
        public UIArmorAmountData(DamageElement damageElement, float amount)
        {
            this.damageElement = damageElement;
            this.amount = amount;
        }
    }

    public struct UIDamageElementAmountData
    {
        public DamageElement damageElement;
        public MinMaxFloat amount;
        public UIDamageElementAmountData(DamageElement damageElement, MinMaxFloat amount)
        {
            this.damageElement = damageElement;
            this.amount = amount;
        }
    }

    public struct UIDamageElementInflictionData
    {
        public DamageElement damageElement;
        public float infliction;
        public UIDamageElementInflictionData(DamageElement damageElement, float infliction)
        {
            this.damageElement = damageElement;
            this.infliction = infliction;
        }
    }

    public struct UIQuestTaskData
    {
        public QuestTask questTask;
        public int progress;
        public UIQuestTaskData(QuestTask questTask, int progress)
        {
            this.questTask = questTask;
            this.progress = progress;
        }
    }

    public struct UIEquipmentSetData
    {
        public EquipmentSet equipmentSet;
        public int equippedCount;
        public UIEquipmentSetData(EquipmentSet equipmentSet, int equippedCount)
        {
            this.equipmentSet = equipmentSet;
            this.equippedCount = equippedCount;
        }
    }

    public struct UIEquipmentSocketsData
    {
        public List<int> sockets;
        public int maxSocket;
        public UIEquipmentSocketsData(List<int> sockets, int maxSocket)
        {
            this.sockets = sockets;
            this.maxSocket = maxSocket;
        }
    }

    public struct UIRewardingData
    {
        public int rewardExp;
        public int rewardGold;
        public int rewardCash;
        [ArrayElementTitle("item")]
        public ItemAmount[] rewardItems;
    }
}
