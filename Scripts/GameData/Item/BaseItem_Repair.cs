namespace MultiplayerARPG
{
    public partial class BaseItem
    {
        public bool CanRepair(IPlayerCharacterData character, float durability, out float maxDurability, out ItemRepairPrice repairPrice)
        {
            return CanRepair(character, durability, out maxDurability, out repairPrice, out _);
        }

        public bool CanRepair(IPlayerCharacterData character, float durability, out float maxDurability, out ItemRepairPrice repairPrice, out UITextKeys gameMessageType)
        {
            maxDurability = 0f;
            repairPrice = default;
            if (!this.IsEquipment())
            {
                // Cannot repair because it's not equipment item
                gameMessageType = UITextKeys.UI_ERROR_CANNOT_REPAIR;
                return false;
            }
            if (itemRefine == null)
            {
                // Cannot repair because there is no item refine info
                gameMessageType = UITextKeys.UI_ERROR_CANNOT_REPAIR;
                return false;
            }
            repairPrice = GetRepairPrice(durability, out maxDurability);
            if (durability >= maxDurability)
            {
                gameMessageType = UITextKeys.UI_ERROR_CANNOT_REPAIR;
                return false;
            }
            return repairPrice.CanRepair(character, out gameMessageType);
        }

        public ItemRepairPrice GetRepairPrice(float durability)
        {
            return GetRepairPrice(durability, out _);
        }

        public ItemRepairPrice GetRepairPrice(float durability, out float maxDurability)
        {
            ItemRepairPrice repairPrice = default;
            maxDurability = (this as IEquipmentItem).MaxDurability;
            if (maxDurability <= 0f)
                return repairPrice;
            float durabilityRate = durability / maxDurability;
            if (durabilityRate >= 1f)
                return repairPrice;
            System.Array.Sort(itemRefine.RepairPrices);
            for (int i = itemRefine.RepairPrices.Length - 1; i >= 0; --i)
            {
                repairPrice = itemRefine.RepairPrices[i];
                if (durabilityRate < repairPrice.DurabilityRate)
                    return repairPrice;
            }
            return repairPrice;
        }
    }
}
