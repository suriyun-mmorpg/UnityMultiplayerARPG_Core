namespace MultiplayerARPG
{
    public struct CharacterItemTuple
    {
        public CharacterItem characterItem;
        public short targetLevel;
        public InventoryType inventoryType;
        public CharacterItemTuple(CharacterItem characterItem, short targetLevel, InventoryType inventoryType)
        {
            this.characterItem = characterItem;
            this.targetLevel = targetLevel;
            this.inventoryType = inventoryType;
        }
    }

    public struct AttributeTuple
    {
        public Attribute attribute;
        public short targetAmount;
        public AttributeTuple(Attribute attribute, short targetAmount)
        {
            this.attribute = attribute;
            this.targetAmount = targetAmount;
        }
    }

    public struct BuffTuple
    {
        public Buff buff;
        public short targetLevel;
        public BuffTuple(Buff buff, short targetLevel)
        {
            this.buff = buff;
            this.targetLevel = targetLevel;
        }
    }

    public struct SkillTuple
    {
        public Skill skill;
        public short targetLevel;
        public SkillTuple(Skill skill, short targetLevel)
        {
            this.skill = skill;
            this.targetLevel = targetLevel;
        }
    }

    public struct GuildSkillTuple
    {
        public GuildSkill guildSkill;
        public short targetLevel;
        public GuildSkillTuple(GuildSkill guildSkill, short targetLevel)
        {
            this.guildSkill = guildSkill;
            this.targetLevel = targetLevel;
        }
    }

    public struct DamageElementAmountTuple
    {
        public DamageElement damageElement;
        public MinMaxFloat amount;
        public DamageElementAmountTuple(DamageElement damageElement, MinMaxFloat amount)
        {
            this.damageElement = damageElement;
            this.amount = amount;
        }
    }

    public struct DamageElementInflictionTuple
    {
        public DamageElement damageElement;
        public float infliction;
        public DamageElementInflictionTuple(DamageElement damageElement, float infliction)
        {
            this.damageElement = damageElement;
            this.infliction = infliction;
        }
    }

    public struct QuestTaskProgressTuple
    {
        public QuestTask questTask;
        public int progress;
        public QuestTaskProgressTuple(QuestTask questTask, int progress)
        {
            this.questTask = questTask;
            this.progress = progress;
        }
    }

    public struct SocialCharacterEntityTuple
    {
        public SocialCharacterData socialCharacter;
        public BasePlayerCharacterEntity characterEntity;
    }
}
