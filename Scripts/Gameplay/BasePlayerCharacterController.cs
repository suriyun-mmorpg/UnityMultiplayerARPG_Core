using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibHighLevel;

[RequireComponent(typeof(PlayerCharacterEntity))]
public abstract class BasePlayerCharacterController : MonoBehaviour
{
    public static BasePlayerCharacterController OwningCharacterController { get; protected set; }
    public static PlayerCharacterEntity OwningCharacter { get { return OwningCharacterController == null ? null : OwningCharacterController.CacheCharacterEntity; } }

    private PlayerCharacterEntity cacheCharacterEntity;
    public PlayerCharacterEntity CacheCharacterEntity
    {
        get
        {
            if (cacheCharacterEntity == null)
                cacheCharacterEntity = GetComponent<PlayerCharacterEntity>();
            return cacheCharacterEntity;
        }
    }

    public Transform CacheCharacterTransform
    {
        get { return CacheCharacterEntity.CacheTransform; }
    }

    public float stoppingDistance
    {
        get { return CacheCharacterEntity.stoppingDistance; }
    }

    public FollowCameraControls CacheMinimapCameraControls { get; protected set; }
    public FollowCameraControls CacheGameplayCameraControls { get; protected set; }
    public GameObject CacheTargetObject { get; protected set; }
    public UISceneGameplay CacheUISceneGameplay { get; protected set; }

    protected virtual void Awake()
    {
        CacheCharacterEntity.onDatabaseIdChange += OnDatabaseIdChange;
        CacheCharacterEntity.onEquipWeaponsChange += OnEquipWeaponsChange;
        CacheCharacterEntity.onAttributesOperation += OnAttributesOperation;
        CacheCharacterEntity.onSkillsOperation += OnSkillsOperation;
        CacheCharacterEntity.onBuffsOperation += OnBuffsOperation;
        CacheCharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
        CacheCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        CacheCharacterEntity.onHotkeysOperation += OnHotkeysOperation;
        CacheCharacterEntity.onQuestsOperation += OnQuestsOperation;
    }

    protected virtual void OnDestroy()
    {
        CacheCharacterEntity.onDatabaseIdChange -= OnDatabaseIdChange;
        CacheCharacterEntity.onEquipWeaponsChange -= OnEquipWeaponsChange;
        CacheCharacterEntity.onAttributesOperation -= OnAttributesOperation;
        CacheCharacterEntity.onSkillsOperation -= OnSkillsOperation;
        CacheCharacterEntity.onBuffsOperation -= OnBuffsOperation;
        CacheCharacterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
        CacheCharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        CacheCharacterEntity.onHotkeysOperation -= OnHotkeysOperation;
        CacheCharacterEntity.onQuestsOperation -= OnQuestsOperation;
        if (CacheUISceneGameplay != null)
        {
            CacheCharacterEntity.onShowNpcDialog -= CacheUISceneGameplay.OnShowNpcDialog;
            CacheCharacterEntity.onDead -= CacheUISceneGameplay.OnCharacterDead;
            CacheCharacterEntity.onRespawn -= CacheUISceneGameplay.OnCharacterRespawn;
        }
    }


    #region Sync data changes callback
    protected void OnDatabaseIdChange(string databaseId)
    {
        if (CacheCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateSkills();
            CacheUISceneGameplay.UpdateEquipItems();
            CacheUISceneGameplay.UpdateNonEquipItems();
        }
    }

    protected void OnEquipWeaponsChange(EquipWeapons equipWeapons)
    {
        if (CacheCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateEquipItems();
        }
    }

    protected void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CacheCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateCharacter();
    }

    protected void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CacheCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateSkills();
            CacheUISceneGameplay.UpdateHotkeys();
        }
    }

    protected void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CacheCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateCharacter();
    }

    protected void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CacheCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateEquipItems();
        }
    }

    protected void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CacheCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateNonEquipItems();
            CacheUISceneGameplay.UpdateHotkeys();
            CacheUISceneGameplay.UpdateQuests();
        }
    }

    protected void OnHotkeysOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CacheCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateHotkeys();
    }

    protected void OnQuestsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CacheCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateQuests();
    }
    #endregion

    protected virtual void Start()
    {
        var gameInstance = GameInstance.Singleton;
        if (CacheCharacterEntity.IsOwnerClient)
        {
            OwningCharacterController = this;
            CacheMinimapCameraControls = Instantiate(gameInstance.minimapCameraPrefab);
            CacheMinimapCameraControls.target = CacheCharacterTransform;
            CacheGameplayCameraControls = Instantiate(gameInstance.gameplayCameraPrefab);
            CacheGameplayCameraControls.target = CacheCharacterTransform;
            CacheTargetObject = Instantiate(gameInstance.targetObject);
            CacheTargetObject.gameObject.SetActive(false);
            if (gameInstance.uiSceneGameplayPrefab != null)
            {
                CacheUISceneGameplay = Instantiate(gameInstance.uiSceneGameplayPrefab);
                CacheUISceneGameplay.UpdateCharacter();
                CacheUISceneGameplay.UpdateSkills();
                CacheUISceneGameplay.UpdateEquipItems();
                CacheUISceneGameplay.UpdateNonEquipItems();
                CacheUISceneGameplay.UpdateHotkeys();
                CacheUISceneGameplay.UpdateQuests();
                CacheCharacterEntity.onShowNpcDialog += CacheUISceneGameplay.OnShowNpcDialog;
                CacheCharacterEntity.onDead += CacheUISceneGameplay.OnCharacterDead;
                CacheCharacterEntity.onRespawn += CacheUISceneGameplay.OnCharacterRespawn;
            }
        }
    }

    protected virtual void Update() { }

    public abstract void UseHotkey(int hotkeyIndex);
}
