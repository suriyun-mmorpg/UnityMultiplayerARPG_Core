using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibHighLevel;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CombatAmountTypes : byte
{
    Miss,
    NormalDamage,
    CriticalDamage,
    BlockedDamage,
    HpRecovery,
    MpRecovery,
}

public enum AnimActionTypes : byte
{
    Generic,
    Attack,
    Skill,
}

[RequireComponent(typeof(CapsuleCollider))]
public abstract class BaseCharacterEntity : RpgNetworkEntity, ICharacterData
{
    public const string ANIM_IS_DEAD = "IsDead";
    public const string ANIM_MOVE_SPEED = "MoveSpeed";
    public const string ANIM_Y_SPEED = "YSpeed";
    public const string ANIM_DO_ACTION = "DoAction";
    public const string ANIM_HURT = "Hurt";
    public const string ANIM_MOVE_CLIP_MULTIPLIER = "MoveSpeedMultiplier";
    public const string ANIM_ACTION_CLIP_MULTIPLIER = "ActionSpeedMultiplier";
    public const float RECOVERY_UPDATE_DURATION = 0.5f;

    // Use id as primary key
    #region Sync data
    [Header("Sync Fields")]
    public SyncFieldString id = new SyncFieldString();
    public SyncFieldString databaseId = new SyncFieldString();
    public SyncFieldString characterName = new SyncFieldString();
    public SyncFieldInt level = new SyncFieldInt();
    public SyncFieldInt exp = new SyncFieldInt();
    public SyncFieldInt currentHp = new SyncFieldInt();
    public SyncFieldInt currentMp = new SyncFieldInt();
    public SyncFieldEquipWeapons equipWeapons = new SyncFieldEquipWeapons();
    public SyncFieldBool isDoingAction = new SyncFieldBool();
    public SyncFieldBool isHidding = new SyncFieldBool();
    [Header("Sync Lists")]
    public SyncListCharacterAttribute attributes = new SyncListCharacterAttribute();
    public SyncListCharacterSkill skills = new SyncListCharacterSkill();
    public SyncListCharacterBuff buffs = new SyncListCharacterBuff();
    public SyncListCharacterItem equipItems = new SyncListCharacterItem();
    public SyncListCharacterItem nonEquipItems = new SyncListCharacterItem();
    #endregion

    #region Public data
    [Header("Settings")]
    [Tooltip("These objects will be hidden on non owner objects")]
    public GameObject[] ownerObjects;
    [Tooltip("These objects will be hidden on owner objects")]
    public GameObject[] nonOwnerObjects;
    [Tooltip("Model will be instantiated inside this transform, if not set will use this component's transform")]
    public Transform modelContainer;
    #endregion

    #region Protected data
    protected BaseCharacterDatabase database;
    protected RpgNetworkEntity targetEntity;
    protected CharacterModel model;
    protected readonly Dictionary<string, int> buffIndexes = new Dictionary<string, int>();
    protected readonly Dictionary<string, int> equipItemIndexes = new Dictionary<string, int>();
    protected Vector3? previousPosition;
    protected Vector3 currentVelocity;
    protected float recoveryingHp;
    protected float recoveryingMp;
    protected float recoveryTime;
    protected bool shouldRecaches;
    #endregion

    #region Caches Data
    public CharacterStats CacheStats { get; protected set; }
    public Dictionary<Attribute, int> CacheAttributes { get; protected set; }
    public Dictionary<Resistance, float> CacheResistances { get; protected set; }
    public int CacheMaxHp { get; protected set; }
    public int CacheMaxMp { get; protected set; }
    #endregion

    #region Sync data actions
    public System.Action<string> onIdChange;
    public System.Action<string> onDatabaseIdChange;
    public System.Action<string> onCharacterNameChange;
    public System.Action<int> onLevelChange;
    public System.Action<int> onExpChange;
    public System.Action<int> onCurrentHpChange;
    public System.Action<int> onCurrentMpChange;
    public System.Action<EquipWeapons> onEquipWeaponsChange;
    public System.Action<bool> onIsDoingActionChange;
    public System.Action<bool> onIsHiddingChange;
    // List
    public System.Action<LiteNetLibSyncList.Operation, int> onAttributesOperation;
    public System.Action<LiteNetLibSyncList.Operation, int> onSkillsOperation;
    public System.Action<LiteNetLibSyncList.Operation, int> onBuffsOperation;
    public System.Action<LiteNetLibSyncList.Operation, int> onEquipItemsOperation;
    public System.Action<LiteNetLibSyncList.Operation, int> onNonEquipItemsOperation;
    #endregion

    #region Another actions
    public System.Action<bool> onDead;
    public System.Action<bool> onRespawn;
    public System.Action onLevelUp;
    #endregion

    #region Fields/Interface implementation
    public virtual string Id { get { return id; } set { id.Value = value; } }
    public virtual string DatabaseId { get { return databaseId; } set { databaseId.Value = value; } }
    public virtual string CharacterName { get { return characterName; } set { characterName.Value = value; } }
    public virtual int Level { get { return level.Value; } set { level.Value = value; } }
    public virtual int Exp { get { return exp.Value; } set { exp.Value = value; } }
    public virtual int CurrentHp { get { return currentHp.Value; } set { currentHp.Value = value; } }
    public virtual int CurrentMp { get { return currentMp.Value; } set { currentMp.Value = value; } }
    public virtual EquipWeapons EquipWeapons { get { return equipWeapons; } set { equipWeapons.Value = value; } }
    public virtual float MoveSpeed { get { return this.GetMoveSpeed(); } }
    public virtual float AttackSpeed { get { return this.GetAttackSpeed(); } }

    public IList<CharacterAttribute> Attributes
    {
        get { return attributes; }
        set
        {
            attributes.Clear();
            foreach (var entry in value)
                attributes.Add(entry);
        }
    }

    public IList<CharacterSkill> Skills
    {
        get { return skills; }
        set
        {
            skills.Clear();
            foreach (var entry in value)
                skills.Add(entry);
        }
    }

    public IList<CharacterBuff> Buffs
    {
        get { return buffs; }
        set
        {
            buffIndexes.Clear();
            buffs.Clear();
            for (var i = 0; i < value.Count; ++i)
            {
                var entry = value[i];
                var buffId = entry.GetBuffId();
                if (!buffIndexes.ContainsKey(buffId))
                {
                    buffIndexes.Add(buffId, i);
                    buffs.Add(entry);
                }
            }
        }
    }

    public IList<CharacterItem> EquipItems
    {
        get { return equipItems; }
        set
        {
            equipItemIndexes.Clear();
            equipItems.Clear();
            for (var i = 0; i < value.Count; ++i)
            {
                var entry = value[i];
                var armorItem = entry.GetArmorItem();
                if (entry.IsValid() && armorItem != null && !equipItemIndexes.ContainsKey(armorItem.EquipPosition))
                {
                    equipItemIndexes.Add(armorItem.EquipPosition, i);
                    equipItems.Add(entry);
                }
            }
        }
    }

    public IList<CharacterItem> NonEquipItems
    {
        get { return nonEquipItems; }
        set
        {
            nonEquipItems.Clear();
            foreach (var entry in value)
                nonEquipItems.Add(entry);
        }
    }
    #endregion

    #region Cache components
    private CapsuleCollider cacheCapsuleCollider;
    public CapsuleCollider CacheCapsuleCollider
    {
        get
        {
            if (cacheCapsuleCollider == null)
                cacheCapsuleCollider = GetComponent<CapsuleCollider>();
            return cacheCapsuleCollider;
        }
    }

    public Transform CacheModelContainer
    {
        get
        {
            if (modelContainer == null)
                modelContainer = GetComponent<Transform>();
            return modelContainer;
        }
    }
    #endregion

    protected virtual void Awake()
    {
        var gameInstance = GameInstance.Singleton;
        gameObject.layer = gameInstance.characterLayer;
        shouldRecaches = true;
    }

    protected virtual void Start()
    {
        foreach (var ownerObject in ownerObjects)
        {
            ownerObject.SetActive(IsOwnerClient);
        }
        foreach (var nonOwnerObject in nonOwnerObjects)
        {
            nonOwnerObject.SetActive(!IsOwnerClient);
        }
        // Notify clients that this character is spawn or dead
        if (IsServer)
        {
            if (CurrentHp > 0)
                RequestOnRespawn(true);
            else
                RequestOnDead(true);
        }
    }

    protected virtual void Update()
    {
        MakeCaches();
        UpdateAnimation();
        UpdateSkillAndBuff();
        UpdateRecoverying();
    }

    protected virtual void FixedUpdate()
    {
        // Update current velocity
        if (!previousPosition.HasValue)
            previousPosition = CacheTransform.position;
        var currentMove = CacheTransform.position - previousPosition.Value;
        currentVelocity = currentMove / Time.deltaTime;
        previousPosition = CacheTransform.position;
    }

    protected virtual void UpdateAnimation()
    {
        if (model != null && model.gameObject.activeInHierarchy)
        {
            var animator = model.CacheAnimator;
            var velocity = currentVelocity;
            var moveSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            if (CurrentHp <= 0)
            {
                moveSpeed = 0f;
                // Force set to none action when dead
                animator.SetBool(ANIM_DO_ACTION, false);
            }
            animator.SetFloat(ANIM_MOVE_SPEED, moveSpeed);
            animator.SetFloat(ANIM_MOVE_CLIP_MULTIPLIER, MoveSpeed);
            animator.SetFloat(ANIM_Y_SPEED, velocity.y);
        }
    }

    protected virtual void UpdateSkillAndBuff()
    {
        if (CurrentHp <= 0 || !IsServer)
            return;
        var count = skills.Count;
        for (var i = count - 1; i >= 0; --i)
        {
            var skill = skills[i];
            if (skill.ShouldUpdate())
            {
                skill.Update(Time.unscaledDeltaTime);
                skills[i] = skill;
            }
        }
        count = buffs.Count;
        for (var i = count - 1; i >= 0; --i)
        {
            var buff = buffs[i];
            if (buff.ShouldRemove())
            {
                buffs.RemoveAt(i);
                UpdateBuffIndexes();
            }
            else
            {
                buff.Update(Time.unscaledDeltaTime);
                buffs[i] = buff;
            }
        }
    }

    protected virtual void UpdateRecoverying()
    {
        var maxHp = CacheMaxHp;
        var maxMp = CacheMaxMp;

        if (CurrentHp > 0)
        {
            var gameRule = GameInstance.Singleton.GameplayRule;
            var timeDiff = Time.realtimeSinceStartup - recoveryTime;
            if (timeDiff >= RECOVERY_UPDATE_DURATION)
            {
                recoveryingHp += timeDiff * gameRule.GetRecoveryHpPerSeconds(this);
                recoveryingMp += timeDiff * gameRule.GetRecoveryMpPerSeconds(this);
                recoveryTime = Time.realtimeSinceStartup;
                if (CurrentHp < maxHp)
                {
                    if (recoveryingHp >= 0)
                    {
                        var intRecoveryingHp = (int)recoveryingHp;
                        CurrentHp += intRecoveryingHp;
                        recoveryingHp -= intRecoveryingHp;
                    }
                }
                else
                    recoveryingHp = 0;

                if (CurrentMp < maxMp)
                {
                    if (recoveryingMp >= 0)
                    {
                        var intRecoveryingMp = (int)recoveryingMp;
                        CurrentMp += intRecoveryingMp;
                        recoveryingMp -= intRecoveryingMp;
                    }
                }
                else
                    recoveryingMp = 0;
            }
        }

        // Validates Hp / Mp
        if (CurrentHp < 0)
            CurrentHp = 0;
        if (CurrentMp < 0)
            CurrentMp = 0;
        if (CurrentHp > maxHp)
            CurrentHp = maxHp;
        if (CurrentMp > maxMp)
            CurrentMp = maxMp;
    }

    #region Setup functions
    public override void OnBehaviourValidate()
    {
        base.OnBehaviourValidate();
#if UNITY_EDITOR
        SetupNetElements();
        EditorUtility.SetDirty(this);
#endif
    }

    protected virtual void SetupNetElements()
    {
        id.sendOptions = SendOptions.ReliableOrdered;
        id.forOwnerOnly = false;
        databaseId.sendOptions = SendOptions.ReliableOrdered;
        databaseId.forOwnerOnly = false;
        characterName.sendOptions = SendOptions.ReliableOrdered;
        characterName.forOwnerOnly = false;
        level.sendOptions = SendOptions.ReliableOrdered;
        level.forOwnerOnly = false;
        exp.sendOptions = SendOptions.ReliableOrdered;
        exp.forOwnerOnly = false;
        currentHp.sendOptions = SendOptions.ReliableOrdered;
        currentHp.forOwnerOnly = false;
        currentMp.sendOptions = SendOptions.ReliableOrdered;
        currentMp.forOwnerOnly = false;
        equipWeapons.sendOptions = SendOptions.ReliableOrdered;
        equipWeapons.forOwnerOnly = false;
        isDoingAction.sendOptions = SendOptions.ReliableOrdered;
        isDoingAction.forOwnerOnly = true;
        isHidding.sendOptions = SendOptions.ReliableOrdered;
        isHidding.forOwnerOnly = false;

        attributes.forOwnerOnly = false;
        skills.forOwnerOnly = true;
        buffs.forOwnerOnly = false;
        equipItems.forOwnerOnly = false;
        nonEquipItems.forOwnerOnly = true;
    }

    public override void OnSetup()
    {
        SetupNetElements();
        // On data changes events
        id.onChange += OnIdChange;
        databaseId.onChange += OnDatabaseIdChange;
        characterName.onChange += OnCharacterNameChange;
        level.onChange += OnLevelChange;
        exp.onChange += OnExpChange;
        currentHp.onChange += OnCurrentHpChange;
        currentMp.onChange += OnCurrentMpChange;
        equipWeapons.onChange += OnEquipWeaponsChange;
        isDoingAction.onChange += OnIsDoingActionChange;
        isHidding.onChange += OnIsHiddingChange;
        // On list changes events
        attributes.onOperation += OnAttributesOperation;
        skills.onOperation += OnSkillsOperation;
        buffs.onOperation += OnBuffsOperation;
        equipItems.onOperation += OnEquipItemsOperation;
        nonEquipItems.onOperation += OnNonEquipItemsOperation;
        // Register Network functions
        RegisterNetFunction("Attack", new LiteNetLibFunction(() => NetFuncAttack(1, null, CharacterBuff.Empty)));
        RegisterNetFunction("UseSkill", new LiteNetLibFunction<NetFieldVector3, NetFieldInt>((position, skillIndex) => NetFuncUseSkill(position, skillIndex)));
        RegisterNetFunction("PlayActionAnimation", new LiteNetLibFunction<NetFieldInt, NetFieldByte>((actionId, animActionTypes) => NetFuncPlayActionAnimation(actionId, (AnimActionTypes)animActionTypes.Value)));
        RegisterNetFunction("PickupItem", new LiteNetLibFunction(() => NetFuncPickupItem()));
        RegisterNetFunction("DropItem", new LiteNetLibFunction<NetFieldInt, NetFieldInt>((index, amount) => NetFuncDropItem(index, amount)));
        RegisterNetFunction("EquipItem", new LiteNetLibFunction<NetFieldInt, NetFieldString>((nonEquipIndex, equipPosition) => NetFuncEquipItem(nonEquipIndex, equipPosition)));
        RegisterNetFunction("UnEquipItem", new LiteNetLibFunction<NetFieldString>((fromEquipPosition) => NetFuncUnEquipItem(fromEquipPosition)));
        RegisterNetFunction("CombatAmount", new LiteNetLibFunction<NetFieldByte, NetFieldInt>((combatAmountTypes, amount) => NetFuncCombatAmount((CombatAmountTypes)combatAmountTypes.Value, amount)));
        RegisterNetFunction("SetTargetEntity", new LiteNetLibFunction<NetFieldUInt>((objectId) => NetFuncSetTargetEntity(objectId)));
        RegisterNetFunction("OnDead", new LiteNetLibFunction<NetFieldBool>((isInitialize) => NetFuncOnDead(isInitialize)));
        RegisterNetFunction("OnRespawn", new LiteNetLibFunction<NetFieldBool>((isInitialize) => NetFuncOnRespawn(isInitialize)));
        RegisterNetFunction("OnLevelUp", new LiteNetLibFunction(() => NetFuncOnLevelUp()));
    }

    protected virtual void OnDestroy()
    {
        // On data changes events
        databaseId.onChange -= OnDatabaseIdChange;
        equipWeapons.onChange -= OnEquipWeaponsChange;
        // On list changes events
        attributes.onOperation -= OnAttributesOperation;
        skills.onOperation -= OnSkillsOperation;
        buffs.onOperation -= OnBuffsOperation;
        equipItems.onOperation -= OnEquipItemsOperation;
        nonEquipItems.onOperation -= OnNonEquipItemsOperation;
    }
    #endregion

    #region Net functions callbacks
    /// <summary>
    /// Is function will be called at server to order character to attack
    /// </summary>
    /// <param name="inflictRate">This will be multiplied with weapon damage to calculate total damage</param>
    /// <param name="additionalDamageAttributes">This will be sum with calculated weapon damage to calculate total damage</param>
    /// <param name="debuff">Debuff which will be applies to damage receivers</param>
    protected void NetFuncAttack(
        float inflictRate,
        Dictionary<DamageElement, DamageAmount> additionalDamageAttributes,
        CharacterBuff debuff)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;

        // Prepare requires data
        int actionId;
        float damageDuration;
        float totalDuration;
        DamageInfo damageInfo;
        Dictionary<DamageElement, DamageAmount> allDamageAttributes;

        GetAttackData(
            inflictRate,
            additionalDamageAttributes,
            out actionId,
            out damageDuration,
            out totalDuration,
            out damageInfo,
            out allDamageAttributes);

        isDoingAction.Value = true;
        // Play animation on clients
        RequestPlayActionAnimation(actionId, AnimActionTypes.Attack);
        // Start attack routine
        StartCoroutine(AttackRoutine(CacheTransform.position, damageDuration, totalDuration, damageInfo, allDamageAttributes, debuff));
    }

    IEnumerator AttackRoutine(
        Vector3 position,
        float damageDuration,
        float totalDuration,
        DamageInfo damageInfo,
        Dictionary<DamageElement, DamageAmount> allDamageAttributes,
        CharacterBuff debuff)
    {
        yield return new WaitForSecondsRealtime(damageDuration);
        LaunchDamageEntity(position, damageInfo, allDamageAttributes, debuff);
        yield return new WaitForSecondsRealtime(totalDuration - damageDuration);
        isDoingAction.Value = false;
    }

    /// <summary>
    /// Is function will be called at server to order character to use skill
    /// </summary>
    /// <param name="position">Target position to apply skill at</param>
    /// <param name="skillIndex">Index in `characterSkills` list which will be used</param>
    protected void NetFuncUseSkill(Vector3 position, int skillIndex)
    {
        if (CurrentHp <= 0 ||
            isDoingAction.Value ||
            skillIndex < 0 ||
            skillIndex >= skills.Count)
            return;

        var characterSkill = skills[skillIndex];
        if (!characterSkill.CanUse(this))
            return;

        var skill = characterSkill.GetSkill();
        isDoingAction.Value = true;
        var anim = skill.castAnimation;
        // Play animation on clients
        RequestPlayActionAnimation(anim.Id, AnimActionTypes.Skill);
        // Start use skill routine
        StartCoroutine(UseSkillRoutine(position, skillIndex));
    }

    IEnumerator UseSkillRoutine(Vector3 position, int skillIndex)
    {
        var characterSkill = skills[skillIndex];
        var skill = characterSkill.GetSkill();
        var anim = skill.castAnimation;
        yield return new WaitForSecondsRealtime(anim.TriggerDuration);
        characterSkill.Used();
        characterSkill.ReduceMp(this);
        skills[skillIndex] = characterSkill;
        switch (skill.skillAttackType)
        {
            case SkillAttackType.PureSkillDamage:
                AttackAsPureSkillDamage(characterSkill);
                break;
            case SkillAttackType.WeaponDamageInflict:
                AttackAsWeaponDamageInflict(characterSkill);
                break;
        }
        ApplySkillBuff(characterSkill);
        yield return new WaitForSecondsRealtime(anim.ClipLength + anim.extraDuration - anim.TriggerDuration);
        isDoingAction.Value = false;
    }

    protected void AttackAsPureSkillDamage(CharacterSkill characterSkill)
    {
        var skill = characterSkill.GetSkill();
        if (skill == null)
            return;

        // Calculate all damages
        var effectiveness = skill.GetDamageEffectiveness(this);
        var baseDamageAttribute = skill.GetDamageAttribute(characterSkill.level, effectiveness, 1f);
        var allDamageAttributes = skill.GetAdditionalDamageAttributes(characterSkill.level);
        allDamageAttributes = GameDataHelpers.CombineDamageAttributesDictionary(allDamageAttributes, baseDamageAttribute);
        var damageInfo = skill.damageInfo;
        var debuff = skill.isDebuff ? CharacterBuff.Create(Id, skill.Id, true, characterSkill.level) : CharacterBuff.Empty;
        LaunchDamageEntity(CacheTransform.position, damageInfo, allDamageAttributes, debuff);
    }

    protected void AttackAsWeaponDamageInflict(CharacterSkill characterSkill)
    {
        var skill = characterSkill.GetSkill();
        if (skill == null)
            return;

        var inflictRate = skill.GetInflictRate(characterSkill.level);
        var additionalDamageAttributes = skill.GetAdditionalDamageAttributes(characterSkill.level);
        var debuff = skill.isDebuff ? CharacterBuff.Create(Id, characterSkill.skillId, true, characterSkill.level) : CharacterBuff.Empty;
        NetFuncAttack(inflictRate, additionalDamageAttributes, debuff);
    }

    protected void ApplySkillBuff(CharacterSkill characterSkill)
    {
        var skill = characterSkill.GetSkill();
        if (skill.skillBuffType == SkillBuffType.BuffToUser)
        {
            var buffId = CharacterBuff.GetBuffId(Id, characterSkill.skillId, false);
            var buffIndex = -1;
            if (buffIndexes.TryGetValue(buffId, out buffIndex))
            {
                buffs.RemoveAt(buffIndex);
                UpdateBuffIndexes();
            }
            var characterBuff = CharacterBuff.Create(Id, characterSkill.skillId, false, characterSkill.level);
            characterBuff.Added();
            buffs.Add(characterBuff);
            buffIndexes.Add(buffId, buffs.Count - 1);
        }
    }

    /// <summary>
    /// This will be called at every clients to play any action animation
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="actionId"></param>
    protected void NetFuncPlayActionAnimation(int actionId, AnimActionTypes animActionTypes)
    {
        if (CurrentHp <= 0)
            return;
        StartCoroutine(PlayActionAnimationRoutine(actionId, animActionTypes));
    }

    IEnumerator PlayActionAnimationRoutine(int actionId, AnimActionTypes animActionTypes)
    {
        Animator animator = model == null ? null : model.CacheAnimator;
        // If animator is not null, play the action animation
        ActionAnimation actionAnimation;
        if (animator != null && GameInstance.ActionAnimations.TryGetValue(actionId, out actionAnimation) && actionAnimation.clip != null)
        {
            model.ChangeActionClip(actionAnimation.clip);
            var actionClipMultiplier = 1f;
            switch (animActionTypes)
            {
                case AnimActionTypes.Attack:
                    actionClipMultiplier = AttackSpeed;
                    break;
            }
            animator.SetFloat(ANIM_ACTION_CLIP_MULTIPLIER, actionClipMultiplier);
            animator.SetBool(ANIM_DO_ACTION, true);
            yield return new WaitForSecondsRealtime(actionAnimation.ClipLength / actionClipMultiplier);
            animator.SetBool(ANIM_DO_ACTION, false);
        }
    }

    /// <summary>
    /// This will be called at server to order character to pickup items
    /// </summary>
    /// <param name="objectId"></param>
    protected void NetFuncPickupItem()
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;

        var gameInstance = GameInstance.Singleton;
        ItemDropEntity itemDropEntity;
        var isFoundTargetEntity = TryGetTargetEntity(out itemDropEntity);
        // If have target entity but it's too far from character, don't pick it up
        if (isFoundTargetEntity && Vector3.Distance(CacheTransform.position, itemDropEntity.CacheTransform.position) >= gameInstance.pickUpItemDistance)
            return;

        // If target entity have not been set, try to pick up item randomly within range
        if (!isFoundTargetEntity)
        {
            var foundEntities = Physics.OverlapSphere(CacheTransform.position, gameInstance.pickUpItemDistance, gameInstance.itemDropLayer.Mask);
            foreach (var foundEntity in foundEntities)
            {
                itemDropEntity = foundEntity.GetComponent<ItemDropEntity>();
                if (itemDropEntity != null)
                    break;
            }
        }

        var itemDropData = itemDropEntity.dropData;
        if (!itemDropData.IsValid())
        {
            // Destroy item drop entity without item add because this is not valid
            itemDropEntity.NetworkDestroy();
            return;
        }
        var itemId = itemDropData.itemId;
        var level = itemDropData.level;
        var amount = itemDropData.amount;
        if (IncreaseItems(itemId, level, amount))
            itemDropEntity.NetworkDestroy();
    }

    /// <summary>
    /// This will be called at server to order character to drop items
    /// </summary>
    /// <param name="index"></param>
    /// <param name="amount"></param>
    protected void NetFuncDropItem(int index, int amount)
    {
        var gameInstance = GameInstance.Singleton;
        if (CurrentHp <= 0 ||
            isDoingAction.Value ||
            index < 0 ||
            index > nonEquipItems.Count)
            return;

        var nonEquipItem = nonEquipItems[index];
        if (!nonEquipItem.IsValid() || amount > nonEquipItem.amount)
            return;

        var itemId = nonEquipItem.itemId;
        var level = nonEquipItem.level;
        if (DecreaseItems(index, amount))
            ItemDropEntity.DropItem(this, itemId, level, amount);
    }

    /// <summary>
    /// This will be called at server to order character to equip equipments
    /// </summary>
    /// <param name="nonEquipIndex"></param>
    /// <param name="equipPosition"></param>
    protected void NetFuncEquipItem(int nonEquipIndex, string equipPosition)
    {
        if (CurrentHp <= 0 ||
            isDoingAction.Value ||
            nonEquipIndex < 0 ||
            nonEquipIndex > nonEquipItems.Count)
            return;

        var equippingItem = nonEquipItems[nonEquipIndex];

        string reasonWhyCannot;
        HashSet<string> shouldUnequipPositions;
        if (!CanEquipItem(equippingItem, equipPosition, out reasonWhyCannot, out shouldUnequipPositions))
        {
            Debug.LogError("Cannot equip item " + nonEquipIndex + " " + equipPosition + " " + reasonWhyCannot);
            return;
        }

        // Unequip equipped item if exists
        foreach (var shouldUnequipPosition in shouldUnequipPositions)
        {
            NetFuncUnEquipItem(shouldUnequipPosition);
        }
        // Equipping items
        var tempEquipWeapons = EquipWeapons;
        if (equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
        {
            tempEquipWeapons.rightHand = equippingItem;
            EquipWeapons = tempEquipWeapons;
        }
        else if (equipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
        {
            tempEquipWeapons.leftHand = equippingItem;
            EquipWeapons = tempEquipWeapons;
        }
        else
        {
            equipItems.Add(equippingItem);
            equipItemIndexes.Add(equipPosition, equipItems.Count - 1);
        }
        nonEquipItems.RemoveAt(nonEquipIndex);
    }

    /// <summary>
    /// This will be called at server to order character to unequip equipments
    /// </summary>
    /// <param name="fromEquipPosition"></param>
    protected void NetFuncUnEquipItem(string fromEquipPosition)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;

        var equippedArmorIndex = -1;
        var tempEquipWeapons = EquipWeapons;
        var unEquipItem = CharacterItem.Empty;
        if (fromEquipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
        {
            unEquipItem = tempEquipWeapons.rightHand;
            tempEquipWeapons.rightHand = CharacterItem.Empty;
            EquipWeapons = tempEquipWeapons;
        }
        else if (fromEquipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
        {
            unEquipItem = tempEquipWeapons.leftHand;
            tempEquipWeapons.leftHand = CharacterItem.Empty;
            EquipWeapons = tempEquipWeapons;
        }
        else if (equipItemIndexes.TryGetValue(fromEquipPosition, out equippedArmorIndex))
        {
            unEquipItem = equipItems[equippedArmorIndex];
            equipItems.RemoveAt(equippedArmorIndex);
            UpdateEquipItemIndexes();
        }
        if (unEquipItem.IsValid())
            nonEquipItems.Add(unEquipItem);
    }

    /// <summary>
    /// This will be called on clients to display combat texts
    /// </summary>
    /// <param name="combatAmountTypes"></param>
    /// <param name="amount"></param>
    protected void NetFuncCombatAmount(CombatAmountTypes combatAmountTypes, int amount)
    {
        var uiSceneGameplay = UISceneGameplay.Singleton;
        if (uiSceneGameplay == null)
            return;
        switch (combatAmountTypes)
        {
            case CombatAmountTypes.Miss:
                SpawnCombatText(uiSceneGameplay.uiCombatTextMiss, amount);
                break;
            case CombatAmountTypes.NormalDamage:
                SpawnCombatText(uiSceneGameplay.uiCombatTextNormalDamage, amount);
                break;
            case CombatAmountTypes.CriticalDamage:
                SpawnCombatText(uiSceneGameplay.uiCombatTextCriticalDamage, amount);
                break;
            case CombatAmountTypes.BlockedDamage:
                SpawnCombatText(uiSceneGameplay.uiCombatTextBlockedDamage, amount);
                break;
            case CombatAmountTypes.HpRecovery:
                SpawnCombatText(uiSceneGameplay.uiCombatTextHpRecovery, amount);
                break;
            case CombatAmountTypes.MpRecovery:
                SpawnCombatText(uiSceneGameplay.uiCombatTextMpRecovery, amount);
                break;
        }
    }

    protected void SpawnCombatText(UICombatText prefab, int amount)
    {
        var uiSceneGameplay = UISceneGameplay.Singleton;
        if (uiSceneGameplay == null)
            return;
        var combatTextTransform = CacheTransform;
        if (model != null)
            combatTextTransform = model.CombatTextTransform;
        if (uiSceneGameplay.combatTextTransform != null)
        {
            var combatText = Instantiate(prefab, uiSceneGameplay.combatTextTransform);
            combatText.transform.localScale = Vector3.one;
            combatText.CacheObjectFollower.targetObject = combatTextTransform;
            combatText.Amount = amount;
        }
    }

    /// <summary>
    /// This will be cladded on server to set target entity
    /// </summary>
    /// <param name="objectId"></param>
    protected void NetFuncSetTargetEntity(uint objectId)
    {
        RpgNetworkEntity entity;
        if (!Manager.Assets.TryGetSpawnedObject(objectId, out entity))
            return;
        SetTargetEntity(entity);
    }

    protected void NetFuncOnDead(bool isInitialize)
    {
        if (model != null && model.gameObject.activeInHierarchy)
        {
            var animator = model.CacheAnimator;
            animator.SetBool(ANIM_IS_DEAD, true);
        }
        if (onDead != null)
            onDead.Invoke(isInitialize);
    }

    protected void NetFuncOnRespawn(bool isInitialize)
    {
        if (model != null && model.gameObject.activeInHierarchy)
        {
            var animator = model.CacheAnimator;
            animator.SetBool(ANIM_IS_DEAD, false);
        }
        if (onRespawn != null)
            onRespawn.Invoke(isInitialize);
    }

    protected void NetFuncOnLevelUp()
    {
        if (onLevelUp != null)
            onLevelUp.Invoke();
    }
    #endregion

    #region Net functions callers
    public virtual void RequestAttack()
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("Attack", FunctionReceivers.Server);
    }

    public virtual void RequestUseSkill(Vector3 position, int skillIndex)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("UseSkill", FunctionReceivers.Server, position, skillIndex);
    }

    public virtual void RequestPlayActionAnimation(int actionId, AnimActionTypes animActionTypes)
    {
        if (CurrentHp <= 0)
            return;
        CallNetFunction("PlayActionAnimation", FunctionReceivers.All, actionId, animActionTypes);
    }

    public virtual void RequestPickupItem()
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("PickupItem", FunctionReceivers.Server);
    }

    public virtual void RequestDropItem(int index, int amount)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("DropItem", FunctionReceivers.Server, index, amount);
    }

    public virtual void RequestEquipItem(int nonEquipIndex, string equipPosition)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("EquipItem", FunctionReceivers.Server, nonEquipIndex, equipPosition);
    }

    public virtual void RequestUnEquipItem(string equipPosition)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("UnEquipItem", FunctionReceivers.Server, equipPosition);
    }

    public virtual void RequestCombatAmount(CombatAmountTypes combatAmountTypes, int amount)
    {
        CallNetFunction("CombatAmount", FunctionReceivers.All, combatAmountTypes, amount);
    }

    public virtual void RequestSetTargetEntity(uint objectId)
    {
        CallNetFunction("SetTargetEntity", FunctionReceivers.Server, objectId);
    }

    public virtual void RequestOnDead(bool isInitialize)
    {
        CallNetFunction("OnDead", FunctionReceivers.All, isInitialize);
    }

    public virtual void RequestOnRespawn(bool isInitialize)
    {
        CallNetFunction("OnRespawn", FunctionReceivers.All, isInitialize);
    }

    public virtual void RequestOnLevelUp()
    {
        CallNetFunction("OnLevelUp", FunctionReceivers.All);
    }
    #endregion

    #region Inventory helpers
    public bool IncreaseItems(string itemId, int level, int amount)
    {
        Item itemData;
        // If item not valid
        if (string.IsNullOrEmpty(itemId) || amount <= 0 || !GameInstance.Items.TryGetValue(itemId, out itemData))
            return false;
        
        var maxStack = itemData.maxStack;
        var weight = itemData.weight;
        // If overwhelming
        if (this.GetTotalItemWeight() + (amount * weight) >= CacheStats.weightLimit)
            return false;

        var emptySlots = new Dictionary<int, CharacterItem>();
        var changes = new Dictionary<int, CharacterItem>();
        // Loop to all slots to add amount to any slots that item amount not max in stack
        for (var i = 0; i < nonEquipItems.Count; ++i)
        {
            var nonEquipItem = nonEquipItems[i];
            if (!nonEquipItem.IsValid())
            {
                // If current entry is not valid, add it to empty list, going to replacing it later
                emptySlots[i] = nonEquipItem;
            }
            else if (nonEquipItem.itemId.Equals(itemId))
            {
                // If same item id, increase its amount
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
            // If there are no changes and there are an empty entries, fill them
            foreach (var emptySlot in emptySlots)
            {
                var value = emptySlot.Value;
                var newItem = new CharacterItem();
                newItem.id = System.Guid.NewGuid().ToString();
                newItem.itemId = itemId;
                newItem.level = level;
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

        // Apply all changes
        foreach (var change in changes)
        {
            nonEquipItems[change.Key] = change.Value;
        }

        // Add new items
        while (amount > 0)
        {
            var newItem = new CharacterItem();
            newItem.id = System.Guid.NewGuid().ToString();
            newItem.itemId = itemId;
            newItem.level = level;
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
            nonEquipItems.Add(newItem);
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

    public bool CanEquipItem(CharacterItem equippingItem, string equipPosition, out string reasonWhyCannot, out HashSet<string> shouldUnequipPositions)
    {
        reasonWhyCannot = "";
        shouldUnequipPositions = new HashSet<string>();

        var equipmentItem = equippingItem.GetEquipmentItem();
        if (equipmentItem == null)
        {
            reasonWhyCannot = "This item is not equipment item";
            return false;
        }

        if (string.IsNullOrEmpty(equipPosition))
        {
            reasonWhyCannot = "Invalid equip position";
            return false;
        }

        if (!equippingItem.CanEquip(this))
        {
            reasonWhyCannot = "Character level or attributes does not meet requirements";
            return false;
        }

        var weaponItem = equippingItem.GetWeaponItem();
        var shieldItem = equippingItem.GetShieldItem();
        var armorItem = equippingItem.GetArmorItem();

        var tempEquipWeapons = EquipWeapons;
        var rightHandWeapon = tempEquipWeapons.rightHand.GetWeaponItem();
        var leftHandWeapon = tempEquipWeapons.leftHand.GetWeaponItem();
        var leftHandShield = tempEquipWeapons.leftHand.GetShieldItem();

        WeaponItemEquipType rightHandEquipType;
        var hasRightHandItem = rightHandWeapon.TryGetWeaponItemEquipType(out rightHandEquipType);
        WeaponItemEquipType leftHandEquipType;
        var hasLeftHandItem = leftHandShield != null || leftHandWeapon.TryGetWeaponItemEquipType(out leftHandEquipType);

        if (weaponItem != null)
        {
            switch (weaponItem.EquipType)
            {
                case WeaponItemEquipType.OneHand:
                    // If weapon is one hand its equip position must be right hand
                    if (!equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
                    {
                        reasonWhyCannot = "Can equip to right hand only";
                        return false;
                    }
                    // One hand can equip with shield only 
                    // if there are weapons on left hand it should unequip
                    if (hasRightHandItem)
                        shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
                    if (hasLeftHandItem)
                        shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND);
                    break;
                case WeaponItemEquipType.OneHandCanDual:
                    // If weapon is one hand can dual its equip position must be right or left hand
                    if (!equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) &&
                        !equipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
                    {
                        reasonWhyCannot = "Can equip to right hand or left hand only";
                        return false;
                    }
                    // Unequip item if right hand weapon is one hand or two hand
                    if (hasRightHandItem)
                    {
                        if (rightHandEquipType == WeaponItemEquipType.OneHand ||
                            rightHandEquipType == WeaponItemEquipType.TwoHand)
                            shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
                    }
                    break;
                case WeaponItemEquipType.TwoHand:
                    // If weapon is one hand its equip position must be right hand
                    if (!equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
                    {
                        reasonWhyCannot = "Can equip to right hand or left hand only";
                        return false;
                    }
                    // Unequip both left and right hand
                    if (hasRightHandItem)
                        shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
                    if (hasLeftHandItem)
                        shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND);
                    break;
            }
        }

        if (shieldItem != null)
        {
            if (!equipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
            {
                reasonWhyCannot = "Can equip to left hand only";
                return false;
            }
            if (hasRightHandItem && rightHandEquipType == WeaponItemEquipType.TwoHand)
                shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
        }

        if (armorItem != null)
        {
            if (!equipPosition.Equals(armorItem.EquipPosition))
            {
                reasonWhyCannot = "Can equip to " + armorItem.EquipPosition + " only";
                return false;
            }
        }
        shouldUnequipPositions.Add(equipPosition);
        return true;
    }

    public virtual void ReceiveDamage(BaseCharacterEntity attacker,
        Dictionary<DamageElement, DamageAmount> allDamageAttributes,
        CharacterBuff debuff)
    {
        var calculatingTotalDamage = 0f;
        // Damage calculations apply at server only
        if (!IsServer || !CanReceiveDamageFrom(attacker) || CurrentHp <= 0)
            return;

        var gameInstance = GameInstance.Singleton;
        // Calculate chance to hit
        var hitChance = gameInstance.GameplayRule.GetHitChance(attacker, this);
        // If miss, return don't calculate damages
        if (Random.value > hitChance)
        {
            ReceivedDamage(attacker, CombatAmountTypes.Miss, 0);
            return;
        }
        // Calculate damages
        if (allDamageAttributes.Count > 0)
        {
            foreach (var allDamageAttribute in allDamageAttributes)
            {
                var damageElement = allDamageAttribute.Key;
                var damageAmount = allDamageAttribute.Value;
                var receivingDamage = damageElement.GetDamageReducedByResistance(this, Random.Range(damageAmount.minDamage, damageAmount.maxDamage));
                if (receivingDamage > 0f)
                    calculatingTotalDamage += receivingDamage;
            }
        }
        // Calculate chance to critical
        var criticalChance = gameInstance.GameplayRule.GetCriticalChance(attacker, this);
        var isCritical = Random.value <= criticalChance;
        // If critical occurs
        if (isCritical)
            calculatingTotalDamage = gameInstance.GameplayRule.GetCriticalDamage(attacker, this, calculatingTotalDamage);
        // Calculate chance to block
        var blockChance = gameInstance.GameplayRule.GetBlockChance(attacker, this);
        var isBlocked = Random.value <= blockChance;
        // If block occurs
        if (isBlocked)
            calculatingTotalDamage = gameInstance.GameplayRule.GetBlockDamage(attacker, this, calculatingTotalDamage);
        // Apply damages
        var totalDamage = (int)calculatingTotalDamage;
        CurrentHp -= totalDamage;

        if (isBlocked)
            ReceivedDamage(attacker, CombatAmountTypes.BlockedDamage, totalDamage);
        else if (isCritical)
            ReceivedDamage(attacker, CombatAmountTypes.CriticalDamage, totalDamage);
        else
            ReceivedDamage(attacker, CombatAmountTypes.NormalDamage, totalDamage);

        if (model != null)
        {
            var animator = model.CacheAnimator;
            animator.ResetTrigger(ANIM_HURT);
            animator.SetTrigger(ANIM_HURT);
        }

        // If current hp <= 0, character dead
        if (CurrentHp <= 0)
            Killed(attacker);
        else if (!debuff.IsEmpty())
        {
            var buffId = debuff.GetBuffId();
            var buffIndex = -1;
            if (buffIndexes.TryGetValue(buffId, out buffIndex))
            {
                buffs.RemoveAt(buffIndex);
                UpdateBuffIndexes();
            }
            var characterDebuff = debuff.Clone();
            characterDebuff.Added();
            buffs.Add(characterDebuff);
            buffIndexes.Add(buffId, buffs.Count - 1);
        }
    }
    #endregion

    #region Sync data changes callback
    /// <summary>
    /// Override this to do stuffs when id changes
    /// </summary>
    /// <param name="id"></param>
    protected virtual void OnIdChange(string id)
    {
        if (onIdChange != null)
            onIdChange.Invoke(id);
    }

    /// <summary>
    /// Override this to do stuffs when database Id changes
    /// </summary>
    /// <param name="databaseId"></param>
    protected virtual void OnDatabaseIdChange(string databaseId)
    {
        // Get database
        GameInstance.AllCharacterDatabases.TryGetValue(databaseId, out database);

        // Setup model
        if (model != null)
            Destroy(model.gameObject);

        model = this.InstantiateModel(CacheModelContainer);
        if (model != null)
        {
            SetupModel(model);
            model.SetEquipWeapons(equipWeapons);
            model.SetEquipItems(equipItems);
            model.gameObject.SetActive(!isHidding.Value);
        }

        if (onDatabaseIdChange != null)
            onDatabaseIdChange.Invoke(databaseId);
    }

    /// <summary>
    /// Override this to do stuffs when character name changes
    /// </summary>
    /// <param name="characterName"></param>
    protected virtual void OnCharacterNameChange(string characterName)
    {
        if (onCharacterNameChange != null)
            onCharacterNameChange.Invoke(characterName);
    }

    /// <summary>
    /// Override this to do stuffs when level changes
    /// </summary>
    /// <param name="level"></param>
    protected virtual void OnLevelChange(int level)
    {
        if (onLevelChange != null)
            onLevelChange.Invoke(level);
    }

    /// <summary>
    /// Override this to do stuffs when exp changes
    /// </summary>
    /// <param name="exp"></param>
    protected virtual void OnExpChange(int exp)
    {
        if (onExpChange != null)
            onExpChange.Invoke(exp);
    }

    /// <summary>
    /// Override this to do stuffs when current hp changes
    /// </summary>
    /// <param name="currentHp"></param>
    protected virtual void OnCurrentHpChange(int currentHp)
    {
        if (onCurrentHpChange != null)
            onCurrentHpChange.Invoke(currentHp);
    }

    /// <summary>
    /// Override this to do stuffs when current mp changes
    /// </summary>
    /// <param name="currentMp"></param>
    protected virtual void OnCurrentMpChange(int currentMp)
    {
        if (onCurrentMpChange != null)
            onCurrentMpChange.Invoke(currentMp);
    }

    /// <summary>
    /// Override this to do stuffs when equip weapons changes
    /// </summary>
    /// <param name="equipWeapons"></param>
    protected virtual void OnEquipWeaponsChange(EquipWeapons equipWeapons)
    {
        if (model != null)
            model.SetEquipWeapons(equipWeapons);

        if (onEquipWeaponsChange != null)
            onEquipWeaponsChange.Invoke(equipWeapons);
    }

    /// <summary>
    /// Overrride this to do stuffs when doing action state changes
    /// </summary>
    /// <param name="doingAction"></param>
    protected virtual void OnIsDoingActionChange(bool doingAction)
    {
        if (onIsDoingActionChange != null)
            onIsDoingActionChange.Invoke(doingAction);
    }

    /// <summary>
    /// Override this to do stuffs when hidding state changes
    /// </summary>
    /// <param name="isHidding"></param>
    protected virtual void OnIsHiddingChange(bool isHidding)
    {
        if (model != null)
            model.gameObject.SetActive(!isHidding);

        if (onIsHiddingChange != null)
            onIsHiddingChange.Invoke(isHidding);
    }
    #endregion

    #region Net functions operation callback
    /// <summary>
    /// Override this to do stuffs when attributes changes
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="index"></param>
    protected virtual void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (IsServer)
            shouldRecaches = true;

        if (onAttributesOperation != null)
            onAttributesOperation.Invoke(operation, index);
    }

    /// <summary>
    /// Override this to do stuffs when skills changes
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="index"></param>
    protected virtual void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (IsServer)
            shouldRecaches = true;

        if (onSkillsOperation != null)
            onSkillsOperation.Invoke(operation, index);
    }

    /// <summary>
    /// Override this to do stuffs when buffs changes
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="index"></param>
    protected virtual void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (IsServer)
            shouldRecaches = true;

        if (model != null)
            model.SetBuffs(buffs);

        if (onBuffsOperation != null)
            onBuffsOperation.Invoke(operation, index);
    }

    /// <summary>
    /// Override this to do stuffs when equip items changes
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="index"></param>
    protected virtual void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (IsServer)
            shouldRecaches = true;

        if (model != null)
            model.SetEquipItems(equipItems);

        if (onEquipItemsOperation != null)
            onEquipItemsOperation.Invoke(operation, index);
    }

    /// <summary>
    /// Override this to do stuffs when non equip items changes
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="index"></param>
    protected virtual void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (IsServer)
            shouldRecaches = true;

        if (onNonEquipItemsOperation != null)
            onNonEquipItemsOperation.Invoke(operation, index);
    }
    #endregion

    #region Keys indexes update functions
    protected void UpdateBuffIndexes()
    {
        buffIndexes.Clear();
        for (var i = 0; i < buffs.Count; ++i)
        {
            var entry = buffs[i];
            var buffId = CharacterBuff.GetBuffId(Id, entry.skillId, entry.isDebuff);
            if (!buffIndexes.ContainsKey(buffId))
                buffIndexes.Add(buffId, i);
        }
    }

    protected void UpdateEquipItemIndexes()
    {
        equipItemIndexes.Clear();
        for (var i = 0; i < equipItems.Count; ++i)
        {
            var entry = equipItems[i];
            var armorItem = entry.GetArmorItem();
            if (entry.IsValid() && armorItem != null && !equipItemIndexes.ContainsKey(armorItem.EquipPosition))
                equipItemIndexes.Add(armorItem.EquipPosition, i);
        }
    }
    #endregion

    #region Target Entity Getter/Setter
    public void SetTargetEntity(RpgNetworkEntity entity)
    {
        targetEntity = entity;
        if (!IsServer)
            RequestSetTargetEntity(entity == null ? 0 : entity.ObjectId);
    }

    public bool TryGetTargetEntity<T>(out T entity) where T : RpgNetworkEntity
    {
        entity = null;
        if (targetEntity == null)
            return false;
        entity = targetEntity as T;
        return entity != null;
    }
    #endregion

    #region Weapons / Damage
    public CharacterItem GetRandomedWeapon(out bool isLeftHand)
    {
        isLeftHand = false;
        // Start with default weapon, if character not equip any weapons, will return this
        var resultWeapon = CharacterItem.Create(GameInstance.Singleton.defaultWeaponItem, 1);
        // Find right hand and left and to set result weapon
        var rightHand = EquipWeapons.rightHand;
        var leftHand = EquipWeapons.leftHand;
        var rightWeaponItem = rightHand.GetWeaponItem();
        var leftWeaponItem = leftHand.GetWeaponItem();
        if (rightWeaponItem != null && leftWeaponItem != null)
        {
            // Random right hand or left hand weapon
            isLeftHand = Random.Range(0, 1) == 1;
            resultWeapon = !isLeftHand ? rightHand : leftHand;
        }
        else if (rightWeaponItem != null)
        {
            resultWeapon = rightHand;
            isLeftHand = false;
        }
        else if (leftWeaponItem != null)
        {
            resultWeapon = leftHand;
            isLeftHand = true;
        }
        return resultWeapon;
    }

    public virtual void GetAttackData(
        float inflictRate,
        Dictionary<DamageElement, DamageAmount> additionalDamageAttributes,
        out int actionId,
        out float damageDuration,
        out float totalDuration,
        out DamageInfo damageInfo,
        out Dictionary<DamageElement, DamageAmount> allDamageAttributes)
    {
        // Initialize data
        actionId = -1;
        damageDuration = 0f;
        totalDuration = 0f;
        damageInfo = null;
        allDamageAttributes = new Dictionary<DamageElement, DamageAmount>();
        // Prepare weapon data
        var isLeftHand = false;
        var equipWeapon = GetRandomedWeapon(out isLeftHand);
        var weapon = equipWeapon.GetWeaponItem();
        var weaponType = weapon.WeaponType;
        // Assign damage data
        damageInfo = weaponType.damageInfo;
        // Random animation
        var animArray = !isLeftHand ? weaponType.rightHandAttackAnimations : weaponType.leftHandAttackAnimations;
        var animLength = animArray.Length;
        if (animLength > 0)
        {
            var anim = animArray[Random.Range(0, animLength)];
            // Assign animation data
            actionId = anim.Id;
            damageDuration = anim.TriggerDuration / AttackSpeed;
            totalDuration = (anim.ClipLength + anim.extraDuration) / AttackSpeed;
        }
        // Calculate all damages
        var effectiveness = weapon.GetEffectivenessDamage(this);
        var damageAttribute = weapon.GetDamageAttribute(equipWeapon.level, effectiveness, inflictRate);
        allDamageAttributes = weapon.GetIncreaseDamageAttributes(equipWeapon.level);
        allDamageAttributes = GameDataHelpers.CombineDamageAttributesDictionary(allDamageAttributes, damageAttribute);
        allDamageAttributes = GameDataHelpers.CombineDamageAttributesDictionary(allDamageAttributes, additionalDamageAttributes);
    }

    public virtual float GetAttackDistance()
    {
        // Finding minimum distance of equipped weapons
        // For example, if right hand attack distance is 1m and left hand attack distance is 0.7m
        // it will return 0.7m. if no equipped weapons, it will return default weapon attack distance
        float minDistance = float.MaxValue;
        DamageInfo tempDamageInfo;
        float tempDistance = 0f;
        var rightHand = EquipWeapons.rightHand;
        var leftHand = EquipWeapons.leftHand;
        var rightHandWeapon = rightHand.GetWeaponItem();
        var leftHandWeapon = leftHand.GetWeaponItem();
        if (rightHandWeapon != null)
        {
            tempDamageInfo = rightHandWeapon.WeaponType.damageInfo;
            tempDistance = tempDamageInfo.GetDistance();
            if (minDistance > tempDistance)
                minDistance = tempDistance;
        }
        if (leftHandWeapon != null)
        {
            tempDamageInfo = leftHandWeapon.WeaponType.damageInfo;
            tempDistance = tempDamageInfo.GetDistance();
            if (minDistance > tempDistance)
                minDistance = tempDistance;
        }
        if (rightHandWeapon == null && leftHandWeapon == null)
        {
            tempDamageInfo = GameInstance.Singleton.DefaultWeaponType.damageInfo;
            tempDistance = tempDamageInfo.GetDistance();
            minDistance = tempDistance;
        }
        return minDistance;
    }

    public virtual void LaunchDamageEntity(
        Vector3 position,
        DamageInfo damageInfo,
        Dictionary<DamageElement, DamageAmount> allDamageAttributes,
        CharacterBuff debuff)
    {
        if (!IsServer)
            return;

        Transform damageTransform = GetDamageTransform(damageInfo.damageType);
        switch (damageInfo.damageType)
        {
            case DamageType.Melee:
                var halfFov = damageInfo.hitFov * 0.5f;
                var hits = Physics.OverlapSphere(damageTransform.position, damageInfo.hitDistance);
                foreach (var hit in hits)
                {
                    var characterEntity = hit.GetComponent<BaseCharacterEntity>();
                    if (characterEntity == null || characterEntity == this || characterEntity.CurrentHp <= 0)
                        continue;
                    var targetDir = (CacheTransform.position - characterEntity.CacheTransform.position).normalized;
                    var angle = Vector3.Angle(targetDir, CacheTransform.forward);
                    // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
                    if (angle < 180 + halfFov && angle > 180 - halfFov)
                        characterEntity.ReceiveDamage(this, allDamageAttributes, debuff);
                }
                break;
            case DamageType.Missile:
                if (damageInfo.missileDamageEntity != null)
                {
                    var missileDamageIdentity = Manager.Assets.NetworkSpawn(damageInfo.missileDamageEntity.Identity, damageTransform.position, damageTransform.rotation);
                    var missileDamageEntity = missileDamageIdentity.GetComponent<MissileDamageEntity>();
                    missileDamageEntity.SetupDamage(this, allDamageAttributes, debuff, damageInfo.missileDistance, damageInfo.missileSpeed);
                }
                break;
        }
    }
    #endregion

    protected virtual void SetupModel(CharacterModel characterModel)
    {
        CacheCapsuleCollider.center = characterModel.center;
        CacheCapsuleCollider.radius = characterModel.radius;
        CacheCapsuleCollider.height = characterModel.height;
    }

    protected virtual Transform GetDamageTransform(DamageType damageType)
    {
        if (model != null)
        {
            switch (damageType)
            {
                case DamageType.Melee:
                    return model.MeleeDamageTransform;
                case DamageType.Missile:
                    return model.MissileDamageTransform;
            }
        }
        return CacheTransform;
    }

    protected virtual void ReceivedDamage(BaseCharacterEntity attacker, CombatAmountTypes damageAmountType, int damage)
    {
        RequestCombatAmount(damageAmountType, damage);
    }

    protected virtual void Killed(BaseCharacterEntity lastAttacker)
    {
        StopAllCoroutines();
        isDoingAction.Value = false;
        buffs.Clear();
        var count = skills.Count;
        for (var i = 0; i < count; ++i)
        {
            var skill = skills[i];
            skill.coolDownRemainsDuration = 0;
            skills.Dirty(i);
        }
        // Send OnDead to owner player only
        RequestOnDead(false);
    }

    protected virtual void Respawn()
    {
        if (!IsServer || CurrentHp > 0)
            return;
        CurrentHp = CacheMaxHp;
        CurrentMp = CacheMaxMp;
        // Send OnRespawn to owner player only
        RequestOnRespawn(false);
    }

    protected virtual void MakeCaches()
    {
        if (!shouldRecaches)
            return;
        CacheStats = this.GetStats();
        CacheAttributes = this.GetAttributes();
        CacheResistances = this.GetResistances();
        CacheMaxHp = (int)CacheStats.hp;
        CacheMaxMp = (int)CacheStats.mp;
        shouldRecaches = false;
    }

    internal virtual void IncreaseExp(int exp)
    {
        if (!IsServer)
            return;
        var gameInstance = GameInstance.Singleton;
        if (!gameInstance.GameplayRule.IncreaseExp(this, exp))
            return;
        // Send OnLevelUp to owner player only
        RequestOnLevelUp();
    }

    protected abstract bool CanReceiveDamageFrom(BaseCharacterEntity characterEntity);
    protected abstract bool IsAlly(BaseCharacterEntity characterEntity);
    protected abstract bool IsEnemy(BaseCharacterEntity characterEntity);
}
