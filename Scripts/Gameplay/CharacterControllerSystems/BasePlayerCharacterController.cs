using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public abstract partial class BasePlayerCharacterController : MonoBehaviour
    {
        public struct UsingSkillData
        {
            public AimPosition aimPosition;
            public BaseSkill skill;
            public short level;
            public short itemIndex;
            public UsingSkillData(AimPosition aimPosition, BaseSkill skill, short level, short itemIndex)
            {
                this.aimPosition = aimPosition;
                this.skill = skill;
                this.level = level;
                this.itemIndex = itemIndex;
            }

            public UsingSkillData(AimPosition aimPosition, BaseSkill skill, short level)
            {
                this.aimPosition = aimPosition;
                this.skill = skill;
                this.level = level;
                this.itemIndex = -1;
            }
        }

        public static BasePlayerCharacterController Singleton { get; protected set; }
        /// <summary>
        /// Controlled character, can use `GameInstance.PlayingCharacter` or `GameInstance.PlayingCharacterEntity` instead.
        /// </summary>
        public static BasePlayerCharacterEntity OwningCharacter { get { return Singleton == null ? null : Singleton.PlayerCharacterEntity; } }
        [SerializeField]
        protected InputField.ContentType buildingPasswordContentType = InputField.ContentType.Pin;
        [SerializeField]
        protected int buildingPasswordLength = 6;
        public System.Action<BasePlayerCharacterController> onSetup;
        public System.Action<BasePlayerCharacterController> onDesetup;
        public System.Action<BuildingEntity> onActivateBuilding;

        public BasePlayerCharacterEntity PlayerCharacterEntity
        {
            get { return GameInstance.PlayingCharacterEntity; }
            set
            {
                if (value.IsOwnerClient)
                {
                    Desetup(GameInstance.PlayingCharacterEntity);
                    GameInstance.PlayingCharacter = value;
                    GameInstance.OpenedStorageType = StorageType.None;
                    GameInstance.OpenedStorageOwnerId = string.Empty;
                    Setup(GameInstance.PlayingCharacterEntity);
                }
            }
        }

        public Transform CameraTargetTransform
        {
            get { return PlayerCharacterEntity.CameraTargetTransform; }
        }

        public Transform CacheTransform
        {
            get { return PlayerCharacterEntity.CacheTransform; }
        }

        public Transform MovementTransform
        {
            get { return PlayerCharacterEntity.MovementTransform; }
        }

        public float StoppingDistance
        {
            get { return PlayerCharacterEntity.StoppingDistance; }
        }

        public BaseUISceneGameplay CacheUISceneGameplay { get; protected set; }
        public GameInstance CurrentGameInstance { get { return GameInstance.Singleton; } }
        public BaseGameEntity SelectedEntity { get; protected set; }
        public uint SelectedEntityObjectId { get { return SelectedEntity != null ? SelectedEntity.ObjectId : 0; } }
        public BaseGameEntity TargetEntity { get; protected set; }
        public uint TargetEntityObjectId { get { return TargetEntity != null ? TargetEntity.ObjectId : 0; } }
        public BuildingEntity ConstructingBuildingEntity { get; protected set; }
        public BuildingEntity TargetBuildingEntity
        {
            get
            {
                if (TargetEntity is BuildingEntity)
                    return TargetEntity as BuildingEntity;
                return null;
            }
        }
        protected int buildingItemIndex;
        protected UsingSkillData queueUsingSkill;

        protected virtual void Awake()
        {
            Singleton = this;
            this.InvokeInstanceDevExtMethods("Awake");
        }

        protected virtual void Update()
        {
        }

        protected virtual void Setup(BasePlayerCharacterEntity characterEntity)
        {
            // Initial UI Scene gameplay
            if (CurrentGameInstance.UISceneGameplayPrefab != null)
                CacheUISceneGameplay = Instantiate(CurrentGameInstance.UISceneGameplayPrefab);
            if (CacheUISceneGameplay != null)
                CacheUISceneGameplay.OnControllerSetup(characterEntity);
            // Don't send navigation events
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem != null)
                eventSystem.sendNavigationEvents = false;
            if (onSetup != null)
                onSetup.Invoke(this);
        }

        protected virtual void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            if (CacheUISceneGameplay != null)
                Destroy(CacheUISceneGameplay.gameObject);
            if (onDesetup != null)
                onDesetup.Invoke(this);
        }

        protected virtual void OnDestroy()
        {
            Desetup(PlayerCharacterEntity);
            this.InvokeInstanceDevExtMethods("OnDestroy");
        }

        public virtual void ConfirmBuild()
        {
            if (ConstructingBuildingEntity == null)
                return;
            if (ConstructingBuildingEntity.CanBuild())
            {
                uint parentObjectId = 0;
                if (ConstructingBuildingEntity.BuildingArea != null)
                    parentObjectId = ConstructingBuildingEntity.BuildingArea.GetEntityObjectId();
                PlayerCharacterEntity.Building.CallServerConstructBuilding((short)buildingItemIndex, ConstructingBuildingEntity.CacheTransform.position, ConstructingBuildingEntity.CacheTransform.rotation, parentObjectId);
            }
            DestroyConstructingBuilding();
        }

        public virtual void CancelBuild()
        {
            DestroyConstructingBuilding();
        }

        public virtual BuildingEntity InstantiateConstructingBuilding(BuildingEntity prefab)
        {
            ConstructingBuildingEntity = Instantiate(prefab);
            ConstructingBuildingEntity.SetupAsBuildMode(PlayerCharacterEntity);
            ConstructingBuildingEntity.CacheTransform.parent = null;
            return ConstructingBuildingEntity;
        }

        public virtual void DestroyConstructingBuilding()
        {
            if (ConstructingBuildingEntity == null)
                return;
            Destroy(ConstructingBuildingEntity.gameObject);
            ConstructingBuildingEntity = null;
        }

        public virtual void DeselectBuilding()
        {
            TargetEntity = null;
        }

        public virtual void DestroyBuilding()
        {
            if (TargetBuildingEntity == null)
                return;
            PlayerCharacterEntity.Building.CallServerDestroyBuilding(TargetBuildingEntity.ObjectId);
            DeselectBuilding();
        }

        public virtual void SetBuildingPassword()
        {
            if (TargetBuildingEntity == null)
                return;
            uint objectId = TargetBuildingEntity.ObjectId;
            UISceneGlobal.Singleton.ShowPasswordDialog(
                LanguageManager.GetText(UITextKeys.UI_SET_BUILDING_PASSWORD.ToString()),
                LanguageManager.GetText(UITextKeys.UI_SET_BUILDING_PASSWORD_DESCRIPTION.ToString()),
                (password) =>
                {
                    PlayerCharacterEntity.Building.CallServerSetBuildingPassword(objectId, password);
                }, string.Empty, buildingPasswordContentType, buildingPasswordLength);
            DeselectBuilding();
        }

        public virtual void LockBuilding()
        {
            if (TargetBuildingEntity == null)
                return;
            PlayerCharacterEntity.Building.CallServerLockBuilding(TargetBuildingEntity.ObjectId);
            DeselectBuilding();
        }

        public virtual void UnlockBuilding()
        {
            if (TargetBuildingEntity == null)
                return;
            PlayerCharacterEntity.Building.CallServerUnlockBuilding(TargetBuildingEntity.ObjectId);
            DeselectBuilding();
        }

        protected void ShowConstructBuildingDialog()
        {
            if (!ConstructingBuildingEntity.CanBuild())
            {
                DestroyConstructingBuilding();
                CacheUISceneGameplay.HideConstructBuildingDialog();
                return;
            }
            CacheUISceneGameplay.ShowConstructBuildingDialog(ConstructingBuildingEntity);
        }

        protected void HideConstructBuildingDialog()
        {
            CacheUISceneGameplay.HideConstructBuildingDialog();
        }

        protected void ShowCurrentBuildingDialog()
        {
            CacheUISceneGameplay.ShowCurrentBuildingDialog(TargetBuildingEntity);
        }

        protected void HideCurrentBuildingDialog()
        {
            CacheUISceneGameplay.HideCurrentBuildingDialog();
        }

        protected void ShowItemsContainerDialog(ItemsContainerEntity itemsContainerEntity)
        {
            CacheUISceneGameplay.ShowItemsContainerDialog(itemsContainerEntity);
        }

        protected void HideItemsContainerDialog()
        {
            CacheUISceneGameplay.HideItemsContainerDialog();
        }

        protected void HideNpcDialog()
        {
            CacheUISceneGameplay.HideNpcDialog();
        }

        public void ActivateBuilding()
        {
            if (TargetBuildingEntity == null)
                return;
            ActivateBuilding(TargetBuildingEntity);
            DeselectBuilding();
        }

        public void ActivateBuilding(BuildingEntity buildingEntity)
        {
            uint objectId = buildingEntity.ObjectId;
            if (buildingEntity is DoorEntity)
            {
                if (!(buildingEntity as DoorEntity).IsOpen)
                {
                    if (!buildingEntity.Lockable || !buildingEntity.IsLocked)
                    {
                        OwningCharacter.Building.CallServerOpenDoor(objectId, string.Empty);
                    }
                    else
                    {
                        UISceneGlobal.Singleton.ShowPasswordDialog(
                            LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD.ToString()),
                            LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD_DESCRIPTION.ToString()),
                            (password) =>
                            {
                                OwningCharacter.Building.CallServerOpenDoor(objectId, password);
                            }, string.Empty, buildingPasswordContentType, buildingPasswordLength);
                    }
                }
                else
                {
                    OwningCharacter.Building.CallServerCloseDoor(objectId);
                }
            }

            if (buildingEntity is StorageEntity)
            {
                if (!buildingEntity.Lockable || !buildingEntity.IsLocked)
                {
                    OwningCharacter.CallServerOpenStorage(objectId, string.Empty);
                }
                else
                {
                    UISceneGlobal.Singleton.ShowPasswordDialog(
                            LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD.ToString()),
                            LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD_DESCRIPTION.ToString()),
                        (password) =>
                        {
                            OwningCharacter.CallServerOpenStorage(objectId, password);
                        }, string.Empty, buildingPasswordContentType, buildingPasswordLength);
                }
            }

            if (buildingEntity is WorkbenchEntity)
            {
                CacheUISceneGameplay.ShowWorkbenchDialog(buildingEntity as WorkbenchEntity);
            }

            if (buildingEntity is QueuedWorkbenchEntity)
            {
                CacheUISceneGameplay.ShowCraftingQueueItemsDialog(buildingEntity as QueuedWorkbenchEntity);
            }

            // Action when activate building for custom buildings
            // Can add event by `Awake` dev extension.
            if (onActivateBuilding != null)
                onActivateBuilding.Invoke(buildingEntity);
        }

        public void SetQueueUsingSkill(AimPosition aimPosition, BaseSkill skill, short level)
        {
            queueUsingSkill = new UsingSkillData(aimPosition, skill, level);
        }

        public void SetQueueUsingSkill(AimPosition aimPosition, BaseSkill skill, short level, short itemIndex)
        {
            queueUsingSkill = new UsingSkillData(aimPosition, skill, level, itemIndex);
        }

        public void ClearQueueUsingSkill()
        {
            queueUsingSkill = new UsingSkillData();
            queueUsingSkill.aimPosition = default;
            queueUsingSkill.skill = null;
            queueUsingSkill.level = 0;
            queueUsingSkill.itemIndex = -1;
        }

        public abstract void UseHotkey(HotkeyType type, string relateId, AimPosition aimPosition);
        public abstract AimPosition UpdateBuildAimControls(Vector2 aimAxes, BuildingEntity prefab);
        public abstract void FinishBuildAimControls(bool isCancel);
    }
}
