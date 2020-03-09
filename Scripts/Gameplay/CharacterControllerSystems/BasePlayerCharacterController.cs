using UnityEngine;
using LiteNetLibManager;
using UnityEngine.UI;

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

        [SerializeField]
        private FollowCameraControls gameplayCameraPrefab;
        [SerializeField]
        private FollowCameraControls minimapCameraPrefab;

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
        public Camera CacheGameplayCamera { get { return CacheGameplayCameraControls.CacheCamera; } }
        public Camera CacheMiniMapCamera { get { return CacheMinimapCameraControls.CacheCamera; } }
        public Transform CacheGameplayCameraTransform { get { return CacheGameplayCameraControls.CacheCameraTransform; } }
        public Transform CacheMiniMapCameraTransform { get { return CacheMinimapCameraControls.CacheCameraTransform; } }
        public BaseUISceneGameplay CacheUISceneGameplay { get; protected set; }
        public GameInstance CurrentGameInstance { get { return GameInstance.Singleton; } }
        protected int buildingItemIndex;
        public BaseGameEntity SelectedEntity { get; protected set; }
        public BaseGameEntity TargetEntity { get; protected set; }
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
            if (CurrentGameInstance.UISceneGameplayPrefab != null)
                CacheUISceneGameplay = Instantiate(CurrentGameInstance.UISceneGameplayPrefab);
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
            // Instantiate gameplay UI
            if (CacheUISceneGameplay != null)
                CacheUISceneGameplay.OnControllerSetup(characterEntity);

            if (onSetup != null)
                onSetup.Invoke(this);
        }

        protected virtual void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            if (CacheGameplayCameraControls != null)
                CacheGameplayCameraControls.target = null;

            if (CacheMinimapCameraControls != null)
                CacheMinimapCameraControls.target = null;

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

        public virtual void ConfirmBuild()
        {
            if (ConstructingBuildingEntity == null)
                return;
            if (ConstructingBuildingEntity.CanBuild())
            {
                uint parentObjectId = 0;
                if (ConstructingBuildingEntity.BuildingArea)
                    parentObjectId = ConstructingBuildingEntity.BuildingArea.GetObjectId();
                PlayerCharacterEntity.RequestBuild((short)buildingItemIndex, ConstructingBuildingEntity.CacheTransform.position, ConstructingBuildingEntity.CacheTransform.rotation, parentObjectId);
            }
            Destroy(ConstructingBuildingEntity.gameObject);
            ConstructingBuildingEntity = null;
        }

        public virtual void CancelBuild()
        {
            if (ConstructingBuildingEntity == null)
                return;
            Destroy(ConstructingBuildingEntity.gameObject);
            ConstructingBuildingEntity = null;
        }

        public virtual void DeselectBuilding()
        {
            IsEditingBuilding = false;
            TargetEntity = null;
        }

        public virtual void DestroyBuilding()
        {
            if (TargetBuildingEntity == null)
                return;
            PlayerCharacterEntity.RequestDestroyBuilding(TargetBuildingEntity.ObjectId);
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
                    PlayerCharacterEntity.RequestSetBuildingPassword(objectId, password);
                }, string.Empty, InputField.ContentType.Pin, 6);
            DeselectBuilding();
        }

        public virtual void LockBuilding()
        {
            if (TargetBuildingEntity == null)
                return;
            PlayerCharacterEntity.RequestLockBuilding(TargetBuildingEntity.ObjectId);
            DeselectBuilding();
        }

        public virtual void UnlockBuilding()
        {
            if (TargetBuildingEntity == null)
                return;
            PlayerCharacterEntity.RequestUnlockBuilding(TargetBuildingEntity.ObjectId);
            DeselectBuilding();
        }

        protected void ShowConstructBuildingDialog()
        {
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
                        OwningCharacter.RequestOpenDoor(objectId, string.Empty);
                    }
                    else
                    {
                        UISceneGlobal.Singleton.ShowPasswordDialog(
                            LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD.ToString()),
                            LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD_DESCRIPTION.ToString()),
                            (password) =>
                            {
                                OwningCharacter.RequestOpenDoor(objectId, password);
                            }, string.Empty, InputField.ContentType.Pin, 6);
                    }
                }
                else
                {
                    OwningCharacter.RequestCloseDoor(objectId);
                }
            }

            if (buildingEntity is StorageEntity)
            {
                if (!buildingEntity.Lockable || !buildingEntity.IsLocked)
                {
                    OwningCharacter.RequestOpenStorage(objectId, string.Empty);
                }
                else
                {
                    UISceneGlobal.Singleton.ShowPasswordDialog(
                            LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD.ToString()),
                            LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD_DESCRIPTION.ToString()),
                        (password) =>
                        {
                            OwningCharacter.RequestOpenStorage(objectId, password);
                        }, string.Empty, InputField.ContentType.Pin, 6);
                }
            }

            if (buildingEntity is WorkbenchEntity)
            {
                CacheUISceneGameplay.ShowWorkbenchDialog(buildingEntity as WorkbenchEntity);
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
