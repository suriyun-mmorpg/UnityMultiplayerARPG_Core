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

[RequireComponent(typeof(CapsuleCollider))]
public abstract class CharacterEntity : RpgNetworkEntity, ICharacterData
{
    public const string ANIM_IS_DEAD = "IsDead";
    public const string ANIM_MOVE_SPEED = "MoveSpeed";
    public const string ANIM_Y_SPEED = "YSpeed";
    public const string ANIM_DO_ACTION = "DoAction";
    public const string ANIM_HURT = "Hurt";
    
    #region Protected data
    protected RpgNetworkEntity targetEntity;
    protected CharacterModel model;
    protected readonly Dictionary<string, int> buffIndexes = new Dictionary<string, int>();
    protected readonly Dictionary<string, int> equipItemIndexes = new Dictionary<string, int>();
    protected Vector3 previousPosition;
    protected Vector3 currentVelocity;
    #endregion

    #region Public data
    public GameObject[] ownerObjects;
    public GameObject[] nonOwnerObjects;
    public Transform modelContainer;
    #endregion

    // Use id as primary key
    #region Sync data
    public SyncFieldString databaseId = new SyncFieldString();
    public SyncFieldString characterName = new SyncFieldString();
    public SyncFieldInt level = new SyncFieldInt();
    public SyncFieldInt exp = new SyncFieldInt();
    public SyncFieldFloat currentHp = new SyncFieldFloat();
    public SyncFieldFloat currentMp = new SyncFieldFloat();
    public SyncFieldEquipWeapons equipWeapons = new SyncFieldEquipWeapons();
    public SyncFieldBool isDoingAction = new SyncFieldBool();
    // List
    public SyncListCharacterAttribute attributes = new SyncListCharacterAttribute();
    public SyncListCharacterSkill skills = new SyncListCharacterSkill();
    public SyncListCharacterBuff buffs = new SyncListCharacterBuff();
    public SyncListCharacterItem equipItems = new SyncListCharacterItem();
    public SyncListCharacterItem nonEquipItems = new SyncListCharacterItem();
    #endregion

    #region Interface implementation
    public virtual string DatabaseId { get { return databaseId; } set { databaseId.Value = value; } }
    public virtual string CharacterName { get { return characterName; } set { characterName.Value = value; } }
    public virtual int Level { get { return level.Value; } set { level.Value = value; } }
    public virtual int Exp { get { return exp.Value; } set { exp.Value = value; } }
    public virtual int CurrentHp { get { return (int)currentHp.Value; } set { currentHp.Value = value; } }
    public virtual int CurrentMp { get { return (int)currentMp.Value; } set { currentMp.Value = value; } }
    public virtual EquipWeapons EquipWeapons { get { return equipWeapons; } set { equipWeapons.Value = value; } }

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
                var buffKey = CharacterBuff.GetBuffId(entry.skillId, entry.isDebuff);
                if (!buffIndexes.ContainsKey(buffKey))
                {
                    buffIndexes.Add(buffKey, i);
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
    }

    protected virtual void Update()
    {
        // Use this to update animations
        UpdateSkillAndBuff();
        UpdateAnimation();
    }

    protected virtual void FixedUpdate()
    {
        // Update current velocity
        Vector3 currentMove = CacheTransform.position - previousPosition;
        currentVelocity = currentMove / Time.deltaTime;
        previousPosition = CacheTransform.position;
    }

    protected virtual void UpdateAnimation()
    {
        if (model != null)
        {
            var animator = model.CacheAnimator;
            var isDead = CurrentHp <= 0;
            var velocity = currentVelocity;
            var moveSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            if (isDead)
            {
                moveSpeed = 0f;
                // Force set to none action when dead
                animator.SetBool(ANIM_DO_ACTION, false);
            }
            animator.SetFloat(ANIM_MOVE_SPEED, moveSpeed);
            animator.SetFloat(ANIM_Y_SPEED, velocity.y);
            animator.SetBool(ANIM_IS_DEAD, isDead);
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
        characterName.sendOptions = SendOptions.ReliableOrdered;
        characterName.forOwnerOnly = false;
        databaseId.sendOptions = SendOptions.ReliableOrdered;
        databaseId.forOwnerOnly = false;
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
        databaseId.onChange += OnDatabaseIdChange;
        equipWeapons.onChange += OnChangeEquipWeapons;
        // On list changes events
        attributes.onOperation += OnAttributesOperation;
        skills.onOperation += OnSkillsOperation;
        buffs.onOperation += OnBuffsOperation;
        equipItems.onOperation += OnEquipItemsOperation;
        nonEquipItems.onOperation += OnNonEquipItemsOperation;
        // Register Network functions
        RegisterNetFunction("Attack", new LiteNetLibFunction(() => NetFuncAttack(1, null, CharacterBuff.Empty)));
        RegisterNetFunction("UseSkill", new LiteNetLibFunction<NetFieldInt>((skillIndex) => NetFuncUseSkill(skillIndex)));
        RegisterNetFunction("PlayActionAnimation", new LiteNetLibFunction<NetFieldInt>((actionId) => NetFuncPlayActionAnimation(actionId)));
        RegisterNetFunction("PickupItem", new LiteNetLibFunction<NetFieldUInt>((objectId) => NetFuncPickupItem(objectId)));
        RegisterNetFunction("DropItem", new LiteNetLibFunction<NetFieldInt, NetFieldInt>((index, amount) => NetFuncDropItem(index, amount)));
        RegisterNetFunction("EquipItem", new LiteNetLibFunction<NetFieldInt, NetFieldString>((nonEquipIndex, equipPosition) => NetFuncEquipItem(nonEquipIndex, equipPosition)));
        RegisterNetFunction("UnEquipItem", new LiteNetLibFunction<NetFieldString>((fromEquipPosition) => NetFuncUnEquipItem(fromEquipPosition)));
        RegisterNetFunction("CombatAmount", new LiteNetLibFunction<NetFieldByte, NetFieldInt>((combatAmountTypes, amount) => NetFuncCombatAmount((CombatAmountTypes)combatAmountTypes.Value, amount)));
    }

    protected virtual void OnDestroy()
    {
        // On data changes events
        databaseId.onChange -= OnDatabaseIdChange;
        equipWeapons.onChange -= OnChangeEquipWeapons;
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
    protected void NetFuncAttack(float inflictRate, Dictionary<DamageElement, DamageAmount> additionalDamageAttributes, CharacterBuff debuff)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;

        // Prepare requires data
        int actionId;
        float damageDuration;
        float totalDuration;
        Dictionary<DamageElement, DamageAmount> allDamageAttributes;
        DamageInfo damageInfo;

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
        RequestPlayActionAnimation(actionId);
        // Start attack routine
        StartCoroutine(AttackRoutine(damageDuration, totalDuration, damageInfo, allDamageAttributes, debuff));
    }

    IEnumerator AttackRoutine(
        float damageDuration,
        float totalDuration,
        DamageInfo damageInfo,
        Dictionary<DamageElement, DamageAmount> allDamageAttributes,
        CharacterBuff debuff)
    {
        yield return new WaitForSecondsRealtime(damageDuration);
        LaunchDamageEntity(damageInfo, allDamageAttributes, debuff);
        yield return new WaitForSecondsRealtime(totalDuration - damageDuration);
        isDoingAction.Value = false;
    }

    protected void NetFuncUseSkillCallback(NetFieldInt skillIndex)
    {
        NetFuncUseSkill(skillIndex);
    }

    /// <summary>
    /// Is function will be called at server to order character to use skill
    /// </summary>
    /// <param name="skillIndex">Index in `characterSkills` list which will be used</param>
    protected void NetFuncUseSkill(int skillIndex)
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
        RequestPlayActionAnimation(anim.Id);
        // Start use skill routine
        StartCoroutine(UseSkillRoutine(skillIndex));
    }

    IEnumerator UseSkillRoutine(int skillIndex)
    {
        var characterSkill = skills[skillIndex];
        characterSkill.Used();
        characterSkill.ReduceMp(this);
        skills[skillIndex] = characterSkill;

        var skill = characterSkill.GetSkill();
        var anim = skill.castAnimation;
        yield return new WaitForSecondsRealtime(anim.TriggerDuration);
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
        yield return new WaitForSecondsRealtime(anim.ClipLength - anim.TriggerDuration);
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
        var debuff = skill.isDebuff ? CharacterBuff.Create(skill.Id, true, characterSkill.level) : CharacterBuff.Empty;
        LaunchDamageEntity(damageInfo, allDamageAttributes, debuff);
    }

    protected void AttackAsWeaponDamageInflict(CharacterSkill characterSkill)
    {
        var skill = characterSkill.GetSkill();
        if (skill == null)
            return;

        NetFuncAttack(
            skill.GetInflictRate(characterSkill.level), 
            skill.GetAdditionalDamageAttributes(characterSkill.level), 
            skill.isDebuff ? CharacterBuff.Create(characterSkill.skillId, true, characterSkill.level) : CharacterBuff.Empty);
    }

    protected void ApplySkillBuff(CharacterSkill characterSkill)
    {
        var skill = characterSkill.GetSkill();
        if (skill.skillBuffType == SkillBuffType.BuffToUser)
        {
            var buffKey = CharacterBuff.GetBuffId(characterSkill.skillId, false);
            var buffIndex = -1;
            if (buffIndexes.TryGetValue(buffKey, out buffIndex))
            {
                buffs.RemoveAt(buffIndex);
                UpdateBuffIndexes();
            }
            var characterBuff = CharacterBuff.Create(characterSkill.skillId, false, characterSkill.level);
            characterBuff.Added();
            buffs.Add(characterBuff);
            buffIndexes.Add(buffKey, buffs.Count - 1);
        }
    }

    /// <summary>
    /// This will be called at every clients to play any action animation
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="actionId"></param>
    protected void NetFuncPlayActionAnimation(int actionId)
    {
        if (CurrentHp <= 0)
            return;
        StartCoroutine(PlayActionAnimationRoutine(actionId));
    }

    IEnumerator PlayActionAnimationRoutine(int actionId)
    {
        Animator animator = model == null ? null : model.CacheAnimator;
        // If animator is not null, play the action animation
        ActionAnimation actionAnimation;
        if (animator != null && GameInstance.ActionAnimations.TryGetValue(actionId, out actionAnimation))
        {
            model.ChangeActionClip(actionAnimation.clip);
            animator.SetBool(ANIM_DO_ACTION, false);
            animator.SetBool(ANIM_DO_ACTION, true);
            yield return new WaitForSecondsRealtime(actionAnimation.ClipLength);
            animator.SetBool(ANIM_DO_ACTION, false);
        }
    }

    /// <summary>
    /// This will be called at server to order character to pickup items
    /// </summary>
    /// <param name="objectId"></param>
    protected void NetFuncPickupItem(uint objectId)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;

        var gameInstance = GameInstance.Singleton;
        var spawnedObjects = Manager.Assets.SpawnedObjects;
        LiteNetLibIdentity spawnedObject;
        // Find object by objectId, if not found don't continue
        if (!spawnedObjects.TryGetValue(objectId, out spawnedObject))
            return;
        
        // Don't pickup item if it's too far
        if (Vector3.Distance(CacheTransform.position, spawnedObject.transform.position) >= gameInstance.pickUpItemDistance)
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

    }
    #endregion

    #region Net functions callers
    public void RequestAttack()
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("Attack", FunctionReceivers.Server);
    }

    protected void RequestUseSkill(int skillIndex)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("UseSkill", FunctionReceivers.Server, skillIndex);
    }

    public void RequestPlayActionAnimation(int actionId)
    {
        if (CurrentHp <= 0)
            return;
        CallNetFunction("PlayActionAnimation", FunctionReceivers.All, actionId);
    }

    public void RequestPickupItem(uint objectId)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("PickupItem", FunctionReceivers.Server, objectId);
    }

    public void RequestDropItem(int index, int amount)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("DropItem", FunctionReceivers.Server, index, amount);
    }

    public void RequestEquipItem(int nonEquipIndex, string equipPosition)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("EquipItem", FunctionReceivers.Server, nonEquipIndex, equipPosition);
    }

    public void RequestUnEquipItem(string equipPosition)
    {
        if (CurrentHp <= 0 || isDoingAction.Value)
            return;
        CallNetFunction("UnEquipItem", FunctionReceivers.Server, equipPosition);
    }

    public void RequestCombatAmount(CombatAmountTypes combatAmountTypes, int amount)
    {
        CallNetFunction("CombatAmount", FunctionReceivers.All, combatAmountTypes, amount);
    }
    #endregion

    #region Inventory helpers
    public bool IncreaseItems(string itemId, int level, int amount)
    {
        Item itemData;
        // If item not valid
        if (string.IsNullOrEmpty(itemId) || amount <= 0 || !GameInstance.Items.TryGetValue(itemId, out itemData))
            return false;

        var stats = this.GetStatsWithBuffs();
        var maxStack = itemData.maxStack;
        var weight = itemData.weight;
        // If overwhelming
        if (this.GetTotalItemWeight() + (amount * weight) >= stats.weightLimit)
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

    public virtual void ReceiveDamage(CharacterEntity attacker,
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
            OnReceivedDamage(attacker, CombatAmountTypes.Miss, 0);
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
            OnReceivedDamage(attacker, CombatAmountTypes.BlockedDamage, totalDamage);
        else if (isCritical)
            OnReceivedDamage(attacker, CombatAmountTypes.CriticalDamage, totalDamage);
        else
            OnReceivedDamage(attacker, CombatAmountTypes.NormalDamage, totalDamage);

        if (model != null)
        {
            var animator =  model.CacheAnimator;
            animator.ResetTrigger(ANIM_HURT);
            animator.SetTrigger(ANIM_HURT);
        }

        // If current hp <= 0, character dead
        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            OnDead(attacker);
        }
        else if (!debuff.IsEmpty())
        {
            var buffKey = CharacterBuff.GetBuffId(debuff.skillId, debuff.isDebuff);
            var buffIndex = -1;
            if (buffIndexes.TryGetValue(buffKey, out buffIndex))
            {
                buffs.RemoveAt(buffIndex);
                UpdateBuffIndexes();
            }
            var characterDebuff = debuff.Clone();
            characterDebuff.Added();
            buffs.Add(characterDebuff);
            buffIndexes.Add(buffKey, buffs.Count - 1);
        }
    }
    #endregion
    
    #region Sync data changes callback
    /// <summary>
    /// Override this to do stuffs when database Id changed
    /// </summary>
    /// <param name="databaseId"></param>
    protected virtual void OnDatabaseIdChange(string databaseId)
    {
        // Setup model
        if (model != null)
            Destroy(model.gameObject);

        model = this.InstantiateModel(CacheModelContainer);
        if (model != null)
        {
            SetupModel(model);
            model.SetEquipWeapons(equipWeapons);
            model.SetEquipItems(equipItems);
        }
    }

    /// <summary>
    /// Override this to do stuffs when equip weapons changes
    /// </summary>
    /// <param name="equipWeapons"></param>
    protected virtual void OnChangeEquipWeapons(EquipWeapons equipWeapons)
    {
        if (model != null)
            model.SetEquipWeapons(equipWeapons);
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
    }

    /// <summary>
    /// Override this to do stuffs when skills changes
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="index"></param>
    protected virtual void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
    }

    /// <summary>
    /// Override this to do stuffs when buffs changes
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="index"></param>
    protected virtual void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (model != null)
            model.SetBuffs(buffs);
    }

    /// <summary>
    /// Override this to do stuffs when equip items changes
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="index"></param>
    protected virtual void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (model != null)
            model.SetEquipItems(equipItems);
    }

    /// <summary>
    /// Override this to do stuffs when non equip items changes
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="index"></param>
    protected virtual void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
    }
    #endregion

    #region Keys indexes update functions
    protected void UpdateBuffIndexes()
    {
        buffIndexes.Clear();
        for (var i = 0; i < buffs.Count; ++i)
        {
            var entry = buffs[i];
            var buffKey = CharacterBuff.GetBuffId(entry.skillId, entry.isDebuff);
            if (!buffIndexes.ContainsKey(buffKey))
                buffIndexes.Add(buffKey, i);
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
    public void SetTargetEntity(RpgNetworkEntity newTargetEntity)
    {
        targetEntity = newTargetEntity;
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
            damageDuration = anim.TriggerDuration;
            totalDuration = anim.ClipLength;
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
                var hits = Physics.OverlapSphere(damageTransform.position, damageInfo.hitDistance);
                foreach (var hit in hits)
                {
                    var characterEntity = hit.GetComponent<CharacterEntity>();
                    if (characterEntity == null)
                        continue;
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

    protected virtual void OnReceivedDamage(CharacterEntity attacker, CombatAmountTypes damageAmountType, int damage)
    {
        RequestCombatAmount(damageAmountType, damage);
    }

    protected virtual void OnDead(CharacterEntity lastAttacker)
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
    }

    protected virtual void OnLevelUp()
    {
    }

    internal virtual void Respawn()
    {
        if (!IsServer || CurrentHp > 0)
            return;
        CurrentHp = this.GetMaxHp();
        CurrentMp = this.GetMaxMp();
    }

    internal virtual void IncreaseExp(int exp)
    {
        if (!IsServer)
            return;
        if (GameInstance.Singleton.GameplayRule.IncreaseExp(this, exp))
            OnLevelUp();
    }

    protected abstract bool CanReceiveDamageFrom(CharacterEntity characterEntity);
    protected abstract bool IsAlly(CharacterEntity characterEntity);
    protected abstract bool IsEnemy(CharacterEntity characterEntity);
}
