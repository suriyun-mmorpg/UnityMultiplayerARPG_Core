namespace MultiplayerARPG
{
    public partial class BaseItem
    {
        public bool TryGetItemRefineLevel(int level, out ItemRefineLevel refineLevel)
        {
            refineLevel = default;
            if (ItemRefine == null)
                return false;
            if (level - 1 >= ItemRefine.Levels.Length)
                return false;
            refineLevel = ItemRefine.Levels[level - 1];
            return true;
        }

        public bool CanRefine(IPlayerCharacterData character, int level, int[] enhancerDataIds)
        {
            return CanRefine(character, level, enhancerDataIds, out _);
        }

        public bool CanRefine(IPlayerCharacterData character, int level, int[] enhancerDataIds, out UITextKeys gameMessage)
        {
            if (!this.IsEquipment())
            {
                // Cannot refine because it's not equipment item
                gameMessage = UITextKeys.UI_ERROR_ITEM_NOT_EQUIPMENT;
                return false;
            }
            if (ItemRefine == null)
            {
                // Cannot refine because there is no item refine info
                gameMessage = UITextKeys.UI_ERROR_CANNOT_REFINE;
                return false;
            }
            if (level - 1 >= ItemRefine.Levels.Length)
            {
                // Cannot refine because item reached max level
                gameMessage = UITextKeys.UI_ERROR_REFINE_ITEM_REACHED_MAX_LEVEL;
                return false;
            }
            if (GameInstance.Singleton.refineEnhancerItemsLimit > 0 && enhancerDataIds.Length > GameInstance.Singleton.refineEnhancerItemsLimit)
            {
                // Cannot refine because enhancer items reached level
                gameMessage = UITextKeys.UI_ERROR_REACHED_REFINE_ENHANCER_ITEMS_LIMIT;
                return false;
            }
            return ItemRefine.Levels[level - 1].CanRefine(character, enhancerDataIds, out gameMessage);
        }
    }
}
