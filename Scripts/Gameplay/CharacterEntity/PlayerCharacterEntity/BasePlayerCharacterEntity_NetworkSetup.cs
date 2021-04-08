using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            // Sync fields
            dataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            dataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            factionId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            factionId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            statPoint.deliveryMethod = DeliveryMethod.ReliableOrdered;
            statPoint.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            skillPoint.deliveryMethod = DeliveryMethod.ReliableOrdered;
            skillPoint.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            gold.deliveryMethod = DeliveryMethod.ReliableOrdered;
            gold.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            userGold.deliveryMethod = DeliveryMethod.ReliableOrdered;
            userGold.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            userCash.deliveryMethod = DeliveryMethod.ReliableOrdered;
            userCash.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            partyId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            partyId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            guildId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            guildId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            isWarping.deliveryMethod = DeliveryMethod.ReliableOrdered;
            isWarping.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            respawnMapName.deliveryMethod = DeliveryMethod.ReliableOrdered;
            respawnMapName.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            respawnPosition.deliveryMethod = DeliveryMethod.ReliableOrdered;
            respawnPosition.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            pitch.deliveryMethod = DeliveryMethod.Sequenced;
            pitch.syncMode = LiteNetLibSyncField.SyncMode.ClientMulticast;
            targetEntityId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            targetEntityId.syncMode = LiteNetLibSyncField.SyncMode.ClientMulticast;
            // Sync lists
            hotkeys.forOwnerOnly = true;
            quests.forOwnerOnly = true;
            currencies.forOwnerOnly = true;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // On data changes events
            dataId.onChange += OnDataIdChange;
            factionId.onChange += OnFactionIdChange;
            statPoint.onChange += OnStatPointChange;
            skillPoint.onChange += OnSkillPointChange;
            gold.onChange += OnGoldChange;
            userGold.onChange += OnUserGoldChange;
            userCash.onChange += OnUserCashChange;
            partyId.onChange += OnPartyIdChange;
            guildId.onChange += OnGuildIdChange;
            isWarping.onChange += OnIsWarpingChange;
            // On list changes events
            hotkeys.onOperation += OnHotkeysOperation;
            quests.onOperation += OnQuestsOperation;
            currencies.onOperation += OnCurrenciesOperation;
        }

        protected override void EntityOnSetOwnerClient()
        {
            base.EntityOnSetOwnerClient();

            // Setup relates elements
            if (IsOwnerClient)
            {
                if (BasePlayerCharacterController.Singleton == null)
                {
                    if (ControllerPrefab != null)
                    {
                        BasePlayerCharacterController controller = Instantiate(ControllerPrefab);
                        controller.PlayerCharacterEntity = this;
                    }
                    else if (CurrentGameInstance.defaultControllerPrefab != null)
                    {
                        BasePlayerCharacterController controller = Instantiate(CurrentGameInstance.defaultControllerPrefab);
                        controller.PlayerCharacterEntity = this;
                    }
                    else
                        Logging.LogWarning(ToString(), "`Controller Prefab` is empty so it cannot be instantiated");
                }
                if (CurrentGameInstance.owningCharacterObjects != null && CurrentGameInstance.owningCharacterObjects.Length > 0)
                {
                    foreach (GameObject obj in CurrentGameInstance.owningCharacterObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, CacheTransform.position, CacheTransform.rotation, CacheTransform);
                    }
                }
                if (CurrentGameInstance.owningCharacterMiniMapObjects != null && CurrentGameInstance.owningCharacterMiniMapObjects.Length > 0)
                {
                    foreach (GameObject obj in CurrentGameInstance.owningCharacterMiniMapObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, MiniMapUiTransform.position, MiniMapUiTransform.rotation, MiniMapUiTransform);
                    }
                }
                if (CurrentGameInstance.owningCharacterUI != null)
                    InstantiateUI(CurrentGameInstance.owningCharacterUI);
            }
            else
            {
                if (CurrentGameInstance.nonOwningCharacterMiniMapObjects != null && CurrentGameInstance.nonOwningCharacterMiniMapObjects.Length > 0)
                {
                    foreach (GameObject obj in CurrentGameInstance.nonOwningCharacterMiniMapObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, MiniMapUiTransform.position, MiniMapUiTransform.rotation, MiniMapUiTransform);
                    }
                }
                if (CurrentGameInstance.nonOwningCharacterUI != null)
                    InstantiateUI(CurrentGameInstance.nonOwningCharacterUI);
            }
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            // On data changes events
            dataId.onChange -= OnDataIdChange;
            factionId.onChange -= OnFactionIdChange;
            statPoint.onChange -= OnStatPointChange;
            skillPoint.onChange -= OnSkillPointChange;
            gold.onChange -= OnGoldChange;
            userGold.onChange -= OnUserGoldChange;
            userCash.onChange -= OnUserCashChange;
            partyId.onChange -= OnPartyIdChange;
            guildId.onChange -= OnGuildIdChange;
            isWarping.onChange -= OnIsWarpingChange;
            // On list changes events
            hotkeys.onOperation -= OnHotkeysOperation;
            quests.onOperation -= OnQuestsOperation;
            currencies.onOperation -= OnCurrenciesOperation;

            if (IsOwnerClient && BasePlayerCharacterController.Singleton != null)
                Destroy(BasePlayerCharacterController.Singleton.gameObject);
        }
    }
}
