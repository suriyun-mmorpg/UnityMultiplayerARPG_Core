using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct EnhancerRemoval
    {
        [SerializeField]
        private bool returnEnhancerItem;
        public bool ReturnEnhancerItem { get { return returnEnhancerItem; } }

        [SerializeField]
        private int requireGold;
        public int RequireGold { get { return requireGold; } }

        public EnhancerRemoval(bool returnEnhancerItem, int requireGold)
        {
            this.returnEnhancerItem = returnEnhancerItem;
            this.requireGold = requireGold;
        }

        public bool CanRemove(IPlayerCharacterData character)
        {
            return CanRemove(character, out _);
        }

        public bool CanRemove(IPlayerCharacterData character, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (!GameInstance.Singleton.GameplayRule.CurrenciesEnoughToRemoveEnhancer(character))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD;
                return false;
            }
            return true;
        }
    }
}
