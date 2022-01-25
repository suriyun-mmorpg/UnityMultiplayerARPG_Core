namespace MultiplayerARPG
{
    public interface ICustomSummonManager
    {
        BaseMonsterCharacterEntity GetPrefab(int dataId);
        void UnSummon(CharacterSummon characterSummon);
        void Update(CharacterSummon characterSummon, float deltaTime);
    }
}
