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
                {
                    Desetup(playerCharacterEntity);
                    playerCharacterEntity = value;
                    Setup(playerCharacterEntity);
                }
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
        protected BuildingEntity currentBuildingEntity;

        protected virtual void Awake()
        {
            Singleton = this;
            if (minimapCameraPrefab != null)
                CacheMinimapCameraControls = Instantiate(minimapCameraPrefab);
            if (gameInstance.UISceneGameplayPrefab != null)
                CacheUISceneGameplay = Instantiate(gameInstance.UISceneGameplayPrefab);
        }

        protected virtual void Setup(BasePlayerCharacterEntity characterEntity)
        {
            if (characterEntity == null)
                return;

            // Instantiate Minimap camera, it will render to render texture
            if (CacheMinimapCameraControls != null)
                CacheMinimapCameraControls.target = characterEntity.CacheTransform;

            // Instantiate gameplay UI
            if (CacheUISceneGameplay != null)
            {
                characterEntity.onShowNpcDialog += CacheUISceneGameplay.OnShowNpcDialog;
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
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateSkills();
                CacheUISceneGameplay.UpdateEquipItems();
                CacheUISceneGameplay.UpdateNonEquipItems();
                CacheUISceneGameplay.UpdateHotkeys();
                CacheUISceneGameplay.UpdateQuests();
            }
            characterEntity.onDataIdChange += OnDataIdChange;
            characterEntity.onEquipWeaponsChange += OnEquipWeaponsChange;
            characterEntity.onAttributesOperation += OnAttributesOperation;
            characterEntity.onSkillsOperation += OnSkillsOperation;
            characterEntity.onBuffsOperation += OnBuffsOperation;
            characterEntity.onEquipItemsOperation += OnEquipItemsOperation;
            characterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
            characterEntity.onHotkeysOperation += OnHotkeysOperation;
            characterEntity.onQuestsOperation += OnQuestsOperation;
        }

        protected virtual void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            if (CacheMinimapCameraControls != null)
                CacheMinimapCameraControls.target = null;

            if (characterEntity == null)
                return;

            characterEntity.onDataIdChange -= OnDataIdChange;
            characterEntity.onEquipWeaponsChange -= OnEquipWeaponsChange;
            characterEntity.onAttributesOperation -= OnAttributesOperation;
            characterEntity.onSkillsOperation -= OnSkillsOperation;
            characterEntity.onBuffsOperation -= OnBuffsOperation;
            characterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
            characterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
            characterEntity.onHotkeysOperation -= OnHotkeysOperation;
            characterEntity.onQuestsOperation -= OnQuestsOperation;

            if (CacheUISceneGameplay != null)
            {
                characterEntity.onShowNpcDialog -= CacheUISceneGameplay.OnShowNpcDialog;
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
            }
        }

        protected virtual void OnDestroy()
        {
            Desetup(PlayerCharacterEntity);
            if (CacheMinimapCameraControls != null)
                Destroy(CacheMinimapCameraControls.gameObject);
            if (CacheUISceneGameplay != null)
                Destroy(CacheUISceneGameplay.gameObject);
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
            if (currentBuildingEntity != null)
            {
                if (currentBuildingEntity.CanBuild())
                {
                    uint parentObjectId = 0;
                    if (currentBuildingEntity.buildingArea != null)
                        parentObjectId = currentBuildingEntity.buildingArea.EntityObjectId;
                    PlayerCharacterEntity.RequestBuild(buildingItemIndex, currentBuildingEntity.CacheTransform.position, currentBuildingEntity.CacheTransform.rotation, parentObjectId);
                }
                Destroy(currentBuildingEntity.gameObject);
            }
        }

        public void CancelBuild()
        {
            if (currentBuildingEntity != null)
                Destroy(currentBuildingEntity.gameObject);
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
