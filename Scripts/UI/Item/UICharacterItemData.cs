namespace MultiplayerARPG
{
    public struct UICharacterItemData
    {
        public CharacterItem characterItem;
        public short targetLevel;
        public InventoryType inventoryType;
        public UICharacterItemData(CharacterItem characterItem, short targetLevel, InventoryType inventoryType)
        {
            this.characterItem = characterItem;
            this.targetLevel = targetLevel;
            this.inventoryType = inventoryType;
        }
        public UICharacterItemData(CharacterItem characterItem, InventoryType inventoryType) : this(characterItem, characterItem.level, inventoryType)
        {
        }
        public UICharacterItemData(BaseItem item, short targetLevel, InventoryType inventoryType) : this(CharacterItem.Create(item, targetLevel), targetLevel, inventoryType)
        {
        }
    }
}
