using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.PLAYER_TITLE_FILE, menuName = GameDataMenuConsts.PLAYER_TITLE_MENU, order = GameDataMenuConsts.PLAYER_TITLE_ORDER)]
    public class PlayerTitle : BaseGameData
    {
        public Buff buff;
    }
}
