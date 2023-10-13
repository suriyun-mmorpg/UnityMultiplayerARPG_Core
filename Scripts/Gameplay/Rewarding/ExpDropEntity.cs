using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class ExpDropEntity : BaseRewardDropEntity, IPickupActivatableEntity
    {
        public static ExpDropEntity Drop(BaseGameEntity dropper, float multiplier, RewardGivenType givenType, int giverLevel, int sourceLevel, int amount, IEnumerable<string> looters)
        {
            return Drop(GameInstance.Singleton.expDropEntityPrefab, dropper, multiplier, givenType, giverLevel, sourceLevel, amount, looters, GameInstance.Singleton.itemAppearDuration) as ExpDropEntity;
        }

        protected override bool ProceedPickingUpAtServer_Implementation(BaseCharacterEntity characterEntity, out UITextKeys message)
        {
            // TODO: It is easy to request for a EXP drop on ground feature, but it is actually not easy to implements because it has to share EXP to party/guild
            BaseCharacterEntity rewardingCharacter = characterEntity;
            if (characterEntity is BaseMonsterCharacterEntity monsterCharacterEntity && monsterCharacterEntity.Summoner is BasePlayerCharacterEntity summonerCharacterEntity)
                rewardingCharacter = summonerCharacterEntity;
            if (!CurrentGameplayRule.RewardExp(rewardingCharacter, Amount, Multiplier, GivenType, GiverLevel, SourceLevel, out int rewardedExp))
            {
                GameInstance.ServerGameMessageHandlers.NotifyRewardExp(rewardingCharacter.ConnectionId, GivenType, rewardedExp);
                message = UITextKeys.NONE;
                return true;
            }
            GameInstance.ServerGameMessageHandlers.NotifyRewardExp(rewardingCharacter.ConnectionId, GivenType, rewardedExp);
            rewardingCharacter.CallAllOnLevelUp();
            message = UITextKeys.NONE;
            return true;
        }
    }
}
