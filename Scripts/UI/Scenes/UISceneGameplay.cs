using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UISceneGameplay : BaseUISceneGameplay
    {
        [System.Serializable]
        public struct UIToggleUI
        {
            public UIBase ui;
            [Tooltip("It will toggle `ui` when key with `keyCode` pressed or button with `buttonName` pressed.")]
            [FormerlySerializedAs("key")]
            public KeyCode keyCode;
            [Tooltip("It will toggle `ui` when key with `keyCode` pressed or button with `buttonName` pressed.")]
            public string buttonName;
        }

        [Header("Selected Target UIs")]
        public UICharacterEntity uiTargetCharacter;
        public UIBaseGameEntity uiTargetNpc;
        public UIBaseGameEntity uiTargetItemDrop;
        public UIBaseGameEntity uiTargetItemsContainer;
        public UIDamageableEntity uiTargetBuilding;
        public UIDamageableEntity uiTargetHarvestable;
        public UIBaseGameEntity uiTargetVehicle;

        [Header("Other UIs")]
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UINpcDialog uiNpcDialog;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIQuestRewardItemSelection uiQuestRewardItemSelection;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIRefineItem uiRefineItem;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIDismantleItem uiDismantleItem;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIBulkDismantleItems uiBulkDismantleItems;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIRepairItem uiRepairItem;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIEnhanceSocketItem uiEnhanceSocketItem;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIConstructBuilding uiConstructBuilding;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UICurrentBuilding uiCurrentBuilding;
        [Tooltip("If this UI was not set, it will use the same object with `uiCurrentBuilding`")]
        public UICurrentBuilding uiCurrentDoor;
        [Tooltip("If this UI was not set, it will use the same object with `uiCurrentBuilding`")]
        public UICurrentBuilding uiCurrentStorage;
        [Tooltip("If this UI was not set, it will use the same object with `uiCurrentBuilding`")]
        public UICurrentBuilding uiCurrentWorkbench;
        [Tooltip("If this UI was not set, it will use the same object with `uiCurrentBuilding`")]
        public UICurrentBuilding uiCurrentQueuedWorkbench;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIPlayerActivateMenu uiPlayerActivateMenu;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIDealingRequest uiDealingRequest;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIDealing uiDealing;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIPartyInvitation uiPartyInvitation;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIGuildInvitation uiGuildInvitation;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIStorageItems uiPlayerStorageItems;
        [Tooltip("If this UI was not set, it will use the same object with `uiPlayerStorageItems`")]
        public UIStorageItems uiGuildStorageItems;
        [Tooltip("If this UI was not set, it will use the same object with `uiPlayerStorageItems`")]
        public UIStorageItems uiBuildingStorageItems;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UICampfireItems uiBuildingCampfireItems;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIItemCrafts uiBuildingCraftItems;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UICraftingQueueItems uiCraftingQueueItems;
        [Tooltip("If this UI was not set, it will find component in children to set when `Awake`")]
        public UIItemsContainer uiItemsContainer;
        public UIBase uiIsWarping;

        [Header("Other Settings")]
        public List<UIToggleUI> toggleUis = new List<UIToggleUI>();
        [Tooltip("These Tags will ignore pointer over UIs detection when click or touch on screen")]
        public List<UnityTag> ignorePointerOverUITags = new List<UnityTag>();
        [Tooltip("These GameObjects will ignore pointer over UIs detection when click or touch on screen")]
        [FormerlySerializedAs("ignorePointerDetectionUis")]
        public List<GameObject> ignorePointerOverUIObjects = new List<GameObject>();
        [Tooltip("These UIs will blocks character controller inputs while visible")]
        [FormerlySerializedAs("blockControllerUIs")]
        public List<UIBase> blockControllerUis = new List<UIBase>();

        [Header("Events")]
        public UnityEvent onCharacterDead = new UnityEvent();
        public UnityEvent onCharacterRespawn = new UnityEvent();

        public System.Action<BuildingEntity> onShowConstructBuildingDialog;
        public System.Action onHideConstructBuildingDialog;
        public System.Action<BuildingEntity> onShowCurrentBuildingDialog;
        public System.Action onHideCurrentBuildingDialog;

        public override ItemsContainerEntity ItemsContainerEntity { get { return uiItemsContainer.TargetEntity; } }

        private readonly List<UIBase> _openedNpcDialogs = new List<UIBase>();
        private readonly List<RaycastResult> _pointerOverUIResults = new List<RaycastResult>();

        protected override void Awake()
        {
            base.Awake();
            SetUIComponentsFromChildrenIfEmpty();
            if (blockControllerUis != null && blockControllerUis.Count > 0)
            {
                foreach (UIBase ui in blockControllerUis)
                {
                    if (!ui)
                        continue;
                    if (ui.gameObject.GetComponent<UIBlockController>())
                        continue;
                    ui.gameObject.AddComponent<UIBlockController>();
                }
            }
            this.InvokeInstanceDevExtMethods("Awake");
        }

        protected void OnDestroy()
        {
            this.InvokeInstanceDevExtMethods("OnDestroy");
        }

        [ContextMenu("Set UI Components From Children If Empty")]
        public void SetUIComponentsFromChildrenIfEmpty()
        {
            if (uiNpcDialog == null)
                uiNpcDialog = gameObject.GetComponentInChildren<UINpcDialog>(true);
            if (uiQuestRewardItemSelection == null)
                uiQuestRewardItemSelection = gameObject.GetComponentInChildren<UIQuestRewardItemSelection>(true);
            if (uiRefineItem == null)
                uiRefineItem = gameObject.GetComponentInChildren<UIRefineItem>(true);
            if (uiDismantleItem == null)
                uiDismantleItem = gameObject.GetComponentInChildren<UIDismantleItem>(true);
            if (uiBulkDismantleItems == null)
                uiBulkDismantleItems = gameObject.GetComponentInChildren<UIBulkDismantleItems>(true);
            if (uiRepairItem == null)
                uiRepairItem = gameObject.GetComponentInChildren<UIRepairItem>(true);
            if (uiEnhanceSocketItem == null)
                uiEnhanceSocketItem = gameObject.GetComponentInChildren<UIEnhanceSocketItem>(true);
            if (uiConstructBuilding == null)
                uiConstructBuilding = gameObject.GetComponentInChildren<UIConstructBuilding>(true);
            if (uiCurrentBuilding == null)
                uiCurrentBuilding = gameObject.GetComponentInChildren<UICurrentBuilding>(true);
            if (uiCurrentDoor == null)
                uiCurrentDoor = uiCurrentBuilding;
            if (uiCurrentStorage == null)
                uiCurrentStorage = uiCurrentBuilding;
            if (uiCurrentWorkbench == null)
                uiCurrentWorkbench = uiCurrentBuilding;
            if (uiCurrentQueuedWorkbench == null)
                uiCurrentQueuedWorkbench = uiCurrentBuilding;
            if (uiPlayerActivateMenu == null)
                uiPlayerActivateMenu = gameObject.GetComponentInChildren<UIPlayerActivateMenu>(true);
            if (uiDealingRequest == null)
                uiDealingRequest = gameObject.GetComponentInChildren<UIDealingRequest>(true);
            if (uiDealing == null)
                uiDealing = gameObject.GetComponentInChildren<UIDealing>(true);
            if (uiPartyInvitation == null)
                uiPartyInvitation = gameObject.GetComponentInChildren<UIPartyInvitation>(true);
            if (uiGuildInvitation == null)
                uiGuildInvitation = gameObject.GetComponentInChildren<UIGuildInvitation>(true);
            if (uiPlayerStorageItems == null)
                uiPlayerStorageItems = gameObject.GetComponentInChildren<UIStorageItems>(true);
            if (uiGuildStorageItems == null)
                uiGuildStorageItems = uiPlayerStorageItems;
            if (uiBuildingStorageItems == null)
                uiBuildingStorageItems = uiPlayerStorageItems;
            if (uiBuildingCampfireItems == null)
                uiBuildingCampfireItems = gameObject.GetComponentInChildren<UICampfireItems>(true);
            if (uiBuildingCraftItems == null)
                uiBuildingCraftItems = gameObject.GetComponentInChildren<UIItemCrafts>(true);
            if (uiCraftingQueueItems == null)
                uiCraftingQueueItems = gameObject.GetComponentInChildren<UICraftingQueueItems>(true);
            if (uiItemsContainer == null)
                uiItemsContainer = gameObject.GetComponentInChildren<UIItemsContainer>(true);
        }

        protected override void Update()
        {
            base.Update();

            if (GenericUtils.IsFocusInputField())
                return;

            foreach (UIToggleUI toggleUi in toggleUis)
            {
                if (InputManager.GetKeyDown(toggleUi.keyCode) || InputManager.GetButtonDown(toggleUi.buttonName))
                    toggleUi.ui.Toggle();
            }
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
                SetTargetItemsContainer(null);
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
            if (entity is ItemsContainerEntity)
                SetTargetItemsContainer(entity as ItemsContainerEntity);
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

            uiTargetCharacter.hideWhileDead = false;
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

        protected void SetTargetItemsContainer(ItemsContainerEntity itemsContainer)
        {
            if (uiTargetItemsContainer == null)
                return;

            if (itemsContainer == null)
            {
                uiTargetItemsContainer.Hide();
                return;
            }

            uiTargetItemsContainer.Data = itemsContainer;
            uiTargetItemsContainer.Show();
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
            GameInstance.ClientCharacterHandlers.RequestRespawn(new RequestRespawnMessage()
            {
                option = 0,
            }, ClientCharacterActions.ResponseRespawn);
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

        public void ShowQuestRewardItemSelection(Quest quest)
        {
            if (uiQuestRewardItemSelection == null)
                return;

            if (quest == null)
            {
                uiQuestRewardItemSelection.Hide();
                return;
            }

            uiQuestRewardItemSelection.UpdateData(quest);
            uiQuestRewardItemSelection.Show();
            AddNpcDialog(uiQuestRewardItemSelection);
        }

        public override void HideQuestRewardItemSelection()
        {
            if (uiQuestRewardItemSelection.IsVisible())
                uiQuestRewardItemSelection.Hide();
        }

        public void ShowNpcDialog(BaseNpcDialog npcDialog)
        {
            if (uiNpcDialog == null)
                return;

            if (npcDialog == null)
            {
                uiNpcDialog.Hide();
                return;
            }

            uiNpcDialog.Data = npcDialog;
            uiNpcDialog.Show();
            AddNpcDialog(uiNpcDialog);
        }

        public override void HideNpcDialog()
        {
            for (int i = _openedNpcDialogs.Count - 1; i >= 0; --i)
            {
                if (_openedNpcDialogs[i].IsVisible())
                    _openedNpcDialogs[i].Hide();
                _openedNpcDialogs.RemoveAt(i);
            }
            GameInstance.PlayingCharacterEntity.NpcAction.CallServerHideNpcDialog();
        }

        public void OnShowNpcRefineItem()
        {
            if (uiRefineItem == null)
                return;
            // Don't select any item yet, wait player to select the item
            uiRefineItem.Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
            uiRefineItem.Show();
            AddNpcDialog(uiRefineItem);
        }

        public void OnShowNpcDismantleItem()
        {
            if (uiDismantleItem == null)
                return;
            // Don't select any item yet, wait player to select the item
            uiDismantleItem.Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
            uiDismantleItem.Show();
            AddNpcDialog(uiDismantleItem);
        }

        public void OnShowNpcRepairItem()
        {
            if (uiRepairItem == null)
                return;
            // Don't select any item yet, wait player to select the item
            uiRepairItem.Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
            uiRepairItem.Show();
            AddNpcDialog(uiRepairItem);
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

        public void OnNotifyPartyInvitation(PartyInvitationData invitation)
        {
            if (uiPartyInvitation == null)
                return;
            uiPartyInvitation.Data = invitation;
            uiPartyInvitation.Show();
        }

        public void OnNotifyGuildInvitation(GuildInvitationData invitation)
        {
            if (uiGuildInvitation == null)
                return;
            uiGuildInvitation.Data = invitation;
            uiGuildInvitation.Show();
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
            if (InputManager.IsDraggingMobileInput)
                return false;
            if (UIDragHandler.DraggingObjects.Count > 0)
                return true;
            if (EventSystem.current == null)
                return false;
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = new Vector2(InputManager.MousePosition().x, InputManager.MousePosition().y);
            EventSystem.current.RaycastAll(pointer, _pointerOverUIResults);

            int i;
            int j;
            bool containsTag = false;
            RaycastResult result;
            for (i = 0; i < _pointerOverUIResults.Count; ++i)
            {
                result = _pointerOverUIResults[i];
                // Find containing tags
                for (j = 0; j < ignorePointerOverUITags.Count; ++j)
                {
                    if (result.gameObject.CompareTag(ignorePointerOverUITags[j].Tag))
                    {
                        // Ignoring this one
                        containsTag = true;
                        break;
                    }
                }
                // Not ignored, so determining that the pointer is pointing over UIs
                if (!containsTag && !ignorePointerOverUIObjects.Contains(result.gameObject))
                {
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
            if (onShowConstructBuildingDialog != null)
                onShowConstructBuildingDialog.Invoke(buildingEntity);
        }

        public override void HideConstructBuildingDialog()
        {
            if (uiConstructBuilding.IsVisible())
                uiConstructBuilding.Hide();
            if (onHideConstructBuildingDialog != null)
                onHideConstructBuildingDialog.Invoke();
        }

        public override void ShowCurrentBuildingDialog(BuildingEntity buildingEntity)
        {
            HideCurrentBuildingDialog();
            if (buildingEntity == null)
                return;
            if (buildingEntity is DoorEntity)
            {
                if (!uiCurrentDoor.IsVisible())
                    uiCurrentDoor.Show(buildingEntity);
            }
            else if (buildingEntity is StorageEntity)
            {
                if (!uiCurrentStorage.IsVisible())
                    uiCurrentStorage.Show(buildingEntity);
            }
            else if (buildingEntity is WorkbenchEntity)
            {
                if (!uiCurrentWorkbench.IsVisible())
                    uiCurrentWorkbench.Show(buildingEntity);
            }
            else if (buildingEntity is QueuedWorkbenchEntity)
            {
                if (!uiCurrentQueuedWorkbench.IsVisible())
                    uiCurrentQueuedWorkbench.Show(buildingEntity);
            }
            else
            {
                if (!uiCurrentBuilding.IsVisible())
                    uiCurrentBuilding.Show(buildingEntity);
            }
            if (onShowCurrentBuildingDialog != null)
                onShowCurrentBuildingDialog.Invoke(buildingEntity);
        }

        public override void HideCurrentBuildingDialog()
        {
            if (uiCurrentDoor.IsVisible())
                uiCurrentDoor.Hide();
            if (uiCurrentStorage.IsVisible())
                uiCurrentStorage.Hide();
            if (uiCurrentWorkbench.IsVisible())
                uiCurrentWorkbench.Hide();
            if (uiCurrentQueuedWorkbench.IsVisible())
                uiCurrentQueuedWorkbench.Hide();
            if (uiCurrentBuilding.IsVisible())
                uiCurrentBuilding.Hide();
            if (onHideCurrentBuildingDialog != null)
                onHideCurrentBuildingDialog.Invoke();
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
            return (uiDismantleItem != null && uiDismantleItem.IsVisible()) ||
                (uiBulkDismantleItems != null && uiBulkDismantleItems.IsVisible());
        }

        public override bool IsRepairItemDialogVisible()
        {
            return uiRepairItem != null &&
                uiRepairItem.IsVisible();
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

        public override bool IsItemsContainerDialogVisible()
        {
            return uiItemsContainer != null &&
                uiItemsContainer.IsVisible();
        }

        public override bool IsDealingDialogVisibleWithDealingState()
        {
            return uiDealing != null && uiDealing.IsVisible() &&
                uiDealing.DealingState == DealingState.Dealing;
        }

        public override void ShowRefineItemDialog(InventoryType inventoryType, int indexOfData)
        {
            if (uiRefineItem == null)
                return;
            uiRefineItem.Data = new UIOwningCharacterItemData(inventoryType, indexOfData);
            uiRefineItem.Show();
        }

        public override void ShowDismantleItemDialog(InventoryType inventoryType, int indexOfData)
        {
            if (uiDismantleItem == null)
                return;
            uiDismantleItem.Data = new UIOwningCharacterItemData(inventoryType, indexOfData);
            uiDismantleItem.Show();
        }

        public override void ShowRepairItemDialog(InventoryType inventoryType, int indexOfData)
        {
            if (uiRepairItem == null)
                return;
            uiRepairItem.Data = new UIOwningCharacterItemData(inventoryType, indexOfData);
            uiRepairItem.Show();
        }

        public override void ShowEnhanceSocketItemDialog(InventoryType inventoryType, int indexOfData)
        {
            if (uiEnhanceSocketItem == null)
                return;
            uiEnhanceSocketItem.Data = new UIOwningCharacterItemData(inventoryType, indexOfData);
            uiEnhanceSocketItem.Show();
        }

        public override void ShowStorageDialog(StorageType storageType, string storageOwnerId, uint objectId, int weightLimit, int slotLimit)
        {
            // Hide all of storage UIs
            if (uiPlayerStorageItems != null)
                uiPlayerStorageItems.Hide(true);
            if (uiGuildStorageItems != null)
                uiGuildStorageItems.Hide(true);
            if (uiBuildingStorageItems != null)
                uiBuildingStorageItems.Hide(true);
            if (uiBuildingCampfireItems != null)
                uiBuildingCampfireItems.Hide(true);
            // Show only selected storage type
            switch (storageType)
            {
                case StorageType.Player:
                    if (uiPlayerStorageItems != null)
                    {
                        uiPlayerStorageItems.Show(storageType, storageOwnerId, null, weightLimit, slotLimit);
                        AddNpcDialog(uiPlayerStorageItems);
                    }
                    break;
                case StorageType.Guild:
                    if (uiGuildStorageItems != null)
                    {
                        uiGuildStorageItems.Show(storageType, storageOwnerId, null, weightLimit, slotLimit);
                        AddNpcDialog(uiGuildStorageItems);
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
                            uiBuildingCampfireItems.Show(storageType, storageOwnerId, buildingEntity, weightLimit, slotLimit);
                            AddNpcDialog(uiBuildingCampfireItems);
                        }
                    }
                    else if (buildingEntity is StorageEntity)
                    {
                        if (uiBuildingStorageItems != null)
                        {
                            uiBuildingStorageItems.Show(storageType, storageOwnerId, buildingEntity, weightLimit, slotLimit);
                            AddNpcDialog(uiBuildingStorageItems);
                        }
                    }
                    break;
            }
        }

        public override void HideStorageDialog()
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
        }

        public override void ShowItemsContainerDialog(ItemsContainerEntity itemsContainerEntity)
        {
            if (uiItemsContainer != null)
                uiItemsContainer.Show(itemsContainerEntity);
        }

        public override void HideItemsContainerDialog()
        {
            if (uiItemsContainer != null)
                uiItemsContainer.Hide();
        }

        public override void ShowWorkbenchDialog(WorkbenchEntity workbenchEntity)
        {
            if (uiBuildingCraftItems == null)
                return;
            uiBuildingCraftItems.Show(CrafterType.Workbench, workbenchEntity);
            AddNpcDialog(uiBuildingCraftItems);
        }

        public override void ShowCraftingQueueItemsDialog(ICraftingQueueSource source)
        {
            if (!GameInstance.Singleton.GameplayRule.CanInteractEntity(GameInstance.PlayingCharacterEntity, source.ObjectId))
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }
            if (uiCraftingQueueItems == null)
                return;
            uiCraftingQueueItems.Show(source);
            AddNpcDialog(uiCraftingQueueItems);
        }

        protected void AddNpcDialog(UIBase npcDialog)
        {
            if (!_openedNpcDialogs.Contains(npcDialog))
                _openedNpcDialogs.Add(npcDialog);
        }

        public override void OnControllerSetup(BasePlayerCharacterEntity characterEntity)
        {
            characterEntity.NpcAction.onShowQuestRewardItemSelection += ShowQuestRewardItemSelection;
            characterEntity.NpcAction.onShowNpcDialog += ShowNpcDialog;
            characterEntity.NpcAction.onShowNpcRefineItem += OnShowNpcRefineItem;
            characterEntity.NpcAction.onShowNpcDismantleItem += OnShowNpcDismantleItem;
            characterEntity.NpcAction.onShowNpcRepairItem += OnShowNpcRepairItem;
            characterEntity.Dealing.onShowDealingRequestDialog += OnShowDealingRequest;
            characterEntity.Dealing.onShowDealingDialog += OnShowDealing;
            characterEntity.onIsWarpingChange += OnIsWarpingChange;
            characterEntity.onDead.AddListener(OnCharacterDead);
            characterEntity.onRespawn.AddListener(OnCharacterRespawn);
            ClientPartyActions.onNotifyPartyInvitation += OnNotifyPartyInvitation;
            ClientGuildActions.onNotifyGuildInvitation += OnNotifyGuildInvitation;
        }

        public override void OnControllerDesetup(BasePlayerCharacterEntity characterEntity)
        {
            characterEntity.NpcAction.onShowQuestRewardItemSelection -= ShowQuestRewardItemSelection;
            characterEntity.NpcAction.onShowNpcDialog -= ShowNpcDialog;
            characterEntity.NpcAction.onShowNpcRefineItem -= OnShowNpcRefineItem;
            characterEntity.NpcAction.onShowNpcDismantleItem -= OnShowNpcDismantleItem;
            characterEntity.NpcAction.onShowNpcRepairItem -= OnShowNpcRepairItem;
            characterEntity.Dealing.onShowDealingRequestDialog -= OnShowDealingRequest;
            characterEntity.Dealing.onShowDealingDialog -= OnShowDealing;
            characterEntity.onIsWarpingChange -= OnIsWarpingChange;
            characterEntity.onDead.RemoveListener(OnCharacterDead);
            characterEntity.onRespawn.RemoveListener(OnCharacterRespawn);
            ClientPartyActions.onNotifyPartyInvitation -= OnNotifyPartyInvitation;
            ClientGuildActions.onNotifyGuildInvitation -= OnNotifyGuildInvitation;
        }
    }
}
