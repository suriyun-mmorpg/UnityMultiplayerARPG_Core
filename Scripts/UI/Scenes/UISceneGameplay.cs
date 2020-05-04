using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class UISceneGameplay : BaseUISceneGameplay
    {
        [System.Serializable]
        public struct UIToggleUI
        {
            public UIBase ui;
            public KeyCode key;
        }

        [Header("Character Releates UIs")]
        public UICharacter[] uiCharacters;
        public UIEquipItems[] uiCharacterEquipItems;
        public UINonEquipItems[] uiCharacterNonEquipItems;
        public UICharacterSkills[] uiCharacterSkills;
        public UICharacterSummons[] uiCharacterSummons;
        public UICharacterHotkeys[] uiCharacterHotkeys;
        public UICharacterQuests[] uiCharacterQuests;
        public UIAmmoAmount uiAmmoAmount;

        [HideInInspector]
        public UIEquipItems uiEquipItems;
        [HideInInspector]
        public UINonEquipItems uiNonEquipItems;
        [HideInInspector]
        public UICharacterSkills uiSkills;
        [HideInInspector]
        public UICharacterSummons uiSummons;
        [HideInInspector]
        public UICharacterHotkeys uiHotkeys;
        [HideInInspector]
        public UICharacterQuests uiQuests;

        [Header("Selected Target UIs")]
        public UICharacter uiTargetCharacter;
        public UIBaseGameEntity uiTargetNpc;
        public UIBaseGameEntity uiTargetItemDrop;
        public UIDamageableEntity uiTargetBuilding;
        public UIDamageableEntity uiTargetHarvestable;
        public UIBaseGameEntity uiTargetVehicle;

        [Header("Other UIs")]
        public UINpcDialog uiNpcDialog;
        public UIRefineItem uiRefineItem;
        public UIDismantleItem uiDismantleItem;
        public UIEnhanceSocketItem uiEnhanceSocketItem;
        public UIConstructBuilding uiConstructBuilding;
        public UICurrentBuilding uiCurrentBuilding;
        public UICurrentBuilding uiCurrentDoor;
        public UICurrentBuilding uiCurrentStorage;
        public UICurrentBuilding uiCurrentWorkbench;
        public UIPlayerActivateMenu uiPlayerActivateMenu;
        public UIDealingRequest uiDealingRequest;
        public UIDealing uiDealing;
        public UIPartyInvitation uiPartyInvitation;
        public UIGuildInvitation uiGuildInvitation;
        public UIStorageItems uiPlayerStorageItems;
        public UIStorageItems uiGuildStorageItems;
        public UIStorageItems uiBuildingStorageItems;
        public UICampfireItems uiBuildingCampfireItems;
        public UICraftItems uiBuildingCraftItems;
        public UIBase uiIsWarping;

        [Header("Other Settings")]
        public UIToggleUI[] toggleUis;
        [Tooltip("These GameObject (s) will ignore click / touch detection when click or touch on screen")]
        public List<GameObject> ignorePointerDetectionUis;
        [Tooltip("These UI (s) will block character controller inputs while visible")]
        public List<UIBase> blockControllerUIs;

        [Header("Events")]
        public UnityEvent onCharacterDead;
        public UnityEvent onCharacterRespawn;

        public System.Action<BasePlayerCharacterEntity> onUpdateCharacter;
        public System.Action<BasePlayerCharacterEntity> onUpdateEquipItems;
        public System.Action<BasePlayerCharacterEntity> onUpdateEquipWeapons;
        public System.Action<BasePlayerCharacterEntity> onUpdateNonEquipItems;
        public System.Action<BasePlayerCharacterEntity> onUpdateSkills;
        public System.Action<BasePlayerCharacterEntity> onUpdateSummons;
        public System.Action<BasePlayerCharacterEntity> onUpdateHotkeys;
        public System.Action<BasePlayerCharacterEntity> onUpdateQuests;
        public System.Action<BasePlayerCharacterEntity> onUpdateStorageItems;

        protected override void Awake()
        {
            base.Awake();
            if (uiCurrentDoor == null)
                uiCurrentDoor = uiCurrentBuilding;
            if (uiCurrentStorage == null)
                uiCurrentStorage = uiCurrentBuilding;
            if (uiCurrentWorkbench == null)
                uiCurrentWorkbench = uiCurrentBuilding;
            MigrateNewUIs();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (MigrateNewUIs())
                EditorUtility.SetDirty(this);
#endif
        }

        private bool MigrateNewUIs()
        {
            bool hasChanges = false;
            if (uiEquipItems != null)
            {
                List<UIEquipItems> list = uiCharacterEquipItems == null ? new List<UIEquipItems>() : new List<UIEquipItems>(uiCharacterEquipItems);
                list.Add(uiEquipItems);
                uiCharacterEquipItems = list.ToArray();
                uiEquipItems = null;
                hasChanges = true;
            }

            if (uiNonEquipItems != null)
            {
                List<UINonEquipItems> list = uiCharacterNonEquipItems == null ? new List<UINonEquipItems>() : new List<UINonEquipItems>(uiCharacterNonEquipItems);
                list.Add(uiNonEquipItems);
                uiCharacterNonEquipItems = list.ToArray();
                uiNonEquipItems = null;
                hasChanges = true;
            }

            if (uiSkills != null)
            {
                List<UICharacterSkills> list = uiCharacterSkills == null ? new List<UICharacterSkills>() : new List<UICharacterSkills>(uiCharacterSkills);
                list.Add(uiSkills);
                uiCharacterSkills = list.ToArray();
                uiSkills = null;
                hasChanges = true;
            }

            if (uiSummons != null)
            {
                List<UICharacterSummons> list = uiCharacterSummons == null ? new List<UICharacterSummons>() : new List<UICharacterSummons>(uiCharacterSummons);
                list.Add(uiSummons);
                uiCharacterSummons = list.ToArray();
                uiSummons = null;
                hasChanges = true;
            }

            if (uiHotkeys != null)
            {
                List<UICharacterHotkeys> list = uiCharacterHotkeys == null ? new List<UICharacterHotkeys>() : new List<UICharacterHotkeys>(uiCharacterHotkeys);
                list.Add(uiHotkeys);
                uiCharacterHotkeys = list.ToArray();
                uiHotkeys = null;
                hasChanges = true;
            }

            if (uiQuests != null)
            {
                List<UICharacterQuests> list = uiCharacterQuests == null ? new List<UICharacterQuests>() : new List<UICharacterQuests>(uiCharacterQuests);
                list.Add(uiQuests);
                uiCharacterQuests = list.ToArray();
                uiQuests = null;
                hasChanges = true;
            }

            return hasChanges;
        }

        protected override void Update()
        {
            if (GenericUtils.IsFocusInputField())
                return;

            base.Update();

            foreach (UIToggleUI toggleUi in toggleUis)
            {
                if (Input.GetKeyDown(toggleUi.key))
                {
                    UIBase ui = toggleUi.ui;
                    ui.Toggle();
                }
            }
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character UIs when owning character data updated
        /// </summary>
        public void UpdateCharacter()
        {
            foreach (UICharacter ui in uiCharacters)
            {
                if (ui != null)
                    ui.Data = BasePlayerCharacterController.OwningCharacter;
            }
            if (onUpdateCharacter != null)
                onUpdateCharacter.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character equip items UIs when owning character equip items updated
        /// </summary>
        public void UpdateEquipItems()
        {
            foreach (UIEquipItems ui in uiCharacterEquipItems)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (uiRefineItem != null)
                uiRefineItem.OnUpdateCharacterItems();
            if (uiDismantleItem != null)
                uiDismantleItem.OnUpdateCharacterItems();
            if (uiEnhanceSocketItem != null)
                uiEnhanceSocketItem.OnUpdateCharacterItems();
            if (onUpdateEquipItems != null)
                onUpdateEquipItems.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character equip weapons UIs when owning character equip weapons updated
        /// </summary>
        public void UpdateEquipWeapons()
        {
            if (uiAmmoAmount != null)
                uiAmmoAmount.UpdateData(BasePlayerCharacterController.OwningCharacter);
            if (onUpdateEquipWeapons != null)
                onUpdateEquipWeapons.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character non equip items UIs when owning character non equip items updated
        /// </summary>
        public void UpdateNonEquipItems()
        {
            foreach (UINonEquipItems ui in uiCharacterNonEquipItems)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (uiRefineItem != null)
                uiRefineItem.OnUpdateCharacterItems();
            if (uiDismantleItem != null)
                uiDismantleItem.OnUpdateCharacterItems();
            if (uiEnhanceSocketItem != null)
                uiEnhanceSocketItem.OnUpdateCharacterItems();
            if (onUpdateNonEquipItems != null)
                onUpdateNonEquipItems.Invoke(BasePlayerCharacterController.OwningCharacter);
            if (uiAmmoAmount != null)
                uiAmmoAmount.UpdateData(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character skills UIs when owning character skills updated
        /// </summary>
        public void UpdateSkills()
        {
            foreach (UICharacterSkills ui in uiCharacterSkills)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (onUpdateSkills != null)
                onUpdateSkills.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character summons UIs when owning character summons updated
        /// </summary>
        public void UpdateSummons()
        {
            foreach (UICharacterSummons ui in uiCharacterSummons)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (onUpdateSummons != null)
                onUpdateSummons.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character hotkeys UIs when owning character hotkeys updated
        /// </summary>
        public void UpdateHotkeys()
        {
            foreach (UICharacterHotkeys ui in uiCharacterHotkeys)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (onUpdateHotkeys != null)
                onUpdateHotkeys.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character quests UIs when owning character quests updated
        /// </summary>
        public void UpdateQuests()
        {
            foreach (UICharacterQuests ui in uiCharacterQuests)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (onUpdateQuests != null)
                onUpdateQuests.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character storage items UIs when owning character storage items updated
        /// </summary>
        public void UpdateStorageItems()
        {
            if (uiPlayerStorageItems != null)
                uiPlayerStorageItems.UpdateData();
            if (uiGuildStorageItems != null)
                uiGuildStorageItems.UpdateData();
            if (uiBuildingStorageItems != null)
                uiBuildingStorageItems.UpdateData();
            if (uiBuildingCampfireItems != null)
                uiBuildingCampfireItems.UpdateData();

            if (onUpdateStorageItems != null)
                onUpdateStorageItems.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To set selected target entity UIs
        /// </summary>
        /// <param name="entity"></param>
        public override void SetTargetEntity(BaseGameEntity entity)
        {
            if (entity == null)
            {
                SetTargetCharacter(null);
                SetTargetNpc(null);
                SetTargetItemDrop(null);
                SetTargetBuilding(null);
                SetTargetHarvestable(null);
                SetTargetVehicle(null);
                return;
            }

            if (entity is BaseCharacterEntity)
                SetTargetCharacter(entity as BaseCharacterEntity);
            if (entity is NpcEntity)
                SetTargetNpc(entity as NpcEntity);
            if (entity is ItemDropEntity)
                SetTargetItemDrop(entity as ItemDropEntity);
            if (entity is BuildingEntity)
                SetTargetBuilding(entity as BuildingEntity);
            if (entity is HarvestableEntity)
                SetTargetHarvestable(entity as HarvestableEntity);
            if (entity is VehicleEntity)
                SetTargetVehicle(entity as VehicleEntity);
        }

        protected void SetTargetCharacter(BaseCharacterEntity character)
        {
            if (uiTargetCharacter == null)
                return;

            if (character == null)
            {
                uiTargetCharacter.Hide();
                return;
            }

            uiTargetCharacter.Data = character;
            uiTargetCharacter.Show();
        }

        protected void SetTargetNpc(NpcEntity npc)
        {
            if (uiTargetNpc == null)
                return;

            if (npc == null)
            {
                uiTargetNpc.Hide();
                return;
            }

            uiTargetNpc.Data = npc;
            uiTargetNpc.Show();
        }

        protected void SetTargetItemDrop(ItemDropEntity itemDrop)
        {
            if (uiTargetItemDrop == null)
                return;

            if (itemDrop == null)
            {
                uiTargetItemDrop.Hide();
                return;
            }

            uiTargetItemDrop.Data = itemDrop;
            uiTargetItemDrop.Show();
        }

        protected void SetTargetBuilding(BuildingEntity building)
        {
            if (uiTargetBuilding == null)
                return;

            if (building == null)
            {
                uiTargetBuilding.Hide();
                return;
            }

            uiTargetBuilding.Data = building;
            uiTargetBuilding.Show();
        }

        protected void SetTargetHarvestable(HarvestableEntity harvestable)
        {
            if (uiTargetHarvestable == null)
                return;

            if (harvestable == null)
            {
                uiTargetHarvestable.Hide();
                return;
            }

            uiTargetHarvestable.Data = harvestable;
            uiTargetHarvestable.Show();
        }

        protected void SetTargetVehicle(VehicleEntity vehicle)
        {
            if (uiTargetVehicle == null)
                return;

            if (vehicle == null)
            {
                uiTargetVehicle.Hide();
                return;
            }

            uiTargetVehicle.Data = vehicle;
            uiTargetVehicle.Show();
        }

        public override void SetActivePlayerCharacter(BasePlayerCharacterEntity playerCharacter)
        {
            if (uiPlayerActivateMenu == null)
                return;

            uiPlayerActivateMenu.Data = playerCharacter;
            uiPlayerActivateMenu.Show();
        }

        public void OnClickRespawn()
        {
            BasePlayerCharacterController.OwningCharacter.RequestRespawn();
        }

        public void OnClickExit()
        {
            BaseGameNetworkManager.Singleton.StopHost();
        }

        public void OnCharacterDead()
        {
            onCharacterDead.Invoke();
        }

        public void OnCharacterRespawn()
        {
            onCharacterRespawn.Invoke();
        }

        public override void ShowNpcDialog(int npcDialogDataId)
        {
            if (uiNpcDialog == null)
                return;
            NpcDialog npcDialog;
            if (!GameInstance.NpcDialogs.TryGetValue(npcDialogDataId, out npcDialog))
            {
                uiNpcDialog.Hide();
                return;
            }
            uiNpcDialog.Data = npcDialog;
            uiNpcDialog.Show();
        }

        public void OnShowNpcRefineItem()
        {
            if (uiRefineItem == null)
                return;
            // Don't select any item yet, wait player to select the item
            uiRefineItem.Data = new UICharacterItemByIndexData(InventoryType.NonEquipItems, -1);
            uiRefineItem.Show();
        }

        public void OnShowNpcDismantleItem()
        {
            if (uiDismantleItem == null)
                return;
            // Don't select any item yet, wait player to select the item
            uiDismantleItem.Data = new UICharacterItemByIndexData(InventoryType.NonEquipItems, -1);
            uiDismantleItem.Show();
        }

        public void OnShowDealingRequest(BasePlayerCharacterEntity playerCharacter)
        {
            if (uiDealingRequest == null)
                return;
            uiDealingRequest.Data = playerCharacter;
            uiDealingRequest.Show();
        }

        public void OnShowDealing(BasePlayerCharacterEntity playerCharacter)
        {
            if (uiDealing == null)
                return;
            uiDealing.Data = playerCharacter;
            uiDealing.Show();
        }

        public void OnUpdateDealingState(DealingState state)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateDealingState(state);
        }

        public void OnUpdateAnotherDealingState(DealingState state)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateAnotherDealingState(state);
        }

        public void OnUpdateDealingGold(int gold)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateDealingGold(gold);
        }

        public void OnUpdateAnotherDealingGold(int gold)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateAnotherDealingGold(gold);
        }

        public void OnUpdateDealingItems(DealingCharacterItems items)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateDealingItems(items);
        }

        public void OnUpdateAnotherDealingItems(DealingCharacterItems items)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateAnotherDealingItems(items);
        }

        public void OnShowPartyInvitation(BasePlayerCharacterEntity playerCharacter)
        {
            if (uiPartyInvitation == null)
                return;
            uiPartyInvitation.Data = playerCharacter;
            uiPartyInvitation.Show();
        }

        public void OnShowGuildInvitation(BasePlayerCharacterEntity playerCharacter)
        {
            if (uiGuildInvitation == null)
                return;
            uiGuildInvitation.Data = playerCharacter;
            uiGuildInvitation.Show();
        }

        public void OnShowStorage(StorageType storageType, uint objectId, short weightLimit, short slotLimit)
        {
            // Hide all of storage UIs
            if (uiPlayerStorageItems != null)
                uiPlayerStorageItems.Hide();
            if (uiGuildStorageItems != null)
                uiGuildStorageItems.Hide();
            if (uiBuildingStorageItems != null)
                uiBuildingStorageItems.Hide();
            if (uiBuildingCampfireItems != null)
                uiBuildingCampfireItems.Hide();
            // Show only selected storage type
            switch (storageType)
            {
                case StorageType.Player:
                    if (uiPlayerStorageItems != null)
                    {
                        uiPlayerStorageItems.Show(storageType, null, weightLimit, slotLimit);
                        uiPlayerStorageItems.UpdateData();
                    }
                    break;
                case StorageType.Guild:
                    if (uiGuildStorageItems != null)
                    {
                        uiGuildStorageItems.Show(storageType, null, weightLimit, slotLimit);
                        uiGuildStorageItems.UpdateData();
                    }
                    break;
                case StorageType.Building:
                    BuildingEntity buildingEntity;
                    if (!BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(objectId, out buildingEntity))
                        return;

                    if (buildingEntity is CampFireEntity)
                    {
                        if (uiBuildingCampfireItems != null)
                        {
                            uiBuildingCampfireItems.Show(storageType, buildingEntity, weightLimit, slotLimit);
                            uiBuildingCampfireItems.UpdateData();
                        }
                    }
                    else if (buildingEntity is StorageEntity)
                    {
                        if (uiBuildingStorageItems != null)
                        {
                            uiBuildingStorageItems.Show(storageType, buildingEntity, weightLimit, slotLimit);
                            uiBuildingStorageItems.UpdateData();
                        }
                    }
                    break;
            }
        }

        public void OnIsWarpingChange(bool isWarping)
        {
            if (uiIsWarping == null)
                return;
            if (isWarping)
                uiIsWarping.Show();
            else
                uiIsWarping.Hide();
        }

        public override bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            if (UIDragHandler.DraggingObjects.Count > 0)
                return true;
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            // If it's not mobile ui, assume it's over UI
            if (ignorePointerDetectionUis != null && ignorePointerDetectionUis.Count > 0)
            {
                foreach (RaycastResult result in results)
                {
                    if (!ignorePointerDetectionUis.Contains(result.gameObject))
                        return true;
                }
            }
            else
            {
                return results.Count > 0;
            }
            return false;
        }

        public override bool IsBlockController()
        {
            if (base.IsBlockController())
                return true;

            if (blockControllerUIs != null && blockControllerUIs.Count > 0)
            {
                foreach (UIBase ui in blockControllerUIs)
                {
                    if (!ui)
                        continue;
                    if (ui.IsVisible())
                        return true;
                }
            }
            return false;
        }

        public override void ShowConstructBuildingDialog(BuildingEntity buildingEntity)
        {
            HideConstructBuildingDialog();
            if (buildingEntity == null)
                return;
            if (!uiConstructBuilding.IsVisible())
                uiConstructBuilding.Show();
        }

        public override void HideConstructBuildingDialog()
        {
            if (uiConstructBuilding.IsVisible())
                uiConstructBuilding.Hide();
        }

        public override void ShowCurrentBuildingDialog(BuildingEntity buildingEntity)
        {
            HideCurrentBuildingDialog();
            if (buildingEntity == null)
                return;
            if (buildingEntity is DoorEntity)
            {
                if (!uiCurrentDoor.IsVisible())
                    uiCurrentDoor.Show();
            }
            else if (buildingEntity is StorageEntity)
            {
                if (!uiCurrentStorage.IsVisible())
                    uiCurrentStorage.Show();
            }
            else if (buildingEntity is WorkbenchEntity)
            {
                if (!uiCurrentWorkbench.IsVisible())
                    uiCurrentWorkbench.Show();
            }
            else
            {
                if (!uiCurrentBuilding.IsVisible())
                    uiCurrentBuilding.Show();
            }
        }

        public override void HideCurrentBuildingDialog()
        {
            if (uiCurrentDoor.IsVisible())
                uiCurrentDoor.Hide();
            if (uiCurrentStorage.IsVisible())
                uiCurrentStorage.Hide();
            if (uiCurrentWorkbench.IsVisible())
                uiCurrentWorkbench.Hide();
            if (uiCurrentBuilding.IsVisible())
                uiCurrentBuilding.Hide();
        }

        public override void HideNpcDialog()
        {
            if (uiNpcDialog != null &&
                uiNpcDialog.IsVisible())
                uiNpcDialog.Hide();
            if (uiPlayerStorageItems != null &&
                uiPlayerStorageItems.IsVisible())
                uiPlayerStorageItems.Hide();
            if (uiGuildStorageItems != null &&
                uiGuildStorageItems.IsVisible())
                uiGuildStorageItems.Hide();
            if (uiBuildingStorageItems != null &&
                uiBuildingStorageItems.IsVisible())
                uiBuildingStorageItems.Hide();
            if (uiBuildingCraftItems != null &&
                uiBuildingCraftItems.IsVisible())
                uiBuildingCraftItems.Hide();
            if (uiBuildingCampfireItems != null &&
                uiBuildingCampfireItems.IsVisible())
                uiBuildingCampfireItems.Hide();
            if (!GameInstance.Singleton.canRefineItemByPlayer &&
                uiRefineItem != null &&
                uiRefineItem.IsVisible())
                uiRefineItem.Hide();
            if (!GameInstance.Singleton.canDismantleItemByPlayer &&
                uiDismantleItem != null &&
                uiDismantleItem.IsVisible())
                uiDismantleItem.Hide();
        }

        public override bool IsShopDialogVisible()
        {
            return uiNpcDialog != null &&
                uiNpcDialog.IsVisible() &&
                uiNpcDialog.Data != null &&
                uiNpcDialog.Data.IsShop;
        }

        public override bool IsRefineItemDialogVisible()
        {
            return uiRefineItem != null &&
                uiRefineItem.IsVisible();
        }

        public override bool IsDismantleItemDialogVisible()
        {
            return uiDismantleItem != null &&
                uiDismantleItem.IsVisible();
        }

        public override bool IsEnhanceSocketItemDialogVisible()
        {
            return uiEnhanceSocketItem != null &&
                uiEnhanceSocketItem.IsVisible();
        }

        public override bool IsStorageDialogVisible()
        {
            return (uiPlayerStorageItems != null && uiPlayerStorageItems.IsVisible()) ||
                (uiGuildStorageItems != null && uiGuildStorageItems.IsVisible()) ||
                (uiBuildingStorageItems != null && uiBuildingStorageItems.IsVisible()) ||
                (uiBuildingCampfireItems != null && uiBuildingCampfireItems.IsVisible());
        }

        public override bool IsDealingDialogVisibleWithDealingState()
        {
            return uiDealing != null && uiDealing.IsVisible() &&
                uiDealing.dealingState == DealingState.Dealing;
        }

        public override void ShowRefineItemDialog(InventoryType inventoryType, int indexOfData)
        {
            if (uiRefineItem == null)
                return;
            uiRefineItem.Data = new UICharacterItemByIndexData(inventoryType, indexOfData);
            uiRefineItem.Show();
        }

        public override void ShowDismantleItemDialog(InventoryType inventoryType, int indexOfData)
        {
            if (uiDismantleItem == null)
                return;
            uiDismantleItem.Data = new UICharacterItemByIndexData(inventoryType, indexOfData);
            uiDismantleItem.Show();
        }

        public override void ShowEnhanceSocketItemDialog(InventoryType inventoryType, int indexOfData)
        {
            if (uiEnhanceSocketItem == null)
                return;
            uiEnhanceSocketItem.Data = new UICharacterItemByIndexData(inventoryType, indexOfData);
            uiEnhanceSocketItem.Show();
        }

        public override void ShowWorkbenchDialog(WorkbenchEntity workbenchEntity)
        {
            if (uiBuildingCraftItems != null)
                uiBuildingCraftItems.Show(CrafterType.Workbench, workbenchEntity);
        }

        public override void OnControllerSetup(BasePlayerCharacterEntity characterEntity)
        {
            characterEntity.onShowNpcDialog += ShowNpcDialog;
            characterEntity.onShowNpcRefineItem += OnShowNpcRefineItem;
            characterEntity.onShowNpcDismantleItem += OnShowNpcDismantleItem;
            characterEntity.onDead += OnCharacterDead;
            characterEntity.onRespawn += OnCharacterRespawn;
            characterEntity.onShowDealingRequestDialog += OnShowDealingRequest;
            characterEntity.onShowDealingDialog += OnShowDealing;
            characterEntity.onUpdateDealingState += OnUpdateDealingState;
            characterEntity.onUpdateDealingGold += OnUpdateDealingGold;
            characterEntity.onUpdateDealingItems += OnUpdateDealingItems;
            characterEntity.onUpdateAnotherDealingState += OnUpdateAnotherDealingState;
            characterEntity.onUpdateAnotherDealingGold += OnUpdateAnotherDealingGold;
            characterEntity.onUpdateAnotherDealingItems += OnUpdateAnotherDealingItems;
            characterEntity.onShowPartyInvitationDialog += OnShowPartyInvitation;
            characterEntity.onShowGuildInvitationDialog += OnShowGuildInvitation;
            characterEntity.onShowStorage += OnShowStorage;
            characterEntity.onIsWarpingChange += OnIsWarpingChange;
            characterEntity.onIdChange += OnIdChange;
            characterEntity.onEquipWeaponSetChange += OnEquipWeaponSetChange;
            characterEntity.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
            characterEntity.onAttributesOperation += OnAttributesOperation;
            characterEntity.onSkillsOperation += OnSkillsOperation;
            characterEntity.onSummonsOperation += OnSummonsOperation;
            characterEntity.onBuffsOperation += OnBuffsOperation;
            characterEntity.onEquipItemsOperation += OnEquipItemsOperation;
            characterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
            characterEntity.onHotkeysOperation += OnHotkeysOperation;
            characterEntity.onQuestsOperation += OnQuestsOperation;
            characterEntity.onStorageItemsOperation += OnStorageItemsOperation;

            UpdateCharacter();
            UpdateSkills();
            UpdateSummons();
            UpdateEquipItems();
            UpdateEquipWeapons();
            UpdateNonEquipItems();
            UpdateHotkeys();
            UpdateQuests();
            UpdateStorageItems();
        }

        public override void OnControllerDesetup(BasePlayerCharacterEntity characterEntity)
        {
            characterEntity.onShowNpcDialog -= ShowNpcDialog;
            characterEntity.onShowNpcRefineItem -= OnShowNpcRefineItem;
            characterEntity.onShowNpcDismantleItem -= OnShowNpcDismantleItem;
            characterEntity.onDead -= OnCharacterDead;
            characterEntity.onRespawn -= OnCharacterRespawn;
            characterEntity.onShowDealingRequestDialog -= OnShowDealingRequest;
            characterEntity.onShowDealingDialog -= OnShowDealing;
            characterEntity.onUpdateDealingState -= OnUpdateDealingState;
            characterEntity.onUpdateDealingGold -= OnUpdateDealingGold;
            characterEntity.onUpdateDealingItems -= OnUpdateDealingItems;
            characterEntity.onUpdateAnotherDealingState -= OnUpdateAnotherDealingState;
            characterEntity.onUpdateAnotherDealingGold -= OnUpdateAnotherDealingGold;
            characterEntity.onUpdateAnotherDealingItems -= OnUpdateAnotherDealingItems;
            characterEntity.onShowPartyInvitationDialog -= OnShowPartyInvitation;
            characterEntity.onShowGuildInvitationDialog -= OnShowGuildInvitation;
            characterEntity.onShowStorage -= OnShowStorage;
            characterEntity.onIsWarpingChange -= OnIsWarpingChange;
            characterEntity.onIdChange -= OnIdChange;
            characterEntity.onEquipWeaponSetChange -= OnEquipWeaponSetChange;
            characterEntity.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
            characterEntity.onAttributesOperation -= OnAttributesOperation;
            characterEntity.onSkillsOperation -= OnSkillsOperation;
            characterEntity.onSummonsOperation -= OnSummonsOperation;
            characterEntity.onBuffsOperation -= OnBuffsOperation;
            characterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
            characterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
            characterEntity.onHotkeysOperation -= OnHotkeysOperation;
            characterEntity.onQuestsOperation -= OnQuestsOperation;
            characterEntity.onStorageItemsOperation -= OnStorageItemsOperation;
        }

        #region Sync data changes callback
        protected void OnIdChange(string id)
        {
            UpdateCharacter();
            UpdateSkills();
            UpdateEquipItems();
            UpdateNonEquipItems();
        }

        protected void OnEquipWeaponSetChange(byte equipWeaponSet)
        {
            UpdateCharacter();
            UpdateEquipItems();
            UpdateEquipWeapons();
            UpdateSkills();
            UpdateHotkeys();
        }

        protected void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateCharacter();
            UpdateEquipItems();
            UpdateEquipWeapons();
            UpdateSkills();
            UpdateHotkeys();
        }

        protected void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateCharacter();
        }

        protected void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateCharacter();
            UpdateSkills();
        }

        protected void OnSummonsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateSummons();
        }

        protected void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateCharacter();
        }

        protected void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateCharacter();
            UpdateEquipItems();
            UpdateSkills();
            UpdateHotkeys();
        }

        protected void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateCharacter();
            UpdateNonEquipItems();
            UpdateHotkeys();
            UpdateQuests();
        }

        protected void OnHotkeysOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateHotkeys();
        }

        protected void OnQuestsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateQuests();
        }

        protected void OnStorageItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateStorageItems();
        }
        #endregion
    }
}
