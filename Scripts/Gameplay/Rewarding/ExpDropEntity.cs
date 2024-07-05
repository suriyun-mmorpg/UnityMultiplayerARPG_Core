using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class ExpDropEntity : BaseRewardDropEntity, IPickupActivatableEntity
    {
        public static ExpDropEntity Drop(BaseGameEntity dropper, float multiplier, RewardGivenType givenType, int giverLevel, int sourceLevel, int amount, IEnumerable<string> looters)
        {
            ExpDropEntity entity = null;
            ExpDropEntity prefab;
#if !EXCLUDE_PREFAB_REFS
            prefab = GameInstance.Singleton.expDropEntityPrefab;
#else
            prefab = null;
#endif
            if (prefab != null)
            {
                entity = Drop(prefab, dropper, multiplier, givenType, giverLevel, sourceLevel, amount, looters, GameInstance.Singleton.itemAppearDuration) as ExpDropEntity;
            }
            else if (GameInstance.Singleton.addressableExpDropEntityPrefab.IsDataValid())
            {
                entity = Drop(GameInstance.Singleton.addressableExpDropEntityPrefab.GetOrLoadAsset<ExpDropEntity>(), dropper, multiplier, givenType, giverLevel, sourceLevel, amount, looters, GameInstance.Singleton.itemAppearDuration) as ExpDropEntity;
            }
            return entity;
        }

        protected override bool ProceedPickingUpAtServer_Implementation(BaseCharacterEntity characterEntity, out UITextKeys message)
        {
            // TODO: It is easy to request for a EXP drop on ground feature, but it is actually not easy to implements because it has to share EXP to party/guild
            BaseCharacterEntity rewardingCharacter = characterEntity;
            if (characterEntity is BaseMonsterCharacterEntity monsterCharacterEntity && monsterCharacterEntity.Summoner is BasePlayerCharacterEntity summonerCharacterEntity)
                rewardingCharacter = summonerCharacterEntity;
            if (!CurrentGameplayRule.RewardExp(rewardingCharacter, Amount, Multiplier, GivenType, GiverLevel, SourceLevel, out int rewardedExp))
            {
                rewardingCharacter.OnRewardExp(GivenType, rewardedExp, false);
                message = UITextKeys.NONE;
                return true;
            }
            rewardingCharacter.OnRewardExp(GivenType, rewardedExp, true);
            rewardingCharacter.CallRpcOnLevelUp();
            message = UITextKeys.NONE;
            return true;
        }
    }
}
