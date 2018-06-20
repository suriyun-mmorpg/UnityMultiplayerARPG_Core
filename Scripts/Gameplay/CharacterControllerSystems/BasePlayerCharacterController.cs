using UnityEngine;
using LiteNetLibManager;

public abstract class BasePlayerCharacterController : MonoBehaviour
{
    public static BasePlayerCharacterController Singleton { get; protected set; }
    public static PlayerCharacterEntity OwningCharacter { get { return Singleton == null ? null : Singleton.CharacterEntity; } }

    public FollowCameraControls minimapCameraPrefab;

    private PlayerCharacterEntity characterEntity;
    public PlayerCharacterEntity CharacterEntity
    {
        get { return characterEntity; }
        set
        {
            if (value.IsOwnerClient)
                characterEntity = value;
        }
    }

    public Transform CharacterTransform
    {
        get { return CharacterEntity.CacheTransform; }
    }

    public float StoppingDistance
    {
        get { return CharacterEntity.stoppingDistance; }
    }

    public FollowCameraControls CacheMinimapCameraControls { get; protected set; }
    public UISceneGameplay CacheUISceneGameplay { get; protected set; }
    protected GameInstance gameInstance { get { return GameInstance.Singleton; } }

    protected virtual void Awake()
    {
        Singleton = this;
    }

    protected virtual void Start()
    {
        if (CharacterEntity == null)
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
            CharacterEntity.onShowNpcDialog += CacheUISceneGameplay.OnShowNpcDialog;
            CharacterEntity.onDead += CacheUISceneGameplay.OnCharacterDead;
            CharacterEntity.onRespawn += CacheUISceneGameplay.OnCharacterRespawn;
        }
        CharacterEntity.onDataIdChange += OnDataIdChange;
        CharacterEntity.onEquipWeaponsChange += OnEquipWeaponsChange;
        CharacterEntity.onAttributesOperation += OnAttributesOperation;
        CharacterEntity.onSkillsOperation += OnSkillsOperation;
        CharacterEntity.onBuffsOperation += OnBuffsOperation;
        CharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
        CharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        CharacterEntity.onHotkeysOperation += OnHotkeysOperation;
        CharacterEntity.onQuestsOperation += OnQuestsOperation;
    }

    protected virtual void OnDestroy()
    {
        CharacterEntity.onDataIdChange -= OnDataIdChange;
        CharacterEntity.onEquipWeaponsChange -= OnEquipWeaponsChange;
        CharacterEntity.onAttributesOperation -= OnAttributesOperation;
        CharacterEntity.onSkillsOperation -= OnSkillsOperation;
        CharacterEntity.onBuffsOperation -= OnBuffsOperation;
        CharacterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
        CharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        CharacterEntity.onHotkeysOperation -= OnHotkeysOperation;
        CharacterEntity.onQuestsOperation -= OnQuestsOperation;
        if (CacheUISceneGameplay != null)
        {
            CharacterEntity.onShowNpcDialog -= CacheUISceneGameplay.OnShowNpcDialog;
            CharacterEntity.onDead -= CacheUISceneGameplay.OnCharacterDead;
            CharacterEntity.onRespawn -= CacheUISceneGameplay.OnCharacterRespawn;
        }
    }

    #region Sync data changes callback
    protected void OnDataIdChange(int dataId)
    {
        if (CharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateSkills();
            CacheUISceneGameplay.UpdateEquipItems();
            CacheUISceneGameplay.UpdateNonEquipItems();
        }
    }

    protected void OnEquipWeaponsChange(EquipWeapons equipWeapons)
    {
        if (CharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateEquipItems();
        }
    }

    protected void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateCharacter();
    }

    protected void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateSkills();
            CacheUISceneGameplay.UpdateHotkeys();
        }
    }

    protected void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateCharacter();
    }

    protected void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateEquipItems();
        }
    }

    protected void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateNonEquipItems();
            CacheUISceneGameplay.UpdateHotkeys();
            CacheUISceneGameplay.UpdateQuests();
        }
    }

    protected void OnHotkeysOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateHotkeys();
    }

    protected void OnQuestsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateQuests();
    }
    #endregion

    protected virtual void Update() { }

    public abstract void UseHotkey(int hotkeyIndex);
}
