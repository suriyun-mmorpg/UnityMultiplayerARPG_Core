using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using LiteNetLibHighLevel;

[RequireComponent(typeof(PlayerCharacterEntity))]
public class PlayerCharacterController : MonoBehaviour
{
    public static PlayerCharacterController OwningCharacterController { get; private set; }
    public static PlayerCharacterEntity OwningCharacter { get { return OwningCharacterController == null ? null : OwningCharacterController.CacheCharacterEntity; } }

    public struct UsingSkillData
    {
        public Vector3 position;
        public int skillIndex;
        public UsingSkillData(Vector3 position, int skillIndex)
        {
            this.position = position;
            this.skillIndex = skillIndex;
        }
    }

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

    public float attackDistance
    {
        get { return CacheCharacterEntity.GetAttackDistance(); }
    }

    public float stoppingDistance
    {
        get { return CacheCharacterEntity.stoppingDistance; }
    }

    public FollowCameraControls CacheMinimapCameraControls { get; protected set; }
    public FollowCameraControls CacheGameplayCameraControls { get; protected set; }
    public GameObject CacheTargetObject { get; protected set; }
    public UISceneGameplay CacheUISceneGameplay { get; protected set; }

    protected bool pointClickMoveStopped;
    protected Vector3 oldFollowTargetPosition;
    protected Vector3? destination;
    protected UsingSkillData? queueUsingSkill;

    private void Awake()
    {
        CacheCharacterEntity.onDatabaseIdChange += OnDatabaseIdChange;
        CacheCharacterEntity.onEquipWeaponsChange += OnEquipWeaponsChange;
        CacheCharacterEntity.onAttributesOperation += OnAttributesOperation;
        CacheCharacterEntity.onSkillsOperation += OnSkillsOperation;
        CacheCharacterEntity.onBuffsOperation += OnBuffsOperation;
        CacheCharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
        CacheCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        CacheCharacterEntity.onHotkeysOperation += OnHotkeysOperation;
    }

    private void OnDestroy()
    {
        CacheCharacterEntity.onDatabaseIdChange -= OnDatabaseIdChange;
        CacheCharacterEntity.onEquipWeaponsChange -= OnEquipWeaponsChange;
        CacheCharacterEntity.onAttributesOperation -= OnAttributesOperation;
        CacheCharacterEntity.onSkillsOperation -= OnSkillsOperation;
        CacheCharacterEntity.onBuffsOperation -= OnBuffsOperation;
        CacheCharacterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
        CacheCharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        CacheCharacterEntity.onHotkeysOperation -= OnHotkeysOperation;
        if (CacheUISceneGameplay != null)
        {
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
        }
    }

    protected void OnHotkeysOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (CacheCharacterEntity.IsOwnerClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateHotkeys();
    }
    #endregion

    private void Start()
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
                CacheCharacterEntity.onDead += CacheUISceneGameplay.OnCharacterDead;
                CacheCharacterEntity.onRespawn += CacheUISceneGameplay.OnCharacterRespawn;
            }
            // Set UI to spawned state
            CacheUISceneGameplay.OnCharacterRespawn();
        }
    }

    private void Update()
    {
        if (CacheTargetObject != null)
            CacheTargetObject.gameObject.SetActive(destination.HasValue);

        if (CacheCharacterEntity.CurrentHp <= 0)
        {
            queueUsingSkill = null;
            destination = null;
            if (CacheUISceneGameplay != null)
                CacheUISceneGameplay.SetTargetCharacter(null);
        }
        else
        {
            BaseCharacterEntity targetCharacter = null;
            CacheCharacterEntity.TryGetTargetEntity(out targetCharacter);
            if (CacheUISceneGameplay != null)
                CacheUISceneGameplay.SetTargetCharacter(targetCharacter);
        }

        if (destination.HasValue)
        {
            var destinationValue = destination.Value;
            if (CacheTargetObject != null)
                CacheTargetObject.transform.position = destinationValue;
            if (Vector3.Distance(destinationValue, CacheCharacterTransform.position) < stoppingDistance + 0.5f)
                destination = null;
        }


        UpdateInput();
    }

    protected virtual void UpdateInput()
    {
        if (!CacheCharacterEntity.IsOwnerClient)
            return;

        if (CacheGameplayCameraControls != null)
            CacheGameplayCameraControls.updateRotation = Input.GetMouseButton(1);

        if (CacheCharacterEntity.CurrentHp <= 0)
            return;

        var gameInstance = GameInstance.Singleton;
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            var targetCamera = CacheGameplayCameraControls != null ? CacheGameplayCameraControls.targetCamera : Camera.main;
            CacheCharacterEntity.SetTargetEntity(null);
            LiteNetLibIdentity targetIdentity = null;
            Vector3? targetPosition = null;
            RaycastHit[] hits = Physics.RaycastAll(targetCamera.ScreenPointToRay(Input.mousePosition), 100f);
            foreach (var hit in hits)
            {
                var hitTransform = hit.transform;
                targetPosition = hit.point;
                var playerEntity = hitTransform.GetComponent<PlayerCharacterEntity>();
                var monsterEntity = hitTransform.GetComponent<MonsterCharacterEntity>();
                var npcEntity = hitTransform.GetComponent<NpcEntity>();
                var itemDropEntity = hitTransform.GetComponent<ItemDropEntity>();
                if (playerEntity != null && playerEntity.CurrentHp > 0)
                {
                    targetPosition = playerEntity.CacheTransform.position;
                    targetIdentity = playerEntity.Identity;
                    CacheCharacterEntity.SetTargetEntity(playerEntity);
                    break;
                }
                else if (monsterEntity != null && monsterEntity.CurrentHp > 0)
                {
                    targetPosition = monsterEntity.CacheTransform.position;
                    targetIdentity = monsterEntity.Identity;
                    CacheCharacterEntity.SetTargetEntity(monsterEntity);
                    break;
                }
                else if (npcEntity != null)
                {
                    targetPosition = npcEntity.CacheTransform.position;
                    targetIdentity = npcEntity.Identity;
                    CacheCharacterEntity.SetTargetEntity(npcEntity);
                    break;
                }
                else if (itemDropEntity != null)
                {
                    targetPosition = itemDropEntity.CacheTransform.position;
                    targetIdentity = itemDropEntity.Identity;
                    CacheCharacterEntity.SetTargetEntity(itemDropEntity);
                    break;
                }
            }
            if (targetPosition.HasValue)
            {
                queueUsingSkill = null;
                if (targetIdentity != null)
                    destination = null;
                else
                    destination = targetPosition.Value;
                pointClickMoveStopped = false;
                CacheCharacterEntity.RequestPointClickMovement(targetPosition.Value);
            }
        }

        // Temp variables
        PlayerCharacterEntity targetPlayer;
        MonsterCharacterEntity targetMonster;
        NpcEntity targetNpc;
        ItemDropEntity targetItemDrop;
        if (CacheCharacterEntity.TryGetTargetEntity(out targetPlayer))
        {
            if (targetPlayer.CurrentHp <= 0)
            {
                queueUsingSkill = null;
                CacheCharacterEntity.SetTargetEntity(null);
                StopPointClickMove();
                return;
            }
            var actDistance = gameInstance.conversationDistance - stoppingDistance;
            if (Vector3.Distance(CacheCharacterTransform.position, targetPlayer.CacheTransform.position) <= actDistance)
            {
                StopPointClickMove();
                // TODO: do something
            }
            else
                UpdateTargetEntityPosition(targetPlayer);
        }
        else if (CacheCharacterEntity.TryGetTargetEntity(out targetMonster))
        {
            if (targetMonster.CurrentHp <= 0)
            {
                queueUsingSkill = null;
                CacheCharacterEntity.SetTargetEntity(null);
                StopPointClickMove();
                return;
            }
            var actDistance = attackDistance;
            actDistance -= actDistance * 0.1f;
            actDistance -= stoppingDistance;
            actDistance += targetMonster.CacheCapsuleCollider.radius;
            if (Vector3.Distance(CacheCharacterTransform.position, targetMonster.CacheTransform.position) <= actDistance)
            {
                StopPointClickMove();
                RequestAttack();
            }
            else
                UpdateTargetEntityPosition(targetMonster);
        }
        else if (CacheCharacterEntity.TryGetTargetEntity(out targetNpc))
        {
            var actDistance = gameInstance.conversationDistance - stoppingDistance;
            if (Vector3.Distance(CacheCharacterTransform.position, targetNpc.CacheTransform.position) <= actDistance)
            {
                StopPointClickMove();
                // TODO: implement npc conversation
            }
            else
                UpdateTargetEntityPosition(targetNpc);
        }
        else if (CacheCharacterEntity.TryGetTargetEntity(out targetItemDrop))
        {
            var actDistance = gameInstance.pickUpItemDistance - stoppingDistance;
            if (Vector3.Distance(CacheCharacterTransform.position, targetItemDrop.CacheTransform.position) <= actDistance)
            {
                StopPointClickMove();
                CacheCharacterEntity.RequestPickupItem();
                CacheCharacterEntity.SetTargetEntity(null);
            }
            else
                UpdateTargetEntityPosition(targetItemDrop);
        }
    }

    protected void UpdateTargetEntityPosition(RpgNetworkEntity entity)
    {
        if (entity == null)
            return;

        var targetPosition = entity.CacheTransform.position;
        if (oldFollowTargetPosition != targetPosition)
        {
            CacheCharacterEntity.RequestPointClickMovement(targetPosition);
            oldFollowTargetPosition = targetPosition;
        }
    }

    public void StopPointClickMove()
    {
        if (!pointClickMoveStopped)
            CacheCharacterEntity.RequestPointClickMovement(CacheCharacterTransform.position);
        pointClickMoveStopped = true;
    }

    public void RequestAttack()
    {
        if (!CacheCharacterEntity.isDoingAction.Value && queueUsingSkill.HasValue)
        {
            var usingSkill = queueUsingSkill.Value;
            RequestUseSkill(usingSkill.position, usingSkill.skillIndex);
            queueUsingSkill = null;
        }
        CacheCharacterEntity.RequestAttack();
    }

    public void RequestUseSkill(Vector3 position, int skillIndex)
    {
        if (CacheCharacterEntity.CurrentHp > 0 &&
            CacheCharacterEntity.isDoingAction.Value &&
            skillIndex >= 0 &&
            skillIndex < CacheCharacterEntity.skills.Count)
            queueUsingSkill = new UsingSkillData(position, skillIndex);
        CacheCharacterEntity.RequestUseSkill(position, skillIndex);
    }

    public void UseHotkey(int hotkeyIndex)
    {
        if (hotkeyIndex < 0 || hotkeyIndex >= CacheCharacterEntity.hotkeys.Count)
            return;

        var hotkey = CacheCharacterEntity.hotkeys[hotkeyIndex];
        var skill = hotkey.GetSkill();
        if (skill != null)
        {
            var skillIndex = CacheCharacterEntity.skills.IndexOf(skill.Id);
            BaseCharacterEntity target = null;
            CacheCharacterEntity.TryGetTargetEntity(out target);
            if (skillIndex >= 0)
                RequestUseSkill(CacheCharacterTransform.position, skillIndex);
        }
        var item = hotkey.GetItem();
        if (item != null)
        {
            // TODO: Implement use item functions
        }
    }
}
