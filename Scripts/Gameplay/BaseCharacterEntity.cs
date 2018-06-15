using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum AnimActionType : byte
{
    None,
    Generic,
    Attack,
    Skill,
}

[RequireComponent(typeof(CharacterAnimationSystem))]
[RequireComponent(typeof(CharacterRecoverySystem))]
[RequireComponent(typeof(CharacterSkillAndBuffSystem))]
[RequireComponent(typeof(CapsuleCollider))]
public abstract class BaseCharacterEntity : RpgNetworkEntity, ICharacterData
{
    public const float ACTION_COMMAND_DELAY = 0.2f;

    // Use id as primary key
    #region Sync data
    [Header("Sync Fields")]
    public SyncFieldString id = new SyncFieldString();
    public SyncFieldInt dataId = new SyncFieldInt();
    public SyncFieldString characterName = new SyncFieldString();
    public SyncFieldShort level = new SyncFieldShort();
    public SyncFieldInt exp = new SyncFieldInt();
    public SyncFieldInt currentHp = new SyncFieldInt();
    public SyncFieldInt currentMp = new SyncFieldInt();
    public SyncFieldInt currentStamina = new SyncFieldInt();
    public SyncFieldInt currentFood = new SyncFieldInt();
    public SyncFieldInt currentWater = new SyncFieldInt();
    public SyncFieldEquipWeapons equipWeapons = new SyncFieldEquipWeapons();
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
    protected BaseCharacter database;
    protected RpgNetworkEntity targetEntity;
    protected CharacterModel model;
    protected readonly Dictionary<string, int> equipItemIndexes = new Dictionary<string, int>();
    protected AnimActionType animActionType;
    protected float lastActionCommandReceivedTime;
    public bool isRecaching { get; protected set; }
    public bool isSprinting { get; protected set; }
    #endregion

    #region Caches Data
    public CharacterStats CacheStats { get; protected set; }
    public Dictionary<Attribute, short> CacheAttributes { get; protected set; }
    public Dictionary<Skill, short> CacheSkills { get; protected set; }
    public Dictionary<DamageElement, float> CacheResistances { get; protected set; }
    public Dictionary<DamageElement, MinMaxFloat> CacheIncreaseDamages { get; protected set; }
    public int CacheMaxHp { get; protected set; }
    public int CacheMaxMp { get; protected set; }
    public int CacheMaxStamina { get; protected set; }
    public int CacheMaxFood { get; protected set; }
    public int CacheMaxWater { get; protected set; }
    public float CacheBaseMoveSpeed { get; protected set; }
    public float CacheMoveSpeed { get; protected set; }
    public float CacheAtkSpeed { get; protected set; }
    #endregion

    #region Sync data actions
    public System.Action<string> onIdChange;
    public System.Action<int> onDataIdChange;
    public System.Action<string> onCharacterNameChange;
    public System.Action<short> onLevelChange;
    public System.Action<int> onExpChange;
    public System.Action<int> onCurrentHpChange;
    public System.Action<int> onCurrentMpChange;
    public System.Action<EquipWeapons> onEquipWeaponsChange;
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
    public virtual int DataId { get { return dataId; } set { dataId.Value = value; } }
    public virtual string CharacterName { get { return characterName; } set { characterName.Value = value; } }
    public virtual short Level { get { return level.Value; } set { level.Value = value; } }
    public virtual int Exp { get { return exp.Value; } set { exp.Value = value; } }
    public virtual int CurrentHp { get { return currentHp.Value; } set { currentHp.Value = value; } }
    public virtual int CurrentMp { get { return currentMp.Value; } set { currentMp.Value = value; } }
    public virtual int CurrentStamina { get { return currentStamina.Value; } set { currentStamina.Value = value; } }
    public virtual int CurrentFood { get { return currentFood.Value; } set { currentFood.Value = value; } }
    public virtual int CurrentWater { get { return currentWater.Value; } set { currentWater.Value = value; } }
    public virtual EquipWeapons EquipWeapons { get { return equipWeapons; } set { equipWeapons.Value = value; } }
    public override string Title { get { return CharacterName; } }

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

    public Animator ModelAnimator { get { return model == null ? null : model.CacheAnimator; } }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        var gameInstance = GameInstance.Singleton;
        gameObject.layer = gameInstance.characterLayer;
        animActionType = AnimActionType.None;
        isRecaching = true;
    }

    protected override void Start()
    {
        base.Start();
        foreach (var ownerObject in ownerObjects)
        {
            ownerObject.SetActive(IsOwnerClient);
        }
        foreach (var nonOwnerObject in nonOwnerObjects)
        {
            nonOwnerObject.SetActive(!IsOwnerClient);
        }
    }

    protected override void Update()
    {
        base.Update();

        MakeCaches();
    }

    public virtual void ValidateRecovery()
    {
        if (!IsServer)
            return;

        // Validate Hp
        if (CurrentHp < 0)
            CurrentHp = 0;
        if (CurrentHp > CacheMaxHp)
            CurrentHp = CacheMaxHp;
        // Validate Mp
        if (CurrentMp < 0)
            CurrentMp = 0;
        if (CurrentMp > CacheMaxMp)
            CurrentMp = CacheMaxMp;
        // Validate Stamina
        if (CurrentStamina < 0)
            CurrentStamina = 0;
        if (CurrentStamina > CacheMaxStamina)
            CurrentStamina = CacheMaxStamina;
        // Validate Food
        if (CurrentFood < 0)
            CurrentFood = 0;
        if (CurrentFood > CacheMaxFood)
            CurrentFood = CacheMaxFood;
        // Validate Water
        if (CurrentWater < 0)
            CurrentWater = 0;
        if (CurrentWater > CacheMaxWater)
            CurrentWater = CacheMaxWater;

        if (CurrentHp <= 0)
            Killed(null);
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
        dataId.sendOptions = SendOptions.ReliableOrdered;
        dataId.forOwnerOnly = false;
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
        dataId.onChange += OnDataIdChange;
        characterName.onChange += OnCharacterNameChange;
        level.onChange += OnLevelChange;
        exp.onChange += OnExpChange;
        currentHp.onChange += OnCurrentHpChange;
        currentMp.onChange += OnCurrentMpChange;
        equipWeapons.onChange += OnEquipWeaponsChange;
        isHidding.onChange += OnIsHiddingChange;
        // On list changes events
        attributes.onOperation += OnAttributesOperation;
        skills.onOperation += OnSkillsOperation;
        buffs.onOperation += OnBuffsOperation;
        equipItems.onOperation += OnEquipItemsOperation;
        nonEquipItems.onOperation += OnNonEquipItemsOperation;
        // Register Network functions
        RegisterNetFunction("Attack", new LiteNetLibFunction(() => NetFuncAttack()));
        RegisterNetFunction("UseSkill", new LiteNetLibFunction<NetFieldVector3, NetFieldInt>((position, skillIndex) => NetFuncUseSkill(position, skillIndex)));
        RegisterNetFunction("UseItem", new LiteNetLibFunction<NetFieldInt>((itemIndex) => NetFuncUseItem(itemIndex)));
        RegisterNetFunction("PlayActionAnimation", new LiteNetLibFunction<NetFieldInt, NetFieldByte>((actionId, animActionType) => NetFuncPlayActionAnimation(actionId, (AnimActionType)animActionType.Value)));
        RegisterNetFunction("PlayEffect", new LiteNetLibFunction<NetFieldInt>((effectId) => NetFuncPlayEffect(effectId)));
        RegisterNetFunction("PickupItem", new LiteNetLibFunction<NetFieldUInt>((objectId) => NetFuncPickupItem(objectId)));
        RegisterNetFunction("DropItem", new LiteNetLibFunction<NetFieldInt, NetFieldShort>((index, amount) => NetFuncDropItem(index, amount)));
        RegisterNetFunction("EquipItem", new LiteNetLibFunction<NetFieldInt, NetFieldString>((nonEquipIndex, equipPosition) => NetFuncEquipItem(nonEquipIndex, equipPosition)));
        RegisterNetFunction("UnEquipItem", new LiteNetLibFunction<NetFieldString>((fromEquipPosition) => NetFuncUnEquipItem(fromEquipPosition)));
        RegisterNetFunction("CombatAmount", new LiteNetLibFunction<NetFieldByte, NetFieldInt>((combatAmountType, amount) => NetFuncCombatAmount((CombatAmountType)combatAmountType.Value, amount)));
        RegisterNetFunction("OnDead", new LiteNetLibFunction<NetFieldBool>((isInitialize) => NetFuncOnDead(isInitialize)));
        RegisterNetFunction("OnRespawn", new LiteNetLibFunction<NetFieldBool>((isInitialize) => NetFuncOnRespawn(isInitialize)));
        RegisterNetFunction("OnLevelUp", new LiteNetLibFunction(() => NetFuncOnLevelUp()));
    }

    protected virtual void OnDestroy()
    {
        // On data changes events
        dataId.onChange -= OnDataIdChange;
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
    protected void NetFuncAttack()
    {
        if (Time.unscaledTime - lastActionCommandReceivedTime < ACTION_COMMAND_DELAY)
            return;
        lastActionCommandReceivedTime = Time.unscaledTime;

        if (CurrentHp <= 0 || IsPlayingActionAnimation())
            return;

        // Prepare requires data
        Item weapon;
        int actionId;
        float triggerDuration;
        float totalDuration;
        DamageInfo damageInfo;
        Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

        GetAttackingData(
            out weapon,
            out actionId,
            out triggerDuration,
            out totalDuration,
            out damageInfo,
            out allDamageAmounts);

        // Reduce ammo amount
        if (weapon != null && weapon.WeaponType.requireAmmoType != null)
        {
            Dictionary<CharacterItem, short> decreaseItems;
            if (!this.DecreaseAmmos(weapon.WeaponType.requireAmmoType, 1, out decreaseItems))
                return;
            var firstEntry = decreaseItems.FirstOrDefault();
            if (firstEntry.Key.GetItem() != null && firstEntry.Value > 0)
                allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(allDamageAmounts, firstEntry.Key.GetItem().GetIncreaseDamages(firstEntry.Key.level));
        }

        // Play animation on clients
        RequestPlayActionAnimation(actionId, AnimActionType.Attack);
        // Start attack routine
        StartCoroutine(AttackRoutine(CacheTransform.position, triggerDuration, totalDuration, damageInfo, allDamageAmounts));
    }

    IEnumerator AttackRoutine(
        Vector3 position,
        float triggerDuration,
        float totalDuration,
        DamageInfo damageInfo,
        Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
    {
        yield return new WaitForSecondsRealtime(triggerDuration);
        LaunchDamageEntity(position, damageInfo, allDamageAmounts, CharacterBuff.Empty, -1);
        yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
    }

    /// <summary>
    /// Is function will be called at server to order character to use skill
    /// </summary>
    /// <param name="position">Target position to apply skill at</param>
    /// <param name="skillIndex">Index in `characterSkills` list which will be used</param>
    protected void NetFuncUseSkill(Vector3 position, int skillIndex)
    {
        if (Time.unscaledTime - lastActionCommandReceivedTime < ACTION_COMMAND_DELAY)
            return;
        lastActionCommandReceivedTime = Time.unscaledTime;

        if (CurrentHp <= 0 ||
            IsPlayingActionAnimation() ||
            skillIndex < 0 ||
            skillIndex >= skills.Count)
            return;

        var characterSkill = skills[skillIndex];
        if (!characterSkill.CanUse(this))
            return;

        // Prepare requires data
        Item weapon;
        int actionId;
        float triggerDuration;
        float totalDuration;
        bool isAttack;
        DamageInfo damageInfo;
        Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

        GetUsingSkillData(
            characterSkill,
            out weapon,
            out actionId,
            out triggerDuration,
            out totalDuration,
            out isAttack,
            out damageInfo,
            out allDamageAmounts);

        // Reduce ammo amount
        if (characterSkill.GetSkill().IsAttack() && weapon != null && weapon.WeaponType.requireAmmoType != null)
        {
            Dictionary<CharacterItem, short> decreaseItems;
            if (!this.DecreaseAmmos(weapon.WeaponType.requireAmmoType, 1, out decreaseItems))
                return;
            var firstEntry = decreaseItems.FirstOrDefault();
            if (firstEntry.Key.GetItem() != null && firstEntry.Value > 0)
                allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(allDamageAmounts, firstEntry.Key.GetItem().GetIncreaseDamages(firstEntry.Key.level));
        }

        // Play animation on clients
        RequestPlayActionAnimation(actionId, AnimActionType.Skill);
        // Start use skill routine
        StartCoroutine(UseSkillRoutine(skillIndex, position, triggerDuration, totalDuration, isAttack, damageInfo, allDamageAmounts));
    }

    IEnumerator UseSkillRoutine(
        int skillIndex,
        Vector3 position,
        float triggerDuration,
        float totalDuration,
        bool isAttack,
        DamageInfo damageInfo,
        Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
    {
        // Update skill states
        var characterSkill = skills[skillIndex];
        characterSkill.Used();
        characterSkill.ReduceMp(this);
        skills[skillIndex] = characterSkill;
        yield return new WaitForSecondsRealtime(triggerDuration);
        var skill = characterSkill.GetSkill();
        switch (skill.skillType)
        {
            case SkillType.Active:
                ApplySkillBuff(characterSkill);
                if (isAttack)
                {
                    CharacterBuff debuff = CharacterBuff.Empty;
                    if (skill.isDebuff)
                        debuff = CharacterBuff.Create(Id, BuffType.SkillDebuff, skill.HashId, characterSkill.level);
                    LaunchDamageEntity(position, damageInfo, allDamageAmounts, debuff, skill.hitEffects.Id);
                }
                break;
            case SkillType.CraftItem:
                if (skill.CanCraft(this))
                {
                    var craftRequirements = skill.craftRequirements;
                    foreach (var craftRequirement in craftRequirements)
                    {
                        if (craftRequirement.item != null && craftRequirement.amount > 0)
                            this.DecreaseItems(craftRequirement.item.HashId, craftRequirement.amount);
                    }
                    this.IncreaseItems(skill.craftingItem.HashId, 1, 1);
                }
                break;
        }
        yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
    }
    
    /// <summary>
    /// This will be called on server to use item
    /// </summary>
    /// <param name="itemIndex"></param>
    protected void NetFuncUseItem(int itemIndex)
    {
        if (CurrentHp <= 0 ||
            itemIndex < 0 ||
            itemIndex > nonEquipItems.Count)
            return;

        var item = nonEquipItems[itemIndex];
        var potionItem = item.GetPotionItem();
        if (potionItem != null && this.DecreaseItemsByIndex(itemIndex, 1))
            ApplyPotionBuff(item);
    }

    /// <summary>
    /// This will be called at every clients to play any action animation
    /// </summary>
    /// <param name="actionId"></param>
    /// <param name="animActionType"></param>
    protected void NetFuncPlayActionAnimation(int actionId, AnimActionType animActionType)
    {
        if (CurrentHp <= 0)
            return;
        this.animActionType = animActionType;
        StartCoroutine(PlayActionAnimationRoutine(actionId, animActionType));
    }

    IEnumerator PlayActionAnimationRoutine(int actionId, AnimActionType animActionType)
    {
        Animator animator = model == null ? null : model.CacheAnimator;
        // If animator is not null, play the action animation
        ActionAnimation actionAnimation;
        if (animator != null && GameInstance.ActionAnimations.TryGetValue(actionId, out actionAnimation) && actionAnimation.clip != null)
        {
            animator.SetBool(CharacterAnimationSystem.ANIM_DO_ACTION, false);
            model.ChangeActionClip(actionAnimation.clip);
            var playSpeedMultiplier = 1f;
            switch (animActionType)
            {
                case AnimActionType.Attack:
                    playSpeedMultiplier = CacheAtkSpeed;
                    break;
            }
            AudioClip soundEffect;
            if (actionAnimation.TryGetRandomAudioClip(out soundEffect))
                AudioSource.PlayClipAtPoint(soundEffect, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
            animator.SetFloat(CharacterAnimationSystem.ANIM_ACTION_CLIP_MULTIPLIER, playSpeedMultiplier);
            animator.SetBool(CharacterAnimationSystem.ANIM_DO_ACTION, true);
            // Waits by current transition + clip duration before end animation
            yield return new WaitForSecondsRealtime(animator.GetAnimatorTransitionInfo(0).duration + (actionAnimation.ClipLength / playSpeedMultiplier));
            animator.SetBool(CharacterAnimationSystem.ANIM_DO_ACTION, false);
            // Waits by current transition + extra duration before end playing animation state
            yield return new WaitForSecondsRealtime(animator.GetAnimatorTransitionInfo(0).duration + (actionAnimation.extraDuration / playSpeedMultiplier));
        }
        this.animActionType = AnimActionType.None;
    }

    /// <summary>
    /// This will be called at every clients to play any effect
    /// </summary>
    /// <param name="effectId"></param>
    protected void NetFuncPlayEffect(int effectId)
    {
        GameEffectCollection gameEffectCollection;
        if (model == null || !GameInstance.GameEffectCollections.TryGetValue(effectId, out gameEffectCollection))
            return;
        model.InstantiateEffect(gameEffectCollection.effects);
    }

    /// <summary>
    /// This will be called at server to order character to pickup items
    /// </summary>
    /// <param name="objectId"></param>
    protected void NetFuncPickupItem(uint objectId)
    {
        if (CurrentHp <= 0 || IsPlayingActionAnimation())
            return;
        
        LiteNetLibIdentity entity;
        if (!Manager.Assets.TryGetSpawnedObject(objectId, out entity))
            return;

        var itemDropEntity = entity.GetComponent<ItemDropEntity>();
        if (itemDropEntity == null)
            return;

        if (Vector3.Distance(CacheTransform.position, itemDropEntity.CacheTransform.position) > GameInstance.Singleton.pickUpItemDistance + 5f)
            return;

        var itemDropData = itemDropEntity.dropData;
        if (!itemDropData.IsValid())
        {
            // Destroy item drop entity without item add because this is not valid
            itemDropEntity.NetworkDestroy();
            return;
        }
        var itemDataId = itemDropData.dataId;
        var level = itemDropData.level;
        var amount = itemDropData.amount;
        if (!IncreasingItemsWillOverwhelming(itemDataId, level, amount) && this.IncreaseItems(itemDataId, level, amount))
            itemDropEntity.NetworkDestroy();
    }

    /// <summary>
    /// This will be called at server to order character to drop items
    /// </summary>
    /// <param name="index"></param>
    /// <param name="amount"></param>
    protected void NetFuncDropItem(int index, short amount)
    {
        var gameInstance = GameInstance.Singleton;
        if (CurrentHp <= 0 ||
            IsPlayingActionAnimation() ||
            index < 0 ||
            index > nonEquipItems.Count)
            return;

        var nonEquipItem = nonEquipItems[index];
        if (!nonEquipItem.IsValid() || amount > nonEquipItem.amount)
            return;

        var itemDataId = nonEquipItem.dataId;
        var level = nonEquipItem.level;
        if (this.DecreaseItemsByIndex(index, amount))
            ItemDropEntity.DropItem(this, itemDataId, level, amount);
    }

    /// <summary>
    /// This will be called at server to order character to equip equipments
    /// </summary>
    /// <param name="nonEquipIndex"></param>
    /// <param name="equipPosition"></param>
    protected void NetFuncEquipItem(int nonEquipIndex, string equipPosition)
    {
        if (CurrentHp <= 0 ||
            IsPlayingActionAnimation() ||
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
        if (CurrentHp <= 0 || IsPlayingActionAnimation())
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
    /// <param name="combatAmountType"></param>
    /// <param name="amount"></param>
    protected void NetFuncCombatAmount(CombatAmountType combatAmountType, int amount)
    {
        var uiSceneGameplay = UISceneGameplay.Singleton;
        if (uiSceneGameplay == null)
            return;

        var combatTextTransform = CacheTransform;
        if (model != null)
            combatTextTransform = model.CombatTextTransform;

        uiSceneGameplay.SpawnCombatText(combatTextTransform, combatAmountType, amount);
    }

    protected void NetFuncOnDead(bool isInitialize)
    {
        animActionType = AnimActionType.None;
        if (onDead != null)
            onDead.Invoke(isInitialize);
    }

    protected void NetFuncOnRespawn(bool isInitialize)
    {
        animActionType = AnimActionType.None;
        if (onRespawn != null)
            onRespawn.Invoke(isInitialize);
    }

    protected void NetFuncOnLevelUp()
    {
        var gameInstance = GameInstance.Singleton;
        if (gameInstance != null && gameInstance.levelUpEffect != null && model != null)
            model.InstantiateEffect(new GameEffect[] { gameInstance.levelUpEffect });
        if (onLevelUp != null)
            onLevelUp.Invoke();
    }
    #endregion

    #region Net functions callers
    public virtual void RequestAttack()
    {
        if (CurrentHp <= 0 || IsPlayingActionAnimation())
            return;
        CallNetFunction("Attack", FunctionReceivers.Server);
    }

    public virtual void RequestUseSkill(Vector3 position, int skillIndex)
    {
        if (CurrentHp <= 0 || IsPlayingActionAnimation() || skillIndex < 0 || skillIndex >= skills.Count || !skills[skillIndex].CanUse(this))
            return;
        CallNetFunction("UseSkill", FunctionReceivers.Server, position, skillIndex);
    }

    public virtual void RequestUseItem(int itemIndex)
    {
        if (CurrentHp <= 0)
            return;
        CallNetFunction("UseItem", FunctionReceivers.Server, itemIndex);
    }

    public virtual void RequestPlayActionAnimation(int actionId, AnimActionType animActionType)
    {
        if (CurrentHp <= 0 || actionId < 0)
            return;
        CallNetFunction("PlayActionAnimation", FunctionReceivers.All, actionId, animActionType);
    }

    public virtual void RequestPlayEffect(int effectId)
    {
        if (effectId < 0)
            return;
        CallNetFunction("PlayEffect", FunctionReceivers.All, effectId);
    }

    public virtual void RequestPickupItem(uint objectId)
    {
        if (CurrentHp <= 0 || IsPlayingActionAnimation())
            return;
        CallNetFunction("PickupItem", FunctionReceivers.Server, objectId);
    }

    public virtual void RequestDropItem(int index, short amount)
    {
        if (CurrentHp <= 0 || IsPlayingActionAnimation())
            return;
        CallNetFunction("DropItem", FunctionReceivers.Server, index, amount);
    }

    public virtual void RequestEquipItem(int nonEquipIndex, string equipPosition)
    {
        if (CurrentHp <= 0 || IsPlayingActionAnimation())
            return;
        CallNetFunction("EquipItem", FunctionReceivers.Server, nonEquipIndex, equipPosition);
    }

    public virtual void RequestUnEquipItem(string equipPosition)
    {
        if (CurrentHp <= 0 || IsPlayingActionAnimation())
            return;
        CallNetFunction("UnEquipItem", FunctionReceivers.Server, equipPosition);
    }

    public virtual void RequestCombatAmount(CombatAmountType combatAmountType, int amount)
    {
        CallNetFunction("CombatAmount", FunctionReceivers.All, combatAmountType, amount);
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
    public bool IncreasingItemsWillOverwhelming(int itemDataId, short level, short amount)
    {
        Item itemData;
        // If item not valid
        if (amount <= 0 || !GameInstance.Items.TryGetValue(itemDataId, out itemData))
            return false;

        var weight = itemData.weight;
        // If overwhelming
        if (this.GetTotalItemWeight() + (amount * weight) > CacheStats.weightLimit)
            return true;

        return false;
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

    public virtual void ReceiveDamage(BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, int hitEffectsId)
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
            ReceivedDamage(attacker, CombatAmountType.Miss, 0);
            return;
        }
        // Calculate damages
        if (allDamageAmounts.Count > 0)
        {
            foreach (var allDamageAmount in allDamageAmounts)
            {
                var damageElement = allDamageAmount.Key;
                var damageAmount = allDamageAmount.Value;
                if (hitEffectsId < 0 && damageElement != gameInstance.DefaultDamageElement)
                    hitEffectsId = damageElement.hitEffects.Id;
                var receivingDamage = damageElement.GetDamageReducedByResistance(this, Random.Range(damageAmount.min, damageAmount.max));
                if (receivingDamage > 0f)
                    calculatingTotalDamage += receivingDamage;
            }
        }
        if (hitEffectsId < 0)
            hitEffectsId = gameInstance.defaultHitEffects.Id;
        if (hitEffectsId >= 0)
            RequestPlayEffect(hitEffectsId);
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
            ReceivedDamage(attacker, CombatAmountType.BlockedDamage, totalDamage);
        else if (isCritical)
            ReceivedDamage(attacker, CombatAmountType.CriticalDamage, totalDamage);
        else
            ReceivedDamage(attacker, CombatAmountType.NormalDamage, totalDamage);

        if (model != null)
        {
            var animator = model.CacheAnimator;
            animator.ResetTrigger(CharacterAnimationSystem.ANIM_HURT);
            animator.SetTrigger(CharacterAnimationSystem.ANIM_HURT);
        }

        // If current hp <= 0, character dead
        if (CurrentHp <= 0)
            Killed(attacker);
        else if (!debuff.IsEmpty())
            ApplyBuff(debuff.characterId, debuff.dataId, debuff.type, debuff.level);
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
    /// Override this to do stuffs when data Id changes
    /// </summary>
    /// <param name="dataId"></param>
    protected virtual void OnDataIdChange(int dataId)
    {
        isRecaching = true;

        // Get database
        GameInstance.AllCharacters.TryGetValue(dataId, out database);

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

        if (onDataIdChange != null)
            onDataIdChange.Invoke(dataId);
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
    protected virtual void OnLevelChange(short level)
    {
        isRecaching = true;

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
        isRecaching = true;

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
        isRecaching = true;

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
        isRecaching = true;

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
        isRecaching = true;

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
        isRecaching = true;

        if (onNonEquipItemsOperation != null)
            onNonEquipItemsOperation.Invoke(operation, index);
    }
    #endregion

    #region Keys indexes update functions
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

    #region Buffs / Weapons / Damage
    protected void ApplyBuff(string characterId, int dataId, BuffType type, short level)
    {
        if (CurrentHp <= 0 || !IsServer)
            return;

        var buffIndex = this.IndexOfBuff(characterId, dataId, type);
        if (buffIndex >= 0)
            buffs.RemoveAt(buffIndex);

        var newBuff = CharacterBuff.Create(characterId, type, dataId, level);
        newBuff.Added();
        buffs.Add(newBuff);

        var duration = newBuff.GetDuration();
        var recoveryHp = duration <= 0f ? newBuff.GetBuffRecoveryHp() : 0;
        if (recoveryHp != 0)
        {
            CurrentHp += recoveryHp;
            RequestCombatAmount(CombatAmountType.HpRecovery, recoveryHp);
        }
        var recoveryMp = duration <= 0f ? newBuff.GetBuffRecoveryMp() : 0;
        if (recoveryMp != 0)
        {
            CurrentMp += recoveryMp;
            RequestCombatAmount(CombatAmountType.HpRecovery, recoveryMp);
        }
        var recoveryStamina = duration <= 0f ? newBuff.GetBuffRecoveryStamina() : 0;
        if (recoveryStamina != 0)
        {
            CurrentStamina += recoveryStamina;
            RequestCombatAmount(CombatAmountType.HpRecovery, recoveryStamina);
        }
        var recoveryFood = duration <= 0f ? newBuff.GetBuffRecoveryFood() : 0;
        if (recoveryFood != 0)
        {
            CurrentFood += recoveryFood;
            RequestCombatAmount(CombatAmountType.FoodRecovery, recoveryFood);
        }
        var recoveryWater = duration <= 0f ? newBuff.GetBuffRecoveryWater() : 0;
        if (recoveryWater != 0)
        {
            CurrentWater += recoveryWater;
            RequestCombatAmount(CombatAmountType.WaterRecovery, recoveryWater);
        }
        ValidateRecovery();
    }

    protected void ApplyPotionBuff(CharacterItem characterItem)
    {
        var item = characterItem.GetPotionItem();
        if (item == null)
            return;
        ApplyBuff(Id, item.HashId, BuffType.PotionBuff, characterItem.level);
    }

    protected void ApplySkillBuff(CharacterSkill characterSkill)
    {
        var skill = characterSkill.GetSkill();
        if (skill == null)
            return;
        if (skill.skillBuffType == SkillBuffType.BuffToUser)
            ApplyBuff(Id, skill.HashId, BuffType.SkillBuff, characterSkill.level);
    }

    public virtual void GetAttackingData(
        out Item weapon,
        out int actionId,
        out float triggerDuration,
        out float totalDuration,
        out DamageInfo damageInfo,
        out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
    {
        // Initialize data
        weapon = null;
        actionId = -1;
        triggerDuration = 0f;
        totalDuration = 0f;
        damageInfo = null;
        allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
        // Prepare weapon data
        var isLeftHand = false;
        var equipWeapon = this.GetRandomedWeapon(out isLeftHand);
        weapon = equipWeapon.GetWeaponItem();
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
            triggerDuration = anim.TriggerDuration / CacheAtkSpeed;
            totalDuration = (anim.ClipLength + anim.extraDuration) / CacheAtkSpeed;
        }
        // Calculate all damages
        allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
            allDamageAmounts,
            weapon.GetDamageAmount(equipWeapon.level, this));
        allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
            allDamageAmounts,
            CacheIncreaseDamages);
    }

    public virtual void GetUsingSkillData(
        CharacterSkill characterSkill,
        out Item weapon, 
        out int actionId,
        out float triggerDuration,
        out float totalDuration,
        out bool isAttack,
        out DamageInfo damageInfo,
        out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
    {
        // Initialize data
        weapon = null;
        isAttack = false;
        actionId = -1;
        triggerDuration = 0f;
        totalDuration = 0f;
        damageInfo = null;
        allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
        // Prepare skill data
        var skill = characterSkill.GetSkill();
        if (skill == null)
            return;
        isAttack = skill.IsAttack();
        // Prepare weapon data
        var isLeftHand = false;
        var equipWeapon = this.GetRandomedWeapon(out isLeftHand);
        weapon = equipWeapon.GetWeaponItem();
        var weaponType = weapon.WeaponType;
        // Prepare animation
        if ((skill.castAnimations == null || skill.castAnimations.Length == 0) && isAttack)
        {
            // If there is no cast animations
                // Random attack animation
                var animArray = !isLeftHand ? weaponType.rightHandAttackAnimations : weaponType.leftHandAttackAnimations;
                var animLength = animArray.Length;
                if (animLength > 0)
                {
                    var anim = animArray[Random.Range(0, animLength)];
                    // Assign animation data
                    actionId = anim.Id;
                    triggerDuration = anim.TriggerDuration / CacheAtkSpeed;
                    totalDuration = (anim.ClipLength + anim.extraDuration) / CacheAtkSpeed;
                }
        }
        else if (skill.castAnimations != null && skill.castAnimations.Length > 0)
        {
            // Random animation
            var animArray = skill.castAnimations;
            var animLength = animArray.Length;
            var anim = animArray[Random.Range(0, animLength)];
            // Assign animation data
            actionId = anim.Id;
            triggerDuration = anim.TriggerDuration / CacheAtkSpeed;
            totalDuration = (anim.ClipLength + anim.extraDuration) / CacheAtkSpeed;
        }
        if (isAttack)
        {
            switch (skill.skillAttackType)
            {
                case SkillAttackType.Normal:
                    // Assign damage data
                    damageInfo = skill.damageInfo;
                    // Calculate all damages
                    allDamageAmounts = weapon.GetDamageAmountWithInflictions(equipWeapon.level, this, skill.GetWeaponDamageInflictions(characterSkill.level));
                    // Sum damage with additional damage amounts
                    allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
                        allDamageAmounts, 
                        skill.GetDamageAmount(characterSkill.level, this));
                    // Sum damage with skill damage
                    allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
                        allDamageAmounts,
                        skill.GetAdditionalDamageAmounts(characterSkill.level));
                    break;
                case SkillAttackType.BasedOnWeapon:
                    // Assign damage data
                    damageInfo = weaponType.damageInfo;
                    // Calculate all damages
                    allDamageAmounts = weapon.GetDamageAmountWithInflictions(equipWeapon.level, this, skill.GetWeaponDamageInflictions(characterSkill.level));
                    // Sum damage with additional damage amounts
                    allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
                        allDamageAmounts,
                        skill.GetAdditionalDamageAmounts(characterSkill.level));
                    break;
            }
            allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
                allDamageAmounts,
                CacheIncreaseDamages);
        }
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
            tempDamageInfo = GameInstance.Singleton.DefaultWeaponItem.WeaponType.damageInfo;
            tempDistance = tempDamageInfo.GetDistance();
            minDistance = tempDistance;
        }
        return minDistance;
    }

    public virtual float GetAttackFov()
    {
        float minFov = float.MaxValue;
        DamageInfo tempDamageInfo;
        float tempFov = 0f;
        var rightHand = EquipWeapons.rightHand;
        var leftHand = EquipWeapons.leftHand;
        var rightHandWeapon = rightHand.GetWeaponItem();
        var leftHandWeapon = leftHand.GetWeaponItem();
        if (rightHandWeapon != null)
        {
            tempDamageInfo = rightHandWeapon.WeaponType.damageInfo;
            tempFov = tempDamageInfo.GetFov();
            if (minFov > tempFov)
                minFov = tempFov;
        }
        if (leftHandWeapon != null)
        {
            tempDamageInfo = leftHandWeapon.WeaponType.damageInfo;
            tempFov = tempDamageInfo.GetFov();
            if (minFov > tempFov)
                minFov = tempFov;
        }
        if (rightHandWeapon == null && leftHandWeapon == null)
        {
            tempDamageInfo = GameInstance.Singleton.DefaultWeaponItem.WeaponType.damageInfo;
            tempFov = tempDamageInfo.GetFov();
            minFov = tempFov;
        }
        return minFov;
    }

    public virtual float GetSkillAttackDistance(Skill skill)
    {
        if (skill == null || !skill.IsAttack())
            return 0f;
        if (skill.skillAttackType == SkillAttackType.Normal)
            return skill.damageInfo.hitDistance;
        else
            return GetAttackDistance();
    }

    public virtual float GetSkillAttackFov(Skill skill)
    {
        if (skill == null || !skill.IsAttack())
            return 0f;
        if (skill.skillAttackType == SkillAttackType.Normal)
            return skill.damageInfo.hitFov;
        else
            return GetAttackFov();
    }

    public virtual void LaunchDamageEntity(
        Vector3 position,
        DamageInfo damageInfo,
        Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
        CharacterBuff debuff,
        int hitEffectsId)
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
                        characterEntity.ReceiveDamage(this, allDamageAmounts, debuff, hitEffectsId);
                }
                break;
            case DamageType.Missile:
                if (damageInfo.missileDamageEntity != null)
                {
                    var missileDamageIdentity = Manager.Assets.NetworkSpawn(damageInfo.missileDamageEntity.Identity, damageTransform.position, damageTransform.rotation);
                    var missileDamageEntity = missileDamageIdentity.GetComponent<MissileDamageEntity>();
                    missileDamageEntity.SetupDamage(this, allDamageAmounts, debuff, hitEffectsId, damageInfo.missileDistance, damageInfo.missileSpeed);
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

    public virtual void ReceivedDamage(BaseCharacterEntity attacker, CombatAmountType combatAmountType, int damage)
    {
        RequestCombatAmount(combatAmountType, damage);
    }

    public virtual void Killed(BaseCharacterEntity lastAttacker)
    {
        StopAllCoroutines();
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

    public virtual void Respawn()
    {
        if (!IsServer || CurrentHp > 0)
            return;
        CurrentHp = CacheMaxHp;
        CurrentMp = CacheMaxMp;
        CurrentStamina = CacheMaxStamina;
        CurrentFood = CacheMaxFood;
        CurrentWater = CacheMaxWater;
        // Send OnRespawn to owner player only
        RequestOnRespawn(false);
    }

    protected virtual void MakeCaches()
    {
        if (!isRecaching)
            return;
        CacheStats = this.GetStats();
        CacheAttributes = this.GetAttributes();
        CacheSkills = this.GetSkills();
        CacheResistances = this.GetResistances();
        CacheIncreaseDamages = this.GetIncreaseDamages();
        CacheMaxHp = (int)CacheStats.hp;
        CacheMaxMp = (int)CacheStats.mp;
        CacheMaxStamina = (int)CacheStats.stamina;
        CacheMaxFood = (int)CacheStats.food;
        CacheMaxWater = (int)CacheStats.water;
        if (database != null)
            CacheBaseMoveSpeed = database.stats.baseStats.moveSpeed;
        CacheMoveSpeed = CacheStats.moveSpeed;
        CacheAtkSpeed = CacheStats.atkSpeed;
        isRecaching = false;
    }

    public virtual bool IsPlayingActionAnimation()
    {
        return animActionType == AnimActionType.Attack || animActionType == AnimActionType.Skill;
    }

    public virtual void IncreaseExp(int exp)
    {
        if (!IsServer)
            return;
        var gameInstance = GameInstance.Singleton;
        if (!gameInstance.GameplayRule.IncreaseExp(this, exp))
            return;
        // Send OnLevelUp to owner player only
        RequestOnLevelUp();
    }
    public abstract bool CanReceiveDamageFrom(BaseCharacterEntity characterEntity);
    public abstract bool IsAlly(BaseCharacterEntity characterEntity);
    public abstract bool IsEnemy(BaseCharacterEntity characterEntity);
}
