public struct BuffLevelTuple
{
    public Buff buff;
    public short targetLevel;
    public BuffLevelTuple(Buff buff, short targetLevel)
    {
        this.buff = buff;
        this.targetLevel = targetLevel;
    }
}

public struct CharacterAttributeAmountTuple
{
    public CharacterAttribute characterAttribute;
    public int targetAmount;
    public CharacterAttributeAmountTuple(CharacterAttribute characterAttribute, int targetAmount)
    {
        this.characterAttribute = characterAttribute;
        this.targetAmount = targetAmount;
    }
}

public struct CharacterItemLevelTuple
{
    public CharacterItem characterItem;
    public short targetLevel;
    public CharacterItemLevelTuple(CharacterItem characterItem, short targetLevel)
    {
        this.characterItem = characterItem;
        this.targetLevel = targetLevel;
    }
}

public struct CharacterSkillLevelTuple
{
    public CharacterSkill characterSkill;
    public short targetLevel;
    public CharacterSkillLevelTuple(CharacterSkill characterSkill, short targetLevel)
    {
        this.characterSkill = characterSkill;
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

public struct SkillLevelTuple
{
    public Skill skill;
    public short targetLevel;
    public SkillLevelTuple(Skill skill, short targetLevel)
    {
        this.skill = skill;
        this.targetLevel = targetLevel;
    }
}
