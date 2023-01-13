namespace MultiplayerARPG
{
    public interface IItemUIVisibilityManager
    {
        bool IsShopDialogVisible();
        bool IsRefineItemDialogVisible();
        bool IsDismantleItemDialogVisible();
        bool IsRepairItemDialogVisible();
        bool IsEnhanceSocketItemDialogVisible();
        bool IsStorageDialogVisible();
        void ShowRefineItemDialog(InventoryType inventoryType, int indexOfData);
        void ShowDismantleItemDialog(InventoryType inventoryType, int indexOfData);
        void ShowRepairItemDialog(InventoryType inventoryType, int indexOfData);
        void ShowEnhanceSocketItemDialog(InventoryType inventoryType, int indexOfData);
        void ShowStorageDialog(StorageType storageType, string storageOwnerId, uint objectId, int weightLimit, int slotLimit);
        void HideStorageDialog();
    }
}
