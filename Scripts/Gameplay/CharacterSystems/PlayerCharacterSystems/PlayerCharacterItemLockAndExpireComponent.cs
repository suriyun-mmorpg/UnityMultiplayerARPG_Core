using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class PlayerCharacterItemLockAndExpireComponent : BaseGameEntityComponent<BasePlayerCharacterEntity>
    {
        public const float ITEM_UPDATE_DURATION = 1f;

        private float updatingTime;
        private float deltaTime;

        public override sealed void EntityUpdate()
        {
            if (!Entity.IsServer)
                return;

            deltaTime = Time.unscaledDeltaTime;
            updatingTime += deltaTime;

            if (Entity.IsRecaching || Entity.IsDead())
                return;

            if (updatingTime >= ITEM_UPDATE_DURATION)
            {
                // Removing non-equip items if it should
                long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                bool haveRemovedItems = false;
                CharacterItem nonEquipItem;
                for (int i = Entity.NonEquipItems.Count - 1; i >= 0; --i)
                {
                    nonEquipItem = Entity.NonEquipItems[i];
                    if (nonEquipItem.ShouldRemove(currentTime))
                    {
                        if (CurrentGameInstance.IsLimitInventorySlot)
                            Entity.NonEquipItems[i] = CharacterItem.Empty;
                        else
                            Entity.NonEquipItems.RemoveAt(i);
                        haveRemovedItems = true;
                    }
                    else
                    {
                        if (nonEquipItem.IsLock())
                        {
                            nonEquipItem.Update(updatingTime);
                            Entity.NonEquipItems[i] = nonEquipItem;
                        }
                    }
                }
                if (haveRemovedItems)
                    Entity.FillEmptySlots();
                updatingTime = 0;
            }
        }
    }
}