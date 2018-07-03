using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public abstract class BasePlayerCharacterController : MonoBehaviour
    {
        public static BasePlayerCharacterController Singleton { get; protected set; }
        public static BasePlayerCharacterEntity OwningCharacter { get { return Singleton == null ? null : Singleton.PlayerCharacterEntity; } }

        public FollowCameraControls minimapCameraPrefab;

        private BasePlayerCharacterEntity playerCharacterEntity;
        public BasePlayerCharacterEntity PlayerCharacterEntity
        {
            get { return playerCharacterEntity; }
            set
            {
                if (value.IsOwnerClient)
                    playerCharacterEntity = value;
            }
        }

        public Transform CharacterTransform
        {
            get { return PlayerCharacterEntity.CacheTransform; }
        }

        public float StoppingDistance
        {
            get { return PlayerCharacterEntity.StoppingDistance; }
        }

        public FollowCameraControls CacheMinimapCameraControls { get; protected set; }
        public UISceneGameplay CacheUISceneGameplay { get; protected set; }
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }
        protected int buildingItemIndex;
        protected BuildingObject currentBuildingObject;

        protected virtual void Awake()
        {
            Singleton = this;
        }

        protected virtual void Start()
        {
            if (PlayerCharacterEntity == null)
                return;

            // Instantiate Minimap camera, it will render to render texture
            if (minimapCameraPrefab != null)
            {
                CacheMinimapCameraControls = Instantiate(minimapCameraPrefab);
                CacheMinimapCameraControls.target = CharacterTransform;
            }
            // Instantiate gameplay UI
            if (gameInstance.UISceneGameplayPrefab != null)
            {
                CacheUISceneGameplay = Instantiate(gameInstance.UISceneGameplayPrefab);
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateSkills();
                CacheUISceneGameplay.UpdateEquipItems();
                CacheUISceneGameplay.UpdateNonEquipItems();
                CacheUISceneGameplay.UpdateHotkeys();
                CacheUISceneGameplay.UpdateQuests();
                PlayerCharacterEntity.onShowNpcDialog += CacheUISceneGameplay.OnShowNpcDialog;
                PlayerCharacterEntity.onDead += CacheUISceneGameplay.OnCharacterDead;
                PlayerCharacterEntity.onRespawn += CacheUISceneGameplay.OnCharacterRespawn;
            }
            PlayerCharacterEntity.onDataIdChange += OnDataIdChange;
            PlayerCharacterEntity.onEquipWeaponsChange += OnEquipWeaponsChange;
            PlayerCharacterEntity.onAttributesOperation += OnAttributesOperation;
            PlayerCharacterEntity.onSkillsOperation += OnSkillsOperation;
            PlayerCharacterEntity.onBuffsOperation += OnBuffsOperation;
            PlayerCharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
            PlayerCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
            PlayerCharacterEntity.onHotkeysOperation += OnHotkeysOperation;
            PlayerCharacterEntity.onQuestsOperation += OnQuestsOperation;
        }

        protected virtual void OnDestroy()
        {
            PlayerCharacterEntity.onDataIdChange -= OnDataIdChange;
            PlayerCharacterEntity.onEquipWeaponsChange -= OnEquipWeaponsChange;
            PlayerCharacterEntity.onAttributesOperation -= OnAttributesOperation;
            PlayerCharacterEntity.onSkillsOperation -= OnSkillsOperation;
            PlayerCharacterEntity.onBuffsOperation -= OnBuffsOperation;
            PlayerCharacterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
            PlayerCharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
            PlayerCharacterEntity.onHotkeysOperation -= OnHotkeysOperation;
            PlayerCharacterEntity.onQuestsOperation -= OnQuestsOperation;
            if (CacheUISceneGameplay != null)
            {
                PlayerCharacterEntity.onShowNpcDialog -= CacheUISceneGameplay.OnShowNpcDialog;
                PlayerCharacterEntity.onDead -= CacheUISceneGameplay.OnCharacterDead;
                PlayerCharacterEntity.onRespawn -= CacheUISceneGameplay.OnCharacterRespawn;
            }
        }

        #region Sync data changes callback
        protected void OnDataIdChange(int dataId)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            {
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateSkills();
                CacheUISceneGameplay.UpdateEquipItems();
                CacheUISceneGameplay.UpdateNonEquipItems();
            }
        }

        protected void OnEquipWeaponsChange(EquipWeapons equipWeapons)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            {
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateEquipItems();
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
                CacheUISceneGameplay.UpdateHotkeys();
            }
        }

        protected void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
                CacheUISceneGameplay.UpdateCharacter();
        }

        protected void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (PlayerCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            {
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateEquipItems();
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
        #endregion

        protected virtual void Update() { }

        public void ConfirmBuild()
        {
            if (currentBuildingObject != null)
            {
                if (currentBuildingObject.CanBuild())
                {
                    uint parentObjectId = 0;
                    if (currentBuildingObject.buildingArea != null)
                        parentObjectId = currentBuildingObject.buildingArea.EntityObjectId;
                    PlayerCharacterEntity.RequestBuild(buildingItemIndex, currentBuildingObject.CacheTransform.position, currentBuildingObject.CacheTransform.rotation, parentObjectId);
                }
                Destroy(currentBuildingObject.gameObject);
            }
        }

        public void CancelBuild()
        {
            if (currentBuildingObject != null)
                Destroy(currentBuildingObject.gameObject);
        }

        public void DestroyBuilding()
        {
            BuildingEntity currentBuildingEntity;
            if (PlayerCharacterEntity.TryGetTargetEntity(out currentBuildingEntity))
            {
                PlayerCharacterEntity.RequestDestroyBuilding(currentBuildingEntity.ObjectId);
                PlayerCharacterEntity.SetTargetEntity(null);
            }
        }

        public void DeselectBuilding()
        {
            BuildingEntity currentBuildingEntity;
            if (PlayerCharacterEntity.TryGetTargetEntity(out currentBuildingEntity))
                PlayerCharacterEntity.SetTargetEntity(null);
        }

        public abstract void UseHotkey(int hotkeyIndex);
    }
}
