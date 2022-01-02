namespace MultiplayerARPG
{
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
}
