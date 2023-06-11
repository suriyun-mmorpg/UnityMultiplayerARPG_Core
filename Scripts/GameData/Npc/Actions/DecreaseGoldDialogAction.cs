using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public class DecreaseGoldDialogAction : BaseNpcDialogAction
    {
        public int gold;

        public override UniTask DoAction(IPlayerCharacterData player)
        {
            player.Gold -= gold;
            return UniTask.CompletedTask;
        }

        public override UniTask<bool> IsPass(IPlayerCharacterData player)
        {
            if (player.Gold < gold)
                return new UniTask<bool>(false);
            return new UniTask<bool>(true);
        }
    }
}