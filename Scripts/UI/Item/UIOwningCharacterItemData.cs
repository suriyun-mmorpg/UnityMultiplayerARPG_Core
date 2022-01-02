namespace MultiplayerARPG
{
    public struct UIOwningCharacterItemData
    {
        public InventoryType inventoryType;
        public int indexOfData;
        public UIOwningCharacterItemData(InventoryType inventoryType, int indexOfData)
        {
            this.inventoryType = inventoryType;
            this.indexOfData = indexOfData;
        }
    }
}
