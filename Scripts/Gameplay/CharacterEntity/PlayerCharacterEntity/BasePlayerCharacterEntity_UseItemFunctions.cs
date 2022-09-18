using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        /// <summary>
        /// This function will be called at server to order character to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        [ServerRpc]
        protected void ServerUseItem(short itemIndex)
        {
#if UNITY_SERVER || !MMO_BUILD
            if (!CanUseItem())
                return;

            if (itemIndex >= nonEquipItems.Count)
                return;

            CharacterItem tempCharacterItem = nonEquipItems[itemIndex];
            if (tempCharacterItem.IsLock())
                return;

            IUsableItem usableItem = tempCharacterItem.GetUsableItem();
            if (usableItem == null)
                return;

            usableItem.UseItem(this, itemIndex, tempCharacterItem);

            if (usableItem.UseItemCooldown > 0f)
            {
                for (int i = 0; i < nonEquipItems.Count; ++i)
                {
                    if (nonEquipItems[i].dataId == usableItem.DataId)
                    {
                        tempCharacterItem = nonEquipItems[i];
                        tempCharacterItem.Lock(usableItem.UseItemCooldown);
                        nonEquipItems[i] = tempCharacterItem;
                    }
                }
            }
#endif
        }
    }
}
