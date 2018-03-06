using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibHighLevel;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterMovement))]
public class CharacterEntity : RpgNetworkEntity, ICharacterData
{
    public const string ANIM_IS_DEAD = "IsDead";
    public const string ANIM_MOVE_SPEED = "MoveSpeed";
    public const string ANIM_Y_SPEED = "YSpeed";
    public const string ANIM_DO_ACTION = "DoAction";
    public const string ANIM_ACTION_ID = "ActionId";
    public const float UPDATE_SKILL_BUFF_INTERVAL = 1f;
    // Use id as primary key
    [Header("Sync Fields")]
    public SyncFieldString id = new SyncFieldString();
    public SyncFieldString characterName = new SyncFieldString();
    public SyncFieldString prototypeId = new SyncFieldString();
    public SyncFieldInt level = new SyncFieldInt();
    public SyncFieldInt exp = new SyncFieldInt();
    public SyncFieldFloat currentHp = new SyncFieldFloat();
    public SyncFieldFloat currentMp = new SyncFieldFloat();
    public SyncFieldInt statPoint = new SyncFieldInt();
    public SyncFieldInt skillPoint = new SyncFieldInt();
    public SyncFieldInt gold = new SyncFieldInt();

    [Header("Sync Lists")]
    public SyncListCharacterAttributeLevel attributeLevels = new SyncListCharacterAttributeLevel();
    public SyncListCharacterSkillLevel skillLevels = new SyncListCharacterSkillLevel();
    public SyncListCharacterBuff buffs = new SyncListCharacterBuff();
    public SyncListCharacterItem equipItems = new SyncListCharacterItem();
    public SyncListCharacterItem nonEquipItems = new SyncListCharacterItem();

    #region Protected data
    // Entity data
    protected CharacterModel model;
    protected bool doingAction;
    protected readonly Dictionary<string, int> buffLocations = new Dictionary<string, int>();
    protected readonly Dictionary<string, int> equipItemLocations = new Dictionary<string, int>();
    protected readonly List<CharacterItem> equipWeapons = new List<CharacterItem>();
    protected float lastUpdateSkillAndBuffTime = 0f;
    // Net Functions
    protected LiteNetLibFunction netFuncAttack;
    protected LiteNetLibFunction<NetFieldInt> netFuncUseSkill;
    protected LiteNetLibFunction<NetFieldFloat, NetFieldInt> netFuncPlayActionAnimation;
    protected LiteNetLibFunction<NetFieldUInt> netFuncPickupItem;
    protected LiteNetLibFunction<NetFieldInt, NetFieldInt> netFuncDropItem;
    protected LiteNetLibFunction<NetFieldInt, NetFieldInt> netFuncSwapOrMergeItem;
    protected LiteNetLibFunction<NetFieldInt, NetFieldString> netFuncEquipItem;
    protected LiteNetLibFunction<NetFieldString, NetFieldInt> netFuncUnEquipItem;
    protected LiteNetLibFunction<NetFieldInt> netFuncAddAttributeLevel;
    protected LiteNetLibFunction<NetFieldInt> netFuncAddSkillLevel;
    #endregion

    public string Id { get { return id; } set { id.Value = value; } }
    public string CharacterName { get { return characterName; } set { characterName.Value = value; } }
    public string PrototypeId { get { return prototypeId; } set { prototypeId.Value = value; } }
    public int Level { get { return level.Value; } set { level.Value = value; } }
    public int Exp { get { return exp.Value; } set { exp.Value = value; } }
    public int CurrentHp { get { return (int)currentHp.Value; } set { currentHp.Value = value; } }
    public int CurrentMp { get { return (int)currentMp.Value; } set { currentMp.Value = value; } }
    public int StatPoint { get { return statPoint.Value; } set { statPoint.Value = value; } }
    public int SkillPoint { get { return skillPoint.Value; } set { skillPoint.Value = value; } }
    public int Gold { get { return gold.Value; } set { gold.Value = value; } }
    public string CurrentMapName { get; set; }
    public Vector3 CurrentPosition { get { return TempTransform.position; } set { TempTransform.position = value; } }
    public string RespawnMapName { get; set; }
    public Vector3 RespawnPosition { get; set; }
    public int LastUpdate { get; set; }

    public IList<CharacterAttributeLevel> AttributeLevels
    {
        get { return attributeLevels; }
        set
        {
            attributeLevels.Clear();
            foreach (var entry in value)
                attributeLevels.Add(entry);
        }
    }
    public IList<CharacterSkillLevel> SkillLevels
    {
        get { return skillLevels; }
        set
        {
            skillLevels.Clear();
            foreach (var entry in value)
                skillLevels.Add(entry);
        }
    }
    public IList<CharacterBuff> Buffs
    {
        get { return buffs; }
        set
        {
            buffs.Clear();
            foreach (var entry in value)
                buffs.Add(entry);
        }
    }
    public IList<CharacterItem> EquipItems
    {
        get { return equipItems; }
        set
        {
            equipItems.Clear();
            foreach (var entry in value)
                equipItems.Add(entry);
        }
    }
    public IList<CharacterItem> NonEquipItems
    {
        get { return nonEquipItems; }
        set
        {
            var gameInstance = GameInstance.Singleton;
            nonEquipItems.Clear();
            // Adjust inventory size
            var countItem = 0;
            foreach (var entry in value)
            {
                if (countItem < gameInstance.inventorySize)
                    nonEquipItems.Add(entry);
                ++countItem;
            }
            for (var i = countItem; i < gameInstance.inventorySize; ++i)
            {
                nonEquipItems.Add(new CharacterItem());
            }
        }
    }

    #region Temp components
    private CapsuleCollider tempCapsuleCollider;
    public CapsuleCollider TempCapsuleCollider
    {
        get
        {
            if (tempCapsuleCollider == null)
                tempCapsuleCollider = GetComponent<CapsuleCollider>();
            return tempCapsuleCollider;
        }
    }

    private Rigidbody tempRigidbody;
    public Rigidbody TempRigidbody
    {
        get
        {
            if (tempRigidbody == null)
                tempRigidbody = GetComponent<Rigidbody>();
            return tempRigidbody;
        }
    }

    private CharacterMovement tempCharacterMovement;
    public CharacterMovement TempCharacterMovement
    {
        get
        {
            if (tempCharacterMovement == null)
                tempCharacterMovement = GetComponent<CharacterMovement>();
            return tempCharacterMovement;
        }
    }

    public FollowCameraControls TempFollowCameraControls { get; protected set; }
    public UISceneGameplay TempUISceneGameplay { get; protected set; }
    #endregion

    protected virtual void Awake()
    {
        TempCharacterMovement.enabled = false;
    }

    protected virtual void Start()
    {
        var gameInstance = GameInstance.Singleton;
        if (IsLocalClient)
        {
            TempCharacterMovement.enabled = true;
            TempFollowCameraControls = Instantiate(gameInstance.gameplayCameraPrefab);
            TempFollowCameraControls.target = TempTransform;

            TempUISceneGameplay = Instantiate(gameInstance.uiSceneGameplayPrefab);
            TempUISceneGameplay.SetOwningCharacter(this);
        }
    }

    protected virtual void Update()
    {
        // Use this to update animations
        UpdateSkillAndBuff();
        UpdateAnimation();
    }

    protected virtual void UpdateAnimation()
    {
        if (model != null)
        {
            var isDead = CurrentHp <= 0;
            var velocity = TempRigidbody.velocity;
            var moveSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            if (isDead)
            {
                moveSpeed = 0f;
                // Force set to none action when dead
                model.TempAnimator.SetBool(ANIM_DO_ACTION, false);
            }
            model.TempAnimator.SetFloat(ANIM_MOVE_SPEED, moveSpeed);
            model.TempAnimator.SetFloat(ANIM_Y_SPEED, velocity.y);
            model.TempAnimator.SetBool(ANIM_IS_DEAD, isDead);
        }
    }

    protected void UpdateSkillAndBuff()
    {
        if (CurrentHp <= 0 || !IsServer)
            return;
        var timeDiff = Time.realtimeSinceStartup - lastUpdateSkillAndBuffTime;
        var count = skillLevels.Count;
        for (var i = count - 1; i >= 0; --i)
        {
            var skillLevel = skillLevels[i];
            if (skillLevel.ShouldUpdate())
            {
                skillLevel.Update(Time.unscaledDeltaTime);
                if (timeDiff > UPDATE_SKILL_BUFF_INTERVAL)
                    skillLevels.Dirty(i);
            }
        }
        count = buffs.Count;
        for (var i = count - 1; i >= 0; --i)
        {
            var buff = buffs[i];
            if (buff.ShouldRemove())
                buffs.RemoveAt(i);
            else
            {
                buff.Update(Time.unscaledDeltaTime);
                if (timeDiff > UPDATE_SKILL_BUFF_INTERVAL)
                    buffs.Dirty(i);
            }
        }
        if (timeDiff > UPDATE_SKILL_BUFF_INTERVAL)
            lastUpdateSkillAndBuffTime = Time.realtimeSinceStartup;
    }

    public override void OnBehaviourValidate()
    {
#if UNITY_EDITOR
        SetupNetElements();
        EditorUtility.SetDirty(this);
#endif
    }

    public override void OnSetup()
    {
        SetupNetElements();
        prototypeId.onChange += OnPrototypeIdChange;
        buffs.onOperation += OnBuffsOperation;
        equipItems.onOperation += OnEquipItemsOperation;
        nonEquipItems.onOperation += OnNonEquipItemsOperation;
        netFuncAttack = new LiteNetLibFunction(NetFuncAttackCallback);
        netFuncUseSkill = new LiteNetLibFunction<NetFieldInt>(NetFuncUseSkillCallback);
        netFuncPlayActionAnimation = new LiteNetLibFunction<NetFieldFloat, NetFieldInt>(NetFuncPlayActionAnimationCallback);
        netFuncPickupItem = new LiteNetLibFunction<NetFieldUInt>(NetFuncPickupItemCallback);
        netFuncDropItem = new LiteNetLibFunction<NetFieldInt, NetFieldInt>(NetFuncDropItemCallback);
        netFuncSwapOrMergeItem = new LiteNetLibFunction<NetFieldInt, NetFieldInt>(NetFuncSwapOrMergeItemCallback);
        netFuncEquipItem = new LiteNetLibFunction<NetFieldInt, NetFieldString>(NetFuncEquipItemCallback);
        netFuncUnEquipItem = new LiteNetLibFunction<NetFieldString, NetFieldInt>(NetFuncUnEquipItemCallback);
        netFuncAddAttributeLevel = new LiteNetLibFunction<NetFieldInt>(NetFuncAddAttributeLevelCallback);
        netFuncAddSkillLevel = new LiteNetLibFunction<NetFieldInt>(NetFuncAddSkillLevelCallback);
        RegisterNetFunction("Attack", netFuncAttack);
        RegisterNetFunction("PlayActionAnimation", netFuncPlayActionAnimation);
        RegisterNetFunction("PickupItem", netFuncPickupItem);
        RegisterNetFunction("DropItem", netFuncDropItem);
        RegisterNetFunction("SwapOrMergeItem", netFuncSwapOrMergeItem);
        RegisterNetFunction("EquipItem", netFuncEquipItem);
        RegisterNetFunction("UnEquipItem", netFuncUnEquipItem);
        RegisterNetFunction("AddAttributeLevel", netFuncAddAttributeLevel);
        RegisterNetFunction("AddSkillLevel", netFuncAddSkillLevel);
    }

    #region Net functions callbacks
    protected void NetFuncAttackCallback()
    {
        NetFuncAttack();
    }

    protected void NetFuncAttack()
    {
        if (CurrentHp <= 0 || model == null || doingAction)
            return;
        doingAction = true;

        WeaponItem weapon;
        var useSubAttackAnims = false;
        // Random left hand / right hand weapon
        if (equipWeapons.Count > 0)
        {
            var equipWeapon = equipWeapons[Random.Range(0, equipWeapons.Count - 1)];
            weapon = equipWeapon.GetWeaponItem();
            useSubAttackAnims = equipWeapon.isSubWeapon;
        }
        else
            weapon = GameInstance.Singleton.defaultWeaponItem;

        var weaponType = weapon.WeaponType;
        if (weaponType.subAttackAnimations == null || weaponType.subAttackAnimations.Length == 0)
            useSubAttackAnims = false;
        var animArray = useSubAttackAnims ? weaponType.subAttackAnimations : weaponType.mainAttackAnimations;
        var animLength = animArray.Length;
        if (animLength == 0)
        {
            doingAction = false;
            Debug.LogError("Cannot attack, animLength == 0 (" + weapon.Id + ")");
            return;
        }
        var anim = animArray[Random.Range(0, animLength - 1)];
        PlayActionAnimation(anim.totalDuration, anim.actionId);
        StartCoroutine(AttackRoutine(anim.triggerDuration, anim.totalDuration, weapon.TempDamageAmounts, weaponType.damage, weaponType.TempEffectivenessAttributes));
    }

    IEnumerator AttackRoutine(float damageDuration,
        float totalDuration,
        Dictionary<string, DamageAmount> damageAmounts,
        Damage damage,
        Dictionary<string, DamageEffectivenessAttribute> effectivenessAttributes)
    {
        yield return new WaitForSecondsRealtime(damageDuration);
        switch (damage.damageType)
        {
            case DamageType.Melee:
                var hits = Physics.OverlapSphere(TempTransform.position, model.radius + damage.hitDistance);
                foreach (var hit in hits)
                {
                    var characterEntity = hit.GetComponent<CharacterEntity>();
                    if (characterEntity == null)
                        continue;
                    characterEntity.ReceiveDamage(this, damageAmounts, effectivenessAttributes);
                }
                break;
            case DamageType.Missile:
                if (damage.missileDamageEntity != null)
                {
                    var missileDamageIdentity = Manager.Assets.NetworkSpawn(damage.missileDamageEntity.Identity, TempTransform.position);
                    var missileDamageEntity = missileDamageIdentity.GetComponent<MissileDamageEntity>();
                    missileDamageEntity.SetupDamage(this, damageAmounts, effectivenessAttributes, damage.missileDistance, damage.missileSpeed);
                }
                break;
        }
        yield return new WaitForSecondsRealtime(totalDuration - damageDuration);
        doingAction = false;
    }

    protected void NetFuncUseSkillCallback(NetFieldInt skillIndex)
    {
        NetFuncUseSkill(skillIndex);
    }

    protected void NetFuncUseSkill(int skillIndex)
    {
        if (CurrentHp <= 0 || model == null || doingAction || skillIndex < 0 || skillIndex >= skillLevels.Count)
            return;

        var characterSkill = skillLevels[skillIndex];
        if (!characterSkill.CanUse(CurrentMp))
            return;

        doingAction = true;

        var skill = characterSkill.GetSkill();
        var anim = skill.castAnimation;
        PlayActionAnimation(anim.totalDuration, anim.actionId);
        StartCoroutine(UseSkillRoutine(skillIndex));
    }

    IEnumerator UseSkillRoutine(int skillIndex)
    {
        var characterSkill = skillLevels[skillIndex];
        var skill = characterSkill.GetSkill();
        var anim = skill.castAnimation;
        yield return new WaitForSecondsRealtime(anim.triggerDuration);
        if (skill.isAttack)
        {
            var damageAmounts = skill.TempDamageAmounts;
            var damage = skill.damage;
            var effectivenessAttributes = skill.TempEffectivenessAttributes;
            switch (damage.damageType)
            {
                case DamageType.Melee:
                    var hits = Physics.OverlapSphere(TempTransform.position, model.radius + damage.hitDistance);
                    foreach (var hit in hits)
                    {
                        var characterEntity = hit.GetComponent<CharacterEntity>();
                        if (characterEntity == null)
                            continue;
                        characterEntity.ReceiveDamage(this, damageAmounts, effectivenessAttributes);
                    }
                    break;
                case DamageType.Missile:
                    if (damage.missileDamageEntity != null)
                    {
                        var missileDamageIdentity = Manager.Assets.NetworkSpawn(damage.missileDamageEntity.Identity, TempTransform.position);
                        var missileDamageEntity = missileDamageIdentity.GetComponent<MissileDamageEntity>();
                        missileDamageEntity.SetupDamage(this, damageAmounts, effectivenessAttributes, damage.missileDistance, damage.missileSpeed);
                    }
                    break;
            }
        }
        if (skill.isBuff)
        {
            // TODO: Implement buff add to another characters
            if (buffLocations.ContainsKey(characterSkill.skillId))
            {
                var buffIndex = buffLocations[characterSkill.skillId];
                // Don't update here let it update at update function to remove it
                buffs[buffIndex].buffRemainsDuration = 0;
            }
            var characterBuff = new CharacterBuff();
            characterBuff.skillId = characterSkill.skillId;
            characterBuff.level = characterSkill.level;
            characterBuff.Added();
            buffs.Add(characterBuff);
        }
        skillLevels[skillIndex].Used();
        skillLevels.Dirty(skillIndex);
        yield return new WaitForSecondsRealtime(anim.totalDuration - anim.triggerDuration);
        doingAction = false;
    }

    protected void NetFuncPlayActionAnimationCallback(NetFieldFloat duration, NetFieldInt actionId)
    {
        NetFuncPlayActionAnimation(duration, actionId);
    }

    protected void NetFuncPlayActionAnimation(float duration, int actionId)
    {
        if (CurrentHp <= 0 || model == null)
            return;

        StartCoroutine(PlayActionAnimationRoutine(duration, actionId));
    }

    IEnumerator PlayActionAnimationRoutine(float duration, int actionId)
    {
        var animator = model.TempAnimator;
        animator.SetBool(ANIM_DO_ACTION, true);
        animator.SetInteger(ANIM_ACTION_ID, actionId);
        yield return new WaitForSecondsRealtime(duration);
        animator.SetBool(ANIM_DO_ACTION, false);
    }

    protected void NetFuncPickupItemCallback(NetFieldUInt objectId)
    {
        NetFuncPickupItem(objectId);
    }

    protected void NetFuncPickupItem(uint objectId)
    {
        if (CurrentHp <= 0 || doingAction)
            return;

        var gameInstance = GameInstance.Singleton;
        var spawnedObjects = Manager.Assets.SpawnedObjects;
        // Find object by objectId, if not found don't continue
        if (!Manager.Assets.SpawnedObjects.ContainsKey(objectId))
            return;

        var spawnedObject = spawnedObjects[objectId];
        // Don't pickup item if it's too far
        if (Vector3.Distance(TempTransform.position, spawnedObject.transform.position) >= gameInstance.pickUpItemDistance)
            return;

        var itemDropEntity = spawnedObject.GetComponent<ItemDropEntity>();
        var itemDropData = itemDropEntity.dropData;
        if (!itemDropData.IsValid())
        {
            // Destroy item drop entity without item add because this is not valid
            Manager.Assets.NetworkDestroy(objectId);
            return;
        }
        var itemId = itemDropData.itemId;
        var level = itemDropData.level;
        var amount = itemDropData.amount;
        if (IncreaseItems(itemId, level, amount))
            Manager.Assets.NetworkDestroy(objectId);
    }

    protected void NetFuncDropItemCallback(NetFieldInt index, NetFieldInt amount)
    {
        NetFuncDropItem(index, amount);
    }

    protected void NetFuncDropItem(int index, int amount)
    {
        var gameInstance = GameInstance.Singleton;
        if (CurrentHp <= 0 || doingAction || index < 0 || index > nonEquipItems.Count)
            return;

        var nonEquipItem = nonEquipItems[index];
        if (!nonEquipItem.IsValid() || amount > nonEquipItem.amount)
            return;

        var itemId = nonEquipItem.itemId;
        var level = nonEquipItem.level;
        if (DecreaseItems(index, amount))
        {
            var dropPosition = TempTransform.position + new Vector3(Random.value * gameInstance.dropDistance, 0, Random.value * gameInstance.dropDistance);
            var identity = Manager.Assets.NetworkSpawn(gameInstance.itemDropEntityPrefab.gameObject, dropPosition);
            var itemDropEntity = identity.GetComponent<ItemDropEntity>();
            var dropData = new CharacterItem();
            dropData.itemId = itemId;
            dropData.level = level;
            dropData.amount = amount;
            itemDropEntity.dropData = dropData;
        }
    }

    protected void NetFuncSwapOrMergeItemCallback(NetFieldInt fromIndex, NetFieldInt toIndex)
    {
        NetFuncSwapOrMergeItem(fromIndex, toIndex);
    }

    protected void NetFuncSwapOrMergeItem(int fromIndex, int toIndex)
    {
        if (CurrentHp <= 0 || doingAction || 
            fromIndex < 0 || fromIndex > nonEquipItems.Count ||
            toIndex < 0 || toIndex > nonEquipItems.Count)
            return;

        var fromItem = nonEquipItems[fromIndex];
        var toItem = nonEquipItems[toIndex];
        if (!fromItem.IsValid() || !toItem.IsValid())
            return;

        if (fromItem.itemId.Equals(toItem.itemId) && !fromItem.IsFull() && !toItem.IsFull())
        {
            // Merge if same id and not full
            var maxStack = toItem.GetMaxStack();
            if (toItem.amount + fromItem.amount <= maxStack)
            {
                toItem.amount += fromItem.amount;
                fromItem.Empty();
                nonEquipItems[fromIndex] = fromItem;
                nonEquipItems[toIndex] = toItem;
            }
            else
            {
                var remains = toItem.amount + fromItem.amount - maxStack;
                toItem.amount = maxStack;
                fromItem.amount = remains;
                nonEquipItems[fromIndex] = fromItem;
                nonEquipItems[toIndex] = toItem;
            }
        }
        else
        {
            // Swap
            nonEquipItems[fromIndex] = toItem;
            nonEquipItems[toIndex] = fromItem;
        }
    }

    protected void NetFuncEquipItemCallback(NetFieldInt fromIndex, NetFieldString toEquipPosition)
    {
        NetFuncEquipItem(fromIndex, toEquipPosition);
    }

    protected void NetFuncEquipItem(int fromIndex, string toEquipPosition)
    {
        if (CurrentHp <= 0 || doingAction || fromIndex < 0 || fromIndex > nonEquipItems.Count || !GameInstance.EquipmentPositions.Contains(toEquipPosition))
            return;
        var equipItem = nonEquipItems[fromIndex];
        string reasonWhyCannot;
        if (!CanEquipItem(equipItem, toEquipPosition, out reasonWhyCannot))
            return;
        var weaponItem = equipItem.GetWeaponItem();
        var isSubWeapon = false;
        if (weaponItem != null && GameDataConst.EQUIP_POSITION_LEFT_HAND.Equals(toEquipPosition))
            isSubWeapon = true;
        // Unequip old item
        if (equipItemLocations.ContainsKey(toEquipPosition))
        {
            var equipItemIndex = equipItemLocations[toEquipPosition];
            var unEquipItem = equipItems[equipItemIndex];
            equipItems.RemoveAt(equipItemIndex);
            equipItem.isSubWeapon = isSubWeapon;
            equipItems.Add(equipItem);

            unEquipItem.isSubWeapon = false;
            nonEquipItems[fromIndex] = unEquipItem;
        }
        else
        {
            equipItem.isSubWeapon = isSubWeapon;
            equipItems.Add(equipItem);
            nonEquipItems[fromIndex].Empty();
            nonEquipItems.Dirty(fromIndex);
        }
    }

    protected void NetFuncUnEquipItemCallback(NetFieldString fromEquipPosition, NetFieldInt toIndex)
    {
        NetFuncUnEquipItem(fromEquipPosition, toIndex);
    }

    protected void NetFuncUnEquipItem(string fromEquipPosition, int toIndex)
    {
        if (CurrentHp <= 0 || doingAction || toIndex < 0 || toIndex > nonEquipItems.Count || !equipItemLocations.ContainsKey(fromEquipPosition) || !GameInstance.EquipmentPositions.Contains(fromEquipPosition))
            return;
        var fromIndex = equipItemLocations[fromEquipPosition];
        var unEquipItem = equipItems[fromIndex];
        var toItem = nonEquipItems[toIndex];
        // If drop slot is not empty, try to equip
        if (toItem.IsValid())
            NetFuncEquipItem(toIndex, fromEquipPosition);
        else
        {
            // Unequip to toIndex
            equipItems.RemoveAt(fromIndex);
            unEquipItem.isSubWeapon = false;
            nonEquipItems[toIndex] = unEquipItem;
        }
    }

    protected void NetFuncAddAttributeLevelCallback(NetFieldInt attributeIndex)
    {
        NetFuncAddAttributeLevel(attributeIndex);
    }

    protected void NetFuncAddAttributeLevel(int attributeIndex)
    {
        if (CurrentHp <= 0 || attributeIndex < 0 || attributeIndex >= attributeLevels.Count)
            return;

        var attributeLevel = attributeLevels[attributeIndex];
        if (attributeLevel.CanLevelUp())
            return;

        ++attributeLevels[attributeIndex].level;
        attributeLevels.Dirty(attributeIndex);
    }

    protected void NetFuncAddSkillLevelCallback(NetFieldInt skillIndex)
    {
        NetFuncAddSkillLevel(skillIndex);
    }

    protected void NetFuncAddSkillLevel(int skillIndex)
    {
        if (CurrentHp <= 0 || skillIndex < 0 || skillIndex >= skillLevels.Count)
            return;

        var characterSkill = skillLevels[skillIndex];
        if (characterSkill.CanLevelUp())
            return;

        ++skillLevels[skillIndex].level;
        skillLevels.Dirty(skillIndex);
    }
    #endregion

    #region Net functions callers
    public void Attack()
    {
        CallNetFunction("Attack", FunctionReceivers.Server);
    }

    public void PlayActionAnimation(float duration, int actionId)
    {
        CallNetFunction("PlayActionAnimation", FunctionReceivers.All, duration, actionId);
    }

    public void PickupItem(uint objectId)
    {
        CallNetFunction("PickupItem", FunctionReceivers.Server, objectId);
    }

    public void DropItem(int index, int amount)
    {
        CallNetFunction("DropItem", FunctionReceivers.Server, index, amount);
    }

    public void SwapOrMergeItem(CharacterItem fromItem, CharacterItem toItem)
    {
        SwapOrMergeItem(nonEquipItems.IndexOf(fromItem), nonEquipItems.IndexOf(toItem));
    }

    public void SwapOrMergeItem(int fromIndex, int toIndex)
    {
        CallNetFunction("SwapOrMergeItem", FunctionReceivers.Server, fromIndex, toIndex);
    }

    public void EquipItem(CharacterItem fromItem, CharacterItem toItem)
    {
        var equipmentItem = toItem.GetEquipmentItem();
        if (equipmentItem == null)
            return;
        var equipPosition = equipmentItem.equipPosition;
        var weaponItem = toItem.GetWeaponItem();
        var shieldItem = toItem.GetShieldItem();
        if (weaponItem != null)
            equipPosition = !toItem.isSubWeapon ? GameDataConst.EQUIP_POSITION_RIGHT_HAND : GameDataConst.EQUIP_POSITION_LEFT_HAND;
        else if (shieldItem != null)
            equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
        EquipItem(nonEquipItems.IndexOf(fromItem), equipPosition);
    }

    public void EquipItem(int fromIndex, string toEquipPosition)
    {
        CallNetFunction("EquipItem", FunctionReceivers.Server, fromIndex, toEquipPosition);
    }

    public void UnEquipItem(CharacterItem fromItem, CharacterItem toItem)
    {
        var equipmentItem = fromItem.GetEquipmentItem();
        if (equipmentItem == null)
            return;
        var equipPosition = equipmentItem.equipPosition;
        var weaponItem = fromItem.GetWeaponItem();
        var shieldItem = fromItem.GetShieldItem();
        if (weaponItem != null)
            equipPosition = !fromItem.isSubWeapon ? GameDataConst.EQUIP_POSITION_RIGHT_HAND : GameDataConst.EQUIP_POSITION_LEFT_HAND;
        else if (shieldItem != null)
            equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
        UnEquipItem(equipPosition, nonEquipItems.IndexOf(toItem));
    }

    public void UnEquipItem(string fromEquipPosition, int toIndex)
    {
        CallNetFunction("UnEquipItem", FunctionReceivers.Server, fromEquipPosition, toIndex);
    }

    public void AddAttributeLevel(int attributeIndex)
    {
        CallNetFunction("AddAttributeLevel", FunctionReceivers.Server, attributeIndex);
    }

    public void AddSkillLevel(int skillIndex)
    {
        CallNetFunction("AddSkillLevel", FunctionReceivers.Server, skillIndex);
    }
    #endregion

    #region Inventory helpers
    public bool IncreaseItems(string itemId, int level, int amount)
    {
        // If item not valid
        if (string.IsNullOrEmpty(itemId) || amount <= 0 || !GameInstance.Items.ContainsKey(itemId))
            return false;
        var maxStack = GameInstance.Items[itemId].maxStack;
        var emptySlots = new Dictionary<int, CharacterItem>();
        var changes = new Dictionary<int, CharacterItem>();
        // Loop to all slots to add amount to any slots that item amount not max in stack
        for (var i = 0; i < nonEquipItems.Count; ++i)
        {
            var nonEquipItem = nonEquipItems[i];
            if (!nonEquipItem.IsValid())
                emptySlots[i] = nonEquipItem;
            else if (nonEquipItem.itemId.Equals(itemId))
            {
                if (nonEquipItem.amount + amount <= maxStack)
                {
                    nonEquipItem.amount += amount;
                    changes[i] = nonEquipItem;
                    amount = 0;
                    break;
                }
                else if (maxStack - nonEquipItem.amount > 0)
                {
                    amount = maxStack - nonEquipItem.amount;
                    nonEquipItem.amount = amount;
                    changes[i] = nonEquipItem;
                }
            }
        }
        if (changes.Count == 0 && emptySlots.Count > 0)
        {
            foreach (var emptySlot in emptySlots)
            {
                var value = emptySlot.Value;
                var newItem = new CharacterItem();
                newItem.id = System.Guid.NewGuid().ToString();
                newItem.itemId = itemId;
                newItem.level = 1;
                var addAmount = 0;
                if (amount - maxStack >= 0)
                {
                    addAmount = maxStack;
                    amount -= maxStack;
                }
                else
                {
                    addAmount = amount;
                    amount = 0;
                }
                newItem.amount = addAmount;
                changes[emptySlot.Key] = newItem;
            }
        }
        // Cannot add all items
        if (amount > 0)
            return false;
        // Apply all changes
        foreach (var change in changes)
        {
            nonEquipItems[change.Key] = change.Value;
        }
        return true;
    }

    public bool DecreaseItems(int index, int amount)
    {
        if (index < 0 || index > nonEquipItems.Count)
            return false;
        var nonEquipItem = nonEquipItems[index];
        if (!nonEquipItem.IsValid() || amount > nonEquipItem.amount)
            return false;
        if (nonEquipItem.amount - amount == 0)
            nonEquipItems.RemoveAt(index);
        else
        {
            nonEquipItem.amount -= amount;
            nonEquipItems[index] = nonEquipItem;
        }
        return true;
    }

    public bool CanEquipItem(CharacterItem nonEquipItem, string equipPosition, out string reasonWhyCannot)
    {
        reasonWhyCannot = "";
        var equipmentItem = nonEquipItem.GetEquipmentItem();
        if (equipmentItem == null)
        {
            reasonWhyCannot = "This item is not equipment item";
            return false;
        }

        var weaponItem = nonEquipItem.GetWeaponItem();
        if (weaponItem != null)
        {
            switch (weaponItem.WeaponType.equipType)
            {
                case WeaponItemEquipType.OneHand:
                    // If weapon is one hand its equip position must be right hand
                    if (!GameDataConst.EQUIP_POSITION_RIGHT_HAND.Equals(equipPosition))
                    {
                        reasonWhyCannot = "Can equip to right hand only";
                        return false;
                    }
                    break;
                case WeaponItemEquipType.OneHandCanDual:
                    // If weapon is one hand can dual its equip position must be right or left hand
                    if (!GameDataConst.EQUIP_POSITION_RIGHT_HAND.Equals(equipPosition) &&
                        !GameDataConst.EQUIP_POSITION_LEFT_HAND.Equals(equipPosition))
                    {
                        reasonWhyCannot = "Can equip to right hand or left hand only";
                        return false;
                    }
                    break;
                case WeaponItemEquipType.TwoHand:
                    // If weapon is two hand its equip position must be right or left hand
                    if (!GameDataConst.EQUIP_POSITION_RIGHT_HAND.Equals(equipPosition) &&
                        !GameDataConst.EQUIP_POSITION_LEFT_HAND.Equals(equipPosition))
                    {
                        reasonWhyCannot = "Can equip to right hand or left hand only";
                        return false;
                    }
                    if (equipItemLocations.ContainsKey(GameDataConst.EQUIP_POSITION_RIGHT_HAND) &&
                        equipItemLocations.ContainsKey(GameDataConst.EQUIP_POSITION_LEFT_HAND))
                    {
                        reasonWhyCannot = "Have to unequip right hand or left hand equipment";
                        return false;
                    }
                    break;
            }
        }

        var shieldItem = nonEquipItem.GetShieldItem();
        if (shieldItem != null)
        {
            if (!GameDataConst.EQUIP_POSITION_LEFT_HAND.Equals(equipPosition))
            {
                reasonWhyCannot = "Can equip to left hand only";
                return false;
            }
        }

        if (!equipmentItem.equipPosition.Equals(equipPosition))
        {
            reasonWhyCannot = "Can equip to " + equipPosition + " only";
            return false;
        }
        return true;
    }

    public virtual void ReceiveDamage(CharacterEntity attacker, Dictionary<string, DamageAmount> damageAmounts, Dictionary<string, DamageEffectivenessAttribute> effectivenessAttributes)
    {
        // TODO: calculate damages
        if (CurrentHp <= 0)
        {
            StopAllCoroutines();
            doingAction = false;
            buffs.Clear();
            var count = skillLevels.Count;
            for (var i = 0; i < count; ++i)
            {
                var skillLevel = skillLevels[i];
                skillLevel.coolDownRemainsDuration = 0;
                skillLevels.Dirty(i);
            }
        }
    }
    #endregion

    private void SetupNetElements()
    {
        id.sendOptions = SendOptions.ReliableOrdered;
        characterName.sendOptions = SendOptions.ReliableOrdered;
        prototypeId.sendOptions = SendOptions.ReliableOrdered;
        level.sendOptions = SendOptions.ReliableOrdered;
        exp.sendOptions = SendOptions.ReliableOrdered;
        currentHp.sendOptions = SendOptions.ReliableOrdered;
        currentMp.sendOptions = SendOptions.ReliableOrdered;
        statPoint.sendOptions = SendOptions.ReliableOrdered;
        statPoint.forOwnerOnly = true;
        skillPoint.sendOptions = SendOptions.ReliableOrdered;
        skillPoint.forOwnerOnly = true;
        gold.sendOptions = SendOptions.ReliableOrdered;
        skillLevels.forOwnerOnly = true;
        nonEquipItems.forOwnerOnly = true;
    }

    protected void OnPrototypeIdChange(string prototypeId)
    {
        // Setup model
        if (model != null)
            Destroy(model.gameObject);
        model = this.InstantiateModel(transform);
        if (model != null)
        {
            TempCapsuleCollider.center = model.center;
            TempCapsuleCollider.radius = model.radius;
            TempCapsuleCollider.height = model.height;
            model.SetEquipItems(equipItems);
        }
    }

    protected void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        buffLocations.Clear();

        for (var i = 0; i < buffs.Count; ++i)
        {
            var buff = buffs[i];
            buffLocations[buff.skillId] = i;
        }
    }

    protected void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        equipItemLocations.Clear();
        equipWeapons.Clear();

        for (var i = 0; i < equipItems.Count; ++i)
        {
            var equipItem = equipItems[i];
            if (!equipItem.IsValid())
                continue;

            var equipmentItem = equipItem.GetEquipmentItem();
            if (equipmentItem == null)
                continue;

            var weaponItem = equipItem.GetWeaponItem();
            var shieldItem = equipItem.GetShieldItem();
            if (weaponItem != null)
            {
                equipWeapons.Add(equipItem);
                if (!equipItem.isSubWeapon)
                    equipItemLocations[GameDataConst.EQUIP_POSITION_RIGHT_HAND] = i;
                else
                    equipItemLocations[GameDataConst.EQUIP_POSITION_LEFT_HAND] = i;
            }
            else if (shieldItem != null)
                equipItemLocations[GameDataConst.EQUIP_POSITION_LEFT_HAND] = i;
            else
                equipItemLocations[equipmentItem.equipPosition] = i;
        }

        if (model != null)
            model.SetEquipItems(equipItems);

        if (TempUISceneGameplay != null)
            TempUISceneGameplay.SetEquipItems(equipItems);
    }

    protected void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (TempUISceneGameplay != null)
            TempUISceneGameplay.SetNonEquipItems(nonEquipItems);
    }

    public void Warp(string mapName, Vector3 position)
    {
        if (!IsServer)
            return;

        // If warping to same map player does not have to reload new map data
        if (string.IsNullOrEmpty(mapName) || mapName.Equals(CurrentMapName))
        {
            CurrentPosition = position;
            return;
        }
    }

    protected virtual void OnDestroy()
    {
        prototypeId.onChange -= OnPrototypeIdChange;
        buffs.onOperation -= OnBuffsOperation;
        equipItems.onOperation -= OnEquipItemsOperation;
        nonEquipItems.onOperation -= OnNonEquipItemsOperation;
    }
}
