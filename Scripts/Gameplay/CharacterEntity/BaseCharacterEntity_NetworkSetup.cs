using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            id.deliveryMethod = DeliveryMethod.ReliableOrdered;
            id.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            level.deliveryMethod = DeliveryMethod.ReliableOrdered;
            level.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            exp.deliveryMethod = DeliveryMethod.ReliableOrdered;
            exp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            isImmune.deliveryMethod = DeliveryMethod.ReliableOrdered;
            isImmune.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentHp.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentHp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentMp.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentMp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentFood.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentFood.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentWater.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentWater.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            equipWeaponSet.deliveryMethod = DeliveryMethod.ReliableOrdered;
            equipWeaponSet.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            pitch.deliveryMethod = DeliveryMethod.Sequenced;
            pitch.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            targetEntityId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            targetEntityId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

            selectableWeaponSets.forOwnerOnly = false;
            attributes.forOwnerOnly = false;
            skills.forOwnerOnly = false;
            skillUsages.forOwnerOnly = true;
            buffs.forOwnerOnly = false;
            equipItems.forOwnerOnly = false;
            nonEquipItems.forOwnerOnly = true;
            summons.forOwnerOnly = true;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // On data changes events
            id.onChange += OnIdChange;
            syncTitle.onChange += OnCharacterNameChange;
            level.onChange += OnLevelChange;
            exp.onChange += OnExpChange;
            isImmune.onChange += OnIsImmuneChange;
            currentHp.onChange += OnCurrentHpChange;
            currentMp.onChange += OnCurrentMpChange;
            currentFood.onChange += OnCurrentFoodChange;
            currentWater.onChange += OnCurrentWaterChange;
            equipWeaponSet.onChange += OnEquipWeaponSetChange;
            pitch.onChange += OnPitchChange;
            targetEntityId.onChange += OnTargetEntityIdChange;
            // On list changes events
            selectableWeaponSets.onOperation += OnSelectableWeaponSetsOperation;
            attributes.onOperation += OnAttributesOperation;
            skills.onOperation += OnSkillsOperation;
            skillUsages.onOperation += OnSkillUsagesOperation;
            buffs.onOperation += OnBuffsOperation;
            equipItems.onOperation += OnEquipItemsOperation;
            nonEquipItems.onOperation += OnNonEquipItemsOperation;
            summons.onOperation += OnSummonsOperation;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            // On data changes events
            id.onChange -= OnIdChange;
            syncTitle.onChange -= OnCharacterNameChange;
            level.onChange -= OnLevelChange;
            exp.onChange -= OnExpChange;
            isImmune.onChange -= OnIsImmuneChange;
            currentHp.onChange -= OnCurrentHpChange;
            currentMp.onChange -= OnCurrentMpChange;
            currentFood.onChange -= OnCurrentFoodChange;
            currentWater.onChange -= OnCurrentWaterChange;
            equipWeaponSet.onChange -= OnEquipWeaponSetChange;
            pitch.onChange -= OnPitchChange;
            targetEntityId.onChange -= OnTargetEntityIdChange;
            // On list changes events
            selectableWeaponSets.onOperation -= OnSelectableWeaponSetsOperation;
            attributes.onOperation -= OnAttributesOperation;
            skills.onOperation -= OnSkillsOperation;
            skillUsages.onOperation -= OnSkillUsagesOperation;
            buffs.onOperation -= OnBuffsOperation;
            equipItems.onOperation -= OnEquipItemsOperation;
            nonEquipItems.onOperation -= OnNonEquipItemsOperation;
            summons.onOperation -= OnSummonsOperation;

            if (UICharacterEntity != null)
                Destroy(UICharacterEntity.gameObject);
        }
    }
}
