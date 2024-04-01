using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class GoldDropEntity : BaseRewardDropEntity, IPickupActivatableEntity
    {
        public static GoldDropEntity Drop(BaseGameEntity dropper, float multiplier, RewardGivenType givenType, int giverLevel, int sourceLevel, int amount, IEnumerable<string> looters)
        {
            if (GameInstance.Singleton.addressableGoldDropEntityPrefab.IsDataValid())
            {
                return Drop(GameInstance.Singleton.addressableGoldDropEntityPrefab.GetOrLoadAsset<AssetReferenceGoldDropEntity, GoldDropEntity>(), dropper, multiplier, givenType, giverLevel, sourceLevel, amount, looters, GameInstance.Singleton.itemAppearDuration) as GoldDropEntity;
            }
#if !LNLM_NO_PREFABS
            else if (GameInstance.Singleton.goldDropEntityPrefab != null)
            {
                return Drop(GameInstance.Singleton.goldDropEntityPrefab, dropper, multiplier, givenType, giverLevel, sourceLevel, amount, looters, GameInstance.Singleton.itemAppearDuration) as GoldDropEntity;
            }
#endif
            return null;
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
