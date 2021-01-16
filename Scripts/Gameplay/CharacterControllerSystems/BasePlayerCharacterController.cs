using UnityEngine;
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
        /// <summary>
        /// Controlled character, can use `GameInstance.PlayingCharacter` or `GameInstance.PlayingCharacterEntity` instead.
        /// </summary>
        public static BasePlayerCharacterEntity OwningCharacter { get { return Singleton == null ? null : Singleton.PlayerCharacterEntity; } }
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
            if (CurrentGameInstance.UISceneGameplayPrefab != null)
                CacheUISceneGameplay = Instantiate(CurrentGameInstance.UISceneGameplayPrefab);
            if (CacheUISceneGameplay != null)
                CacheUISceneGameplay.OnControllerSetup(characterEntity);
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
                uint parentObjectId = ConstructingBuildingEntity.BuildingArea.GetEntityObjectId();
                PlayerCharacterEntity.CallServerConstructBuilding((short)buildingItemIndex, ConstructingBuildingEntity.CacheTransform.position, ConstructingBuildingEntity.CacheTransform.rotation, parentObjectId);
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
            PlayerCharacterEntity.CallServerDestroyBuilding(TargetBuildingEntity.ObjectId);
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
                    PlayerCharacterEntity.CallServerSetBuildingPassword(objectId, password);
                }, string.Empty, InputField.ContentType.Pin, 6);
            DeselectBuilding();
        }

        public virtual void LockBuilding()
        {
            if (TargetBuildingEntity == null)
                return;
            PlayerCharacterEntity.CallServerLockBuilding(TargetBuildingEntity.ObjectId);
            DeselectBuilding();
        }

        public virtual void UnlockBuilding()
        {
            if (TargetBuildingEntity == null)
                return;
            PlayerCharacterEntity.CallServerUnlockBuilding(TargetBuildingEntity.ObjectId);
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
                        OwningCharacter.CallServerOpenDoor(objectId, string.Empty);
                    }
                    else
                    {
                        UISceneGlobal.Singleton.ShowPasswordDialog(
                            LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD.ToString()),
                            LanguageManager.GetText(UITextKeys.UI_ENTER_BUILDING_PASSWORD_DESCRIPTION.ToString()),
                            (password) =>
                            {
                                OwningCharacter.CallServerOpenDoor(objectId, password);
                            }, string.Empty, InputField.ContentType.Pin, 6);
                    }
                }
                else
                {
                    OwningCharacter.CallServerCloseDoor(objectId);
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
                        }, string.Empty, InputField.ContentType.Pin, 6);
                }
            }

            if (buildingEntity is WorkbenchEntity)
            {
                CacheUISceneGameplay.ShowWorkbenchDialog(buildingEntity as WorkbenchEntity);
            }

            // Action when activate building for custom buildings
            // Can add event by `Awake` dev extension.
            if (onActivateBuilding != null)
                onActivateBuilding.Invoke(buildingEntity);
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

        public abstract void UseHotkey(HotkeyType type, string relateId, Vector3? aimPosition);
        public abstract Vector3? UpdateBuildAimControls(Vector2 aimAxes, BuildingEntity prefab);
        public abstract void FinishBuildAimControls(bool isCancel);
    }
}
