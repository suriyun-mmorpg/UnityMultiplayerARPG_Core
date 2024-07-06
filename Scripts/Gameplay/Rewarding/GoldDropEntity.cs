using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class GoldDropEntity : BaseRewardDropEntity, IPickupActivatableEntity
    {
        public static GoldDropEntity Drop(BaseGameEntity dropper, float multiplier, RewardGivenType givenType, int giverLevel, int sourceLevel, int amount, IEnumerable<string> looters)
        {
            GoldDropEntity entity = null;
            GoldDropEntity tempPrefab = null;
#if !EXCLUDE_PREFAB_REFS
            tempPrefab = GameInstance.Singleton.goldDropEntityPrefab;
#endif
            AssetReferenceGoldDropEntity tempAddressablePrefab = GameInstance.Singleton.addressableGoldDropEntityPrefab;
            GoldDropEntity loadedPrefab = tempAddressablePrefab.GetOrLoadAssetOrUsePrefab(tempPrefab);
            if (loadedPrefab != null)
            {
                entity = Drop(loadedPrefab, dropper, multiplier, givenType, giverLevel, sourceLevel, amount, looters, GameInstance.Singleton.itemAppearDuration) as GoldDropEntity;
            }
            return entity;
        }

        protected override bool ProceedPickingUpAtServer_Implementation(BaseCharacterEntity characterEntity, out UITextKeys message)
        {
            BaseCharacterEntity rewardingCharacter = characterEntity;
            if (characterEntity is BaseMonsterCharacterEntity monsterCharacterEntity && monsterCharacterEntity.Summoner is BasePlayerCharacterEntity summonerCharacterEntity)
                rewardingCharacter = summonerCharacterEntity;
            CurrentGameplayRule.RewardGold(rewardingCharacter, Amount, Multiplier, GivenType, GiverLevel, SourceLevel, out int rewardedGold);
            rewardingCharacter.OnRewardGold(GivenType, rewardedGold);
            message = UITextKeys.NONE;
            return true;
        }
    }
}
