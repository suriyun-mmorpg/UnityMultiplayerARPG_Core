namespace MultiplayerARPG
{
    public interface ICustomSummonManager
    {
        /// <summary>
        /// Return `TRUE` if it is addressable
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="addressablePrefab"></param>
        /// <returns></returns>
        bool GetPrefab(out BaseMonsterCharacterEntity prefab, out AssetReferenceBaseMonsterCharacterEntity addressablePrefab);
        void UnSummon(CharacterSummon characterSummon);
        void Update(CharacterSummon characterSummon, float deltaTime);
    }
}
