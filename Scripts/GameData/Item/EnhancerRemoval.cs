using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct EnhancerRemoval
    {
        [SerializeField]
        private bool returnEnhancerItem;
        [SerializeField]
        private int requireGold;

        public bool ReturnEnhancerItem { get { return returnEnhancerItem; } }
        public int RequireGold { get { return requireGold; } }

        public bool CanRemove(IPlayerCharacterData character)
        {
            return CanRemove(character, out _);
        }

        public bool CanRemove(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.None;
            if (!GameInstance.Singleton.GameplayRule.CurrenciesEnoughToRemoveEnhancer(character))
            {
                gameMessageType = GameMessage.Type.NotEnoughGold;
                return false;
            }
            return true;
        }
    }
}
