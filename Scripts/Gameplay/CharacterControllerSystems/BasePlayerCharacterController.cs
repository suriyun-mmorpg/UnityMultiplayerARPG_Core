using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public abstract partial class BasePlayerCharacterController : MonoBehaviour
    {
        public struct UsingSkillData
        {
            public Vector3? aimPosition;
            public BaseSkill skill;
            public short level;
            public short itemIndex;
            public UsingSkillData(Vector3? aimPosition, BaseSkill skill, short level, short itemIndex)
            {
                this.aimPosition = aimPosition;
                this.skill = skill;
                this.level = level;
                this.itemIndex = itemIndex;
            }

            public UsingSkillData(Vector3? aimPosition, BaseSkill skill, short level)
            {
                this.aimPosition = aimPosition;
                this.skill = skill;
                this.level = level;
                this.itemIndex = -1;
            }
        }

        public static BasePlayerCharacterController Singleton { get; protected set; }
        public static BasePlayerCharacterEntity OwningCharacter { get { return Singleton == null ? null : Singleton.PlayerCharacterEntity; } }

        public FollowCameraControls gameplayCameraPrefab;
        public FollowCameraControls minimapCameraPrefab;

        public System.Action<BasePlayerCharacterController> onSetup;
        public System.Action<BasePlayerCharacterController> onDesetup;

        private BasePlayerCharacterEntity playerCharacterEntity;
        public BasePlayerCharacterEntity PlayerCharacterEntity
        {
            get { return playerCharacterEntity; }
            set
            {
                if (value.IsOwnerClient)
                {
                    Desetup(playerCharacterEntity);
                    playerCharacterEntity = value;
                    Setup(playerCharacterEntity);
                }
            }
        }

        public Transform CameraTargetTransform
        {
            get { return PlayerCharacterEntity.CameraTargetTransform; }
        }

        public Transform MovementTransform
        {
            get { return PlayerCharacterEntity.MovementTransform; }
        }

        public float StoppingDistance
        {
            get { return PlayerCharacterEntity.StoppingDistance; }
        }

        public FollowCameraControls CacheGameplayCameraControls { get; protected set; }
        public FollowCameraControls CacheMinimapCameraControls { get; protected set; }
        public UISceneGameplay CacheUISceneGameplay { get; protected set; }
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }
        protected int buildingItemIndex;
        public BaseGameEntity SelectedEntity { get; protected set; }
        public BaseGameEntity TargetEntity { get; protected set; }
        public BuildingEntity CurrentBuildingEntity { get; protected set; }
        public BuildingEntity ActiveBuildingEntity
        {
            get
            {
                BuildingEntity building = TargetEntity as BuildingEntity;
                if (building != null)
                    return building;
                return null;
            }
            set { TargetEntity = value; }
        }
        public bool IsEditingBuilding { get; protected set; }
        protected UsingSkillData queueUsingSkill;

        protected virtual void Awake()
        {
            Singleton = this;
            this.InvokeInstanceDevExtMethods("Awake");

            if (gameplayCameraPrefab != null)
                CacheGameplayCameraControls = Instantiate(gameplayCameraPrefab);
            if (minimapCameraPrefab != null)
                CacheMinimapCameraControls = Instantiate(minimapCameraPrefab);
            if (gameInstance.UISceneGameplayPrefab != null)
                CacheUISceneGameplay = Instantiate(gameInstance.UISceneGameplayPrefab);
        }

        protected virtual void Update()
        {
            // Instantiate Minimap camera, it will render to render texture
            if (CacheGameplayCameraControls != null)
                CacheGameplayCameraControls.target = CameraTargetTransform;

            // Instantiate Minimap camera, it will render to render texture
            if (CacheMinimapCameraControls != null)
                CacheMinimapCameraControls.target = CameraTargetTransform;
        }

        protected virtual void Setup(BasePlayerCharacterEntity characterEntity)
        {
            if (characterEntity == null)
                return;

            // Instantiate gameplay UI
            if (CacheUISceneGameplay != null)
            {
                characterEntity.onShowNpcDialog += CacheUISceneGameplay.OnShowNpcDialog;
                characterEntity.onShowNpcRefine += CacheUISceneGameplay.OnShowNpcRefine;
                characterEntity.onDead += CacheUISceneGameplay.OnCharacterDead;
                characterEntity.onRespawn += CacheUISceneGameplay.OnCharacterRespawn;
                characterEntity.onShowDealingRequestDialog += CacheUISceneGameplay.OnShowDealingRequest;
                characterEntity.onShowDealingDialog += CacheUISceneGameplay.OnShowDealing;
                characterEntity.onUpdateDealingState += CacheUISceneGameplay.OnUpdateDealingState;
                characterEntity.onUpdateDealingGold += CacheUISceneGameplay.OnUpdateDealingGold;
                characterEntity.onUpdateDealingItems += CacheUISceneGameplay.OnUpdateDealingItems;
                characterEntity.onUpdateAnotherDealingState += CacheUISceneGameplay.OnUpdateAnotherDealingState;
                characterEntity.onUpdateAnotherDealingGold += CacheUISceneGameplay.OnUpdateAnotherDealingGold;
                characterEntity.onUpdateAnotherDealingItems += CacheUISceneGameplay.OnUpdateAnotherDealingItems;
                characterEntity.onShowPartyInvitationDialog += CacheUISceneGameplay.OnShowPartyInvitation;
                characterEntity.onShowGuildInvitationDialog += CacheUISceneGameplay.OnShowGuildInvitation;
                characterEntity.onShowStorage += CacheUISceneGameplay.OnShowStorage;
                characterEntity.onIsWarpingChange += CacheUISceneGameplay.OnIsWarpingChange;

                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateSkills();
                CacheUISceneGameplay.UpdateSummons();
                CacheUISceneGameplay.UpdateEquipItems();
                CacheUISceneGameplay.UpdateEquipWeapons();
                CacheUISceneGameplay.UpdateNonEquipItems();
                CacheUISceneGameplay.UpdateHotkeys();
                CacheUISceneGameplay.UpdateQuests();
                CacheUISceneGameplay.UpdateStorageItems();
            }
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

            if (onSetup != null)
                onSetup.Invoke(this);
        }

        protected virtual void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            if (CacheGameplayCameraControls != null)
                CacheGameplayCameraControls.target = null;

            if (CacheMinimapCameraControls != null)
                CacheMinimapCameraControls.target = null;

            if (characterEntity == null)
                return;

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

            if (CacheUISceneGameplay != null)
            {
                characterEntity.onShowNpcDialog -= CacheUISceneGameplay.OnShowNpcDialog;
                characterEntity.onShowNpcRefine -= CacheUISceneGameplay.OnShowNpcRefine;
                characterEntity.onDead -= CacheUISceneGameplay.OnCharacterDead;
                characterEntity.onRespawn -= CacheUISceneGameplay.OnCharacterRespawn;
                characterEntity.onShowDealingRequestDialog -= CacheUISceneGameplay.OnShowDealingRequest;
                characterEntity.onShowDealingDialog -= CacheUISceneGameplay.OnShowDealing;
                characterEntity.onUpdateDealingState -= CacheUISceneGameplay.OnUpdateDealingState;
                characterEntity.onUpdateDealingGold -= CacheUISceneGameplay.OnUpdateDealingGold;
                characterEntity.onUpdateDealingItems -= CacheUISceneGameplay.OnUpdateDealingItems;
                characterEntity.onUpdateAnotherDealingState -= CacheUISceneGameplay.OnUpdateAnotherDealingState;
                characterEntity.onUpdateAnotherDealingGold -= CacheUISceneGameplay.OnUpdateAnotherDealingGold;
                characterEntity.onUpdateAnotherDealingItems -= CacheUISceneGameplay.OnUpdateAnotherDealingItems;
                characterEntity.onShowPartyInvitationDialog -= CacheUISceneGameplay.OnShowPartyInvitation;
                characterEntity.onShowGuildInvitationDialog -= CacheUISceneGameplay.OnShowGuildInvitation;
                characterEntity.onShowStorage -= CacheUISceneGameplay.OnShowStorage;
                characterEntity.onIsWarpingChange -= CacheUISceneGameplay.OnIsWarpingChange;
            }

            if (onDesetup != null)
                onDesetup.Invoke(this);
        }

        protected virtual void OnDestroy()
        {
            Desetup(PlayerCharacterEntity);
            this.InvokeInstanceDevExtMethods("OnDestroy");

            if (CacheGameplayCameraControls != null)
                Destroy(CacheGameplayCameraControls.gameObject);
            if (CacheMinimapCameraControls != null)
                Destroy(CacheMinimapCameraControls.gameObject);
            if (CacheUISceneGameplay != null)
                Destroy(CacheUISceneGameplay.gameObject);
        }

        #region Sync data changes callback
        protected void OnIdChange(string id)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            {
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateSkills();
                CacheUISceneGameplay.UpdateEquipItems();
                CacheUISceneGameplay.UpdateNonEquipItems();
            }
        }

        protected void OnEquipWeaponSetChange(byte equipWeaponSet)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            {
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateEquipItems();
                CacheUISceneGameplay.UpdateEquipWeapons();
                CacheUISceneGameplay.UpdateSkills();
                CacheUISceneGameplay.UpdateHotkeys();
            }
        }

        protected void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            {
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateEquipItems();
                CacheUISceneGameplay.UpdateEquipWeapons();
                CacheUISceneGameplay.UpdateSkills();
                CacheUISceneGameplay.UpdateHotkeys();
            }
        }

        protected void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
                CacheUISceneGameplay.UpdateCharacter();
        }

        protected void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            {
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateSkills();
            }
        }

        protected void OnSummonsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
                CacheUISceneGameplay.UpdateSummons();
        }

        protected void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            {
                if (operation == LiteNetLibSyncList.Operation.Add ||
                    operation == LiteNetLibSyncList.Operation.RemoveAt ||
                    operation == LiteNetLibSyncList.Operation.RemoveFirst ||
                    operation == LiteNetLibSyncList.Operation.RemoveLast ||
                    operation == LiteNetLibSyncList.Operation.Insert ||
                    operation == LiteNetLibSyncList.Operation.Clear)
                    CacheUISceneGameplay.UpdateCharacter();
            }
        }

        protected void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            {
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateEquipItems();
                CacheUISceneGameplay.UpdateSkills();
                CacheUISceneGameplay.UpdateHotkeys();
            }
        }

        protected void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            {
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateNonEquipItems();
                CacheUISceneGameplay.UpdateHotkeys();
                CacheUISceneGameplay.UpdateQuests();
            }
        }

        protected void OnHotkeysOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
                CacheUISceneGameplay.UpdateHotkeys();
        }

        protected void OnQuestsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
                CacheUISceneGameplay.UpdateQuests();
        }

        protected void OnStorageItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
                CacheUISceneGameplay.UpdateStorageItems();
        }
        #endregion
        
        public void ConfirmBuild()
        {
            if (CurrentBuildingEntity != null)
            {
                if (CurrentBuildingEntity.CanBuild())
                {
                    uint parentObjectId = 0;
                    if (CurrentBuildingEntity.buildingArea != null)
                        parentObjectId = CurrentBuildingEntity.buildingArea.EntityObjectId;
                    PlayerCharacterEntity.RequestBuild((short)buildingItemIndex, CurrentBuildingEntity.CacheTransform.position, CurrentBuildingEntity.CacheTransform.rotation, parentObjectId);
                }
                Destroy(CurrentBuildingEntity.gameObject);
            }
        }

        public void CancelBuild()
        {
            if (CurrentBuildingEntity != null)
                Destroy(CurrentBuildingEntity.gameObject);
        }

        public void DestroyBuilding()
        {
            if (ActiveBuildingEntity == null)
                return;
            PlayerCharacterEntity.RequestDestroyBuilding(ActiveBuildingEntity.ObjectId);
            ActiveBuildingEntity = null;
            IsEditingBuilding = false;
        }

        public void DeselectBuilding()
        {
            if (ActiveBuildingEntity == null)
                return;
            ActiveBuildingEntity = null;
            IsEditingBuilding = false;
        }

        protected void HideNpcDialogs()
        {
            if (CacheUISceneGameplay != null)
            {
                if (CacheUISceneGameplay.uiNpcDialog != null &&
                    CacheUISceneGameplay.uiNpcDialog.IsVisible())
                    CacheUISceneGameplay.uiNpcDialog.Hide();

                if (CacheUISceneGameplay.uiPlayerStorageItems != null &&
                    CacheUISceneGameplay.uiPlayerStorageItems.IsVisible())
                    CacheUISceneGameplay.uiPlayerStorageItems.Hide();

                if (CacheUISceneGameplay.uiGuildStorageItems != null &&
                    CacheUISceneGameplay.uiGuildStorageItems.IsVisible())
                    CacheUISceneGameplay.uiGuildStorageItems.Hide();

                if (CacheUISceneGameplay.uiBuildingStorageItems != null &&
                    CacheUISceneGameplay.uiBuildingStorageItems.IsVisible())
                    CacheUISceneGameplay.uiBuildingStorageItems.Hide();

                if (CacheUISceneGameplay.uiBuildingCraftItems != null &&
                    CacheUISceneGameplay.uiBuildingCraftItems.IsVisible())
                    CacheUISceneGameplay.uiBuildingCraftItems.Hide();
            }
        }
        
        protected void ActivateBuilding(BuildingEntity buildingEntity)
        {
            if (buildingEntity is DoorEntity)
            {
                OwningCharacter.RequestToggleDoor(buildingEntity.ObjectId);
            }

            if (buildingEntity is StorageEntity)
            {
                OwningCharacter.RequestOpenStorage(buildingEntity.ObjectId);
            }

            if (buildingEntity is WorkbenchEntity)
            {
                if (CacheUISceneGameplay != null &&
                    CacheUISceneGameplay.uiBuildingCraftItems != null)
                {
                    CacheUISceneGameplay.uiBuildingCraftItems.UpdateDataForWorkbench(buildingEntity as WorkbenchEntity);
                    CacheUISceneGameplay.uiBuildingCraftItems.Show();
                }
            }
        }

        public void SetQueueUsingSkill(Vector3? aimPosition, BaseSkill skill, short level)
        {
            queueUsingSkill = new UsingSkillData(aimPosition, skill, level);
        }

        public void SetQueueUsingSkill(Vector3? aimPosition, BaseSkill skill, short level, short itemIndex)
        {
            queueUsingSkill = new UsingSkillData(aimPosition, skill, level, itemIndex);
        }

        public void ClearQueueUsingSkill()
        {
            queueUsingSkill = new UsingSkillData();
            queueUsingSkill.aimPosition = null;
            queueUsingSkill.skill = null;
            queueUsingSkill.level = 0;
            queueUsingSkill.itemIndex = -1;
        }

        public abstract void UseHotkey(int hotkeyIndex, Vector3? aimPosition);
    }
}
