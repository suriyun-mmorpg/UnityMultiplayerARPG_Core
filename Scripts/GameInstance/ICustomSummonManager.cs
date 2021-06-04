namespace MultiplayerARPG
{
    public interface ICustomSummonManager
    {
        BaseMonsterCharacterEntity GetPrefab(int dataId);
        void UnSummon(CharacterSummon characterSummon);
    }
}
