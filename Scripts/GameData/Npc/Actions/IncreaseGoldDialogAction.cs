using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public class IncreaseGoldDialogAction : BaseNpcDialogAction
    {
        public int gold;

        public override UniTask DoAction(IPlayerCharacterData player)
        {
            player.Gold.Increase(gold);
            return UniTask.CompletedTask;
        }

        public override UniTask<bool> IsPass(IPlayerCharacterData player)
        {
            return new UniTask<bool>(true);
        }
    }
}