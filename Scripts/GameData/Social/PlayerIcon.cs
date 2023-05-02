using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.PLAYER_ICON_FILE, menuName = GameDataMenuConsts.PLAYER_ICON_MENU, order = GameDataMenuConsts.PLAYER_ICON_ORDER)]
    public partial class PlayerIcon : BaseGameData
    {
        [SerializeField]
        protected bool isLocked;
        public bool IsLocked
        {
            get { return isLocked; }
            set { isLocked = value; }
        }
    }
}
