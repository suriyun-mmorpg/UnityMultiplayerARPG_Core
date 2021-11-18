namespace MultiplayerARPG
{
    public partial class UIQuestRewardItemSelection : UICharacterItems
    {
        public void UpdateData(int questDataId)
        {
            if (!GameInstance.Quests.ContainsKey(questDataId))
                return;
            UpdateData(GameInstance.Quests[questDataId]);
        }

        public void UpdateData(Quest quest)
        {
            if (quest == null)
                return;
            inventoryType = InventoryType.Unknow;
            UpdateData(GameInstance.PlayingCharacterEntity, quest.selectableRewardItems);
        }

        public void OnClickSelectRewardItem()
        {
            GameInstance.PlayingCharacterEntity.CallServerSelectQuestRewardItem((byte)CacheSelectionManager.SelectedUI.IndexOfData);
            Hide();
        }
    }
}
