using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibHighLevel;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class CharacterEntity : RpgNetworkEntity, ICharacterData
{
    public const string ANIM_IS_DEAD = "IsDead";
    public const string ANIM_MOVE_SPEED = "MoveSpeed";
    public const string ANIM_Y_SPEED = "YSpeed";
    public const string ANIM_DO_ACTION = "DoAction";
    public const string ANIM_ACTION_ID = "ActionId";
    public const float UPDATE_SKILL_BUFF_INTERVAL = 1f;
    // Use id as primary key
    #region Sync data
    public SyncFieldString modelId = new SyncFieldString();
    public SyncFieldString classId = new SyncFieldString();
    public SyncFieldString characterName = new SyncFieldString();
    public SyncFieldInt level = new SyncFieldInt();
    public SyncFieldInt exp = new SyncFieldInt();
    public SyncFieldFloat currentHp = new SyncFieldFloat();
    public SyncFieldFloat currentMp = new SyncFieldFloat();
    public SyncFieldEquipWeapons equipWeapons = new SyncFieldEquipWeapons();
    // List
    public SyncListCharacterAttribute attributes = new SyncListCharacterAttribute();
    public SyncListCharacterSkill skills = new SyncListCharacterSkill();
    public SyncListCharacterBuff buffs = new SyncListCharacterBuff();
    public SyncListCharacterItem equipItems = new SyncListCharacterItem();
    public SyncListCharacterItem nonEquipItems = new SyncListCharacterItem();
    #endregion

    #region Protected data
    // Entity data
    protected CharacterModel model;
    protected bool doingAction;
    protected readonly Dictionary<string, int> buffIndexes = new Dictionary<string, int>();
    protected readonly Dictionary<string, int> equipItemIndexes = new Dictionary<string, int>();
    protected float lastUpdateSkillAndBuffTime = 0f;
    // Net Functions
    protected LiteNetLibFunction netFuncAttack;
    protected LiteNetLibFunction<NetFieldInt> netFuncUseSkill;
    protected LiteNetLibFunction<NetFieldFloat, NetFieldInt> netFuncPlayActionAnimation;
    protected LiteNetLibFunction<NetFieldUInt> netFuncPickupItem;
    protected LiteNetLibFunction<NetFieldInt, NetFieldInt> netFuncDropItem;
    protected LiteNetLibFunction<NetFieldInt, NetFieldString> netFuncEquipItem;
    protected LiteNetLibFunction<NetFieldString> netFuncUnEquipItem;
    #endregion

    #region Interface implementation
    public string ModelId { get { return modelId; } set { modelId.Value = value; } }
    public string ClassId { get { return classId; } set { classId.Value = value; } }
    public string CharacterName { get { return characterName; } set { characterName.Value = value; } }
    public int Level { get { return level.Value; } set { level.Value = value; } }
    public int Exp { get { return exp.Value; } set { exp.Value = value; } }
    public int CurrentHp { get { return (int)currentHp.Value; } set { currentHp.Value = value; } }
    public int CurrentMp { get { return (int)currentMp.Value; } set { currentMp.Value = value; } }
    public EquipWeapons EquipWeapons { get { return equipWeapons; } set { equipWeapons.Value = value; } }

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
                var buffKey = GetBuffKey(entry.skillId, entry.isDebuff);
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
            var velocity = GetMovementVelocity();
            var moveSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            if (isDead)
            {
                moveSpeed = 0f;
                // Force set to none action when dead
                model.CacheAnimator.SetBool(ANIM_DO_ACTION, false);
            }
            model.CacheAnimator.SetFloat(ANIM_MOVE_SPEED, moveSpeed);
            model.CacheAnimator.SetFloat(ANIM_Y_SPEED, velocity.y);
            model.CacheAnimator.SetBool(ANIM_IS_DEAD, isDead);
        }
    }

    protected void UpdateSkillAndBuff()
    {
        if (CurrentHp <= 0 || !IsServer)
            return;
        var timeDiff = Time.realtimeSinceStartup - lastUpdateSkillAndBuffTime;
        var count = skills.Count;
        for (var i = count - 1; i >= 0; --i)
        {
            var level = skills[i];
            if (level.ShouldUpdate())
            {
                level.Update(Time.unscaledDeltaTime);
                if (timeDiff > UPDATE_SKILL_BUFF_INTERVAL)
                    skills.Dirty(i);
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
                if (timeDiff > UPDATE_SKILL_BUFF_INTERVAL)
                    buffs.Dirty(i);
            }
        }
        if (timeDiff > UPDATE_SKILL_BUFF_INTERVAL)
            lastUpdateSkillAndBuffTime = Time.realtimeSinceStartup;
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
        modelId.sendOptions = SendOptions.ReliableOrdered;
        modelId.forOwnerOnly = false;
        classId.sendOptions = SendOptions.ReliableOrdered;
        classId.forOwnerOnly = false;
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
        modelId.onChange += OnModelIdChange;
        classId.onChange += OnClassIdChange;
        equipWeapons.onChange += OnChangeEquipWeapons;
        // On list changes events
        attributes.onOperation += OnAttributesOperation;
        skills.onOperation += OnSkillsOperation;
        buffs.onOperation += OnBuffsOperation;
        equipItems.onOperation += OnEquipItemsOperation;
        nonEquipItems.onOperation += OnNonEquipItemsOperation;
        // Setup Network functions
        netFuncAttack = new LiteNetLibFunction(NetFuncAttackCallback);
        netFuncUseSkill = new LiteNetLibFunction<NetFieldInt>(NetFuncUseSkillCallback);
        netFuncPlayActionAnimation = new LiteNetLibFunction<NetFieldFloat, NetFieldInt>(NetFuncPlayActionAnimationCallback);
        netFuncPickupItem = new LiteNetLibFunction<NetFieldUInt>(NetFuncPickupItemCallback);
        netFuncDropItem = new LiteNetLibFunction<NetFieldInt, NetFieldInt>(NetFuncDropItemCallback);
        netFuncEquipItem = new LiteNetLibFunction<NetFieldInt, NetFieldString>(NetFuncEquipItemCallback);
        netFuncUnEquipItem = new LiteNetLibFunction<NetFieldString>(NetFuncUnEquipItemCallback);
        // Register Network functions
        RegisterNetFunction("Attack", netFuncAttack);
        RegisterNetFunction("PlayActionAnimation", netFuncPlayActionAnimation);
        RegisterNetFunction("PickupItem", netFuncPickupItem);
        RegisterNetFunction("DropItem", netFuncDropItem);
        RegisterNetFunction("EquipItem", netFuncEquipItem);
        RegisterNetFunction("UnEquipItem", netFuncUnEquipItem);
    }

    protected virtual void OnDestroy()
    {
        // On data changes events
        modelId.onChange -= OnModelIdChange;
        classId.onChange -= OnClassIdChange;
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
    protected void NetFuncAttackCallback()
    {
        NetFuncAttack(1, null, CharacterBuff.Empty);
    }

    protected void NetFuncAttack(float inflictRate, Dictionary<DamageElement, DamageAmount> additionalDamageAttributes, CharacterBuff debuff)
    {
        if (CurrentHp <= 0 || model == null || doingAction)
            return;
        doingAction = true;
        // Prepare weapon data
        bool isLeftHand = false;
        CharacterItem equipWeapon = equipWeapons.Value.GetRandomedItem(out isLeftHand);
        WeaponItem weapon = equipWeapon.GetWeaponItem();
        var weaponType = weapon.WeaponType;
        // Random animation
        var animArray = !isLeftHand ? weaponType.rightHandAttackAnimations : weaponType.leftHandAttackAnimations;
        var actionId = -1;
        var triggerDuration = 0f;
        var totalDuration = 0f;
        var animLength = animArray.Length;
        if (animLength > 0)
        {
            var anim = animArray[Random.Range(0, animLength - 1)];
            actionId = anim.actionId;
            triggerDuration = anim.triggerDuration;
            totalDuration = anim.totalDuration;
        }
        // Play animation on clients
        PlayActionAnimation(totalDuration, actionId);
        // Calculate all damages
        var effectiveness = weapon.GetEffectivenessDamage(this);
        var damageAttribute = weapon.GetDamageAttribute(equipWeapon.level, effectiveness, inflictRate);
        var allDamageAttributes = weapon.GetIncreaseDamageAttributes(equipWeapon.level);
        allDamageAttributes = GameDataHelpers.CombineDamageAttributesDictionary(allDamageAttributes, damageAttribute);
        allDamageAttributes = GameDataHelpers.CombineDamageAttributesDictionary(allDamageAttributes, additionalDamageAttributes);
        // Start attack routine
        StartCoroutine(AttackRoutine(triggerDuration, totalDuration, allDamageAttributes, weaponType.damage, debuff));
    }

    IEnumerator AttackRoutine(float damageDuration,
        float totalDuration,
        Dictionary<DamageElement, DamageAmount> allDamageAttributes,
        Damage damage, 
        CharacterBuff debuff)
    {
        yield return new WaitForSecondsRealtime(damageDuration);
        switch (damage.damageType)
        {
            case DamageType.Melee:
                var hits = Physics.OverlapSphere(CacheTransform.position, model.radius + damage.hitDistance);
                foreach (var hit in hits)
                {
                    var characterEntity = hit.GetComponent<CharacterEntity>();
                    if (characterEntity == null)
                        continue;
                    characterEntity.ReceiveDamage(this, allDamageAttributes, debuff);
                }
                break;
            case DamageType.Missile:
                if (damage.missileDamageEntity != null)
                {
                    var missileDamageIdentity = Manager.Assets.NetworkSpawn(damage.missileDamageEntity.Identity, CacheTransform.position);
                    var missileDamageEntity = missileDamageIdentity.GetComponent<MissileDamageEntity>();
                    missileDamageEntity.SetupDamage(this, allDamageAttributes, debuff, damage.missileDistance, damage.missileSpeed);
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
        if (CurrentHp <= 0 || model == null || doingAction || skillIndex < 0 || skillIndex >= skills.Count)
            return;

        var characterSkill = skills[skillIndex];
        if (!characterSkill.CanUse(CurrentMp))
            return;

        doingAction = true;

        var skill = characterSkill.GetSkill();
        if (skill == null)
        {
            doingAction = false;
            return;
        }

        var anim = skill.castAnimation;
        PlayActionAnimation(anim.totalDuration, anim.actionId);
        StartCoroutine(UseSkillRoutine(skillIndex));
    }

    IEnumerator UseSkillRoutine(int skillIndex)
    {
        var characterSkill = skills[skillIndex];
        var skill = characterSkill.GetSkill();
        var anim = skill.castAnimation;
        yield return new WaitForSecondsRealtime(anim.triggerDuration);
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
        skills[skillIndex].Used();
        skills.Dirty(skillIndex);
        yield return new WaitForSecondsRealtime(anim.totalDuration - anim.triggerDuration);
        doingAction = false;
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
        var damage = skill.damage;
        var effectivenessAttributes = skill.CacheEffectivenessAttributes;
        var debuff = skill.isDebuff ? CharacterBuff.Create(skill, characterSkill.level, true) : CharacterBuff.Empty;
        switch (damage.damageType)
        {
            case DamageType.Melee:
                var hits = Physics.OverlapSphere(CacheTransform.position, model.radius + damage.hitDistance);
                foreach (var hit in hits)
                {
                    var characterEntity = hit.GetComponent<CharacterEntity>();
                    if (characterEntity == null)
                        continue;
                    characterEntity.ReceiveDamage(this, allDamageAttributes, debuff);
                }
                break;
            case DamageType.Missile:
                if (damage.missileDamageEntity != null)
                {
                    var missileDamageIdentity = Manager.Assets.NetworkSpawn(damage.missileDamageEntity.Identity, CacheTransform.position);
                    var missileDamageEntity = missileDamageIdentity.GetComponent<MissileDamageEntity>();
                    missileDamageEntity.SetupDamage(this, allDamageAttributes, debuff, damage.missileDistance, damage.missileSpeed);
                }
                break;
        }
    }

    protected void AttackAsWeaponDamageInflict(CharacterSkill characterSkill)
    {
        var skill = characterSkill.GetSkill();
        if (skill == null)
            return;

        NetFuncAttack(skill.GetInflictRate(characterSkill.level), skill.GetAdditionalDamageAttributes(characterSkill.level), skill.isDebuff ? CharacterBuff.Create(skill, characterSkill.level, true) : CharacterBuff.Empty);
    }

    protected void ApplySkillBuff(CharacterSkill characterSkill)
    {
        var skill = characterSkill.GetSkill();
        if (skill.skillBuffType == SkillBuffType.BuffToUser)
        {
            var buffKey = GetBuffKey(characterSkill.skillId, false);
            var buffIndex = -1;
            if (buffIndexes.TryGetValue(buffKey, out buffIndex))
            {
                buffs.RemoveAt(buffIndex);
                UpdateBuffIndexes();
            }
            var characterBuff = CharacterBuff.Create(skill, characterSkill.level, false);
            characterBuff.Added();
            buffs.Add(characterBuff);
            buffIndexes.Add(buffKey, buffs.Count - 1);
        }
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
        var animator = model.CacheAnimator;
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
            var dropPosition = CacheTransform.position + new Vector3(Random.value * gameInstance.dropDistance, 0, Random.value * gameInstance.dropDistance);
            var identity = Manager.Assets.NetworkSpawn(gameInstance.itemDropEntityPrefab.gameObject, dropPosition);
            var itemDropEntity = identity.GetComponent<ItemDropEntity>();
            var dropData = new CharacterItem();
            dropData.itemId = itemId;
            dropData.level = level;
            dropData.amount = amount;
            itemDropEntity.dropData = dropData;
        }
    }

    protected void NetFuncEquipItemCallback(NetFieldInt nonEquipIndex, NetFieldString equipPosition)
    {
        NetFuncEquipItem(nonEquipIndex, equipPosition);
    }

    protected void NetFuncEquipItem(int nonEquipIndex, string equipPosition)
    {
        if (CurrentHp <= 0 || doingAction || 
            nonEquipIndex < 0 || nonEquipIndex > nonEquipItems.Count)
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
        var tempEquipWeapons = equipWeapons.Value;
        if (equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
        {
            tempEquipWeapons.rightHand = equippingItem;
            equipWeapons.Value = tempEquipWeapons;
        }
        else if (equipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
        {
            tempEquipWeapons.leftHand = equippingItem;
            equipWeapons.Value = tempEquipWeapons;
        }
        else
        {
            equipItems.Add(equippingItem);
            equipItemIndexes.Add(equipPosition, equipItems.Count - 1);
        }
        nonEquipItems.RemoveAt(nonEquipIndex);
    }

    protected void NetFuncUnEquipItemCallback(NetFieldString fromEquipPosition)
    {
        NetFuncUnEquipItem(fromEquipPosition);
    }

    protected void NetFuncUnEquipItem(string fromEquipPosition)
    {
        if (CurrentHp <= 0 || doingAction)
            return;

        var equippedArmorIndex = -1;
        var tempEquipWeapons = equipWeapons.Value;
        var unEquipItem = CharacterItem.Empty;
        if (fromEquipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
        {
            unEquipItem = tempEquipWeapons.rightHand;
            tempEquipWeapons.rightHand = CharacterItem.Empty;
            equipWeapons.Value = tempEquipWeapons;
        }
        else if (fromEquipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
        {
            unEquipItem = tempEquipWeapons.leftHand;
            tempEquipWeapons.leftHand = CharacterItem.Empty;
            equipWeapons.Value = tempEquipWeapons;
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

    public void EquipItem(int nonEquipIndex, string equipPosition)
    {
        CallNetFunction("EquipItem", FunctionReceivers.Server, nonEquipIndex, equipPosition);
    }

    public void UnEquipItem(string equipPosition)
    {
        CallNetFunction("UnEquipItem", FunctionReceivers.Server, equipPosition);
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

        var tempEquipWeapons = equipWeapons.Value;
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
        var gameInstance = GameInstance.Singleton;
        // Calculate chance to hit
        var hitChance = gameInstance.GameplayRule.GetHitChance(attacker, this);
        // If miss, return don't calculate damages
        if (Random.value > hitChance)
            return;

        // Calculate damages
        var totalDamage = 0f;
        if (allDamageAttributes.Count > 0)
        {
            foreach (var allDamageAttribute in allDamageAttributes)
            {
                var damageElement = allDamageAttribute.Key;
                var damageAmount = allDamageAttribute.Value;
                var receivingDamage = damageElement.GetDamageReducedByResistance(this, Random.Range(damageAmount.minDamage, damageAmount.maxDamage));
                if (receivingDamage > 0f)
                    totalDamage += receivingDamage;
            }
        }
        // Apply damages
        CurrentHp -= (int)totalDamage;

        // If current hp <= 0, character dead
        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            StopAllCoroutines();
            doingAction = false;
            buffs.Clear();
            var count = skills.Count;
            for (var i = 0; i < count; ++i)
            {
                var skill = skills[i];
                skill.coolDownRemainsDuration = 0;
                skills.Dirty(i);
            }
        }
        else if (!debuff.IsEmpty())
        {
            var buffKey = GetBuffKey(debuff.skillId, debuff.isDebuff);
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
    /// Override this to do stuffs when model Id changed
    /// </summary>
    /// <param name="modelId"></param>
    protected virtual void OnModelIdChange(string modelId)
    {
        // Setup model
        if (model != null)
            Destroy(model.gameObject);

        model = this.InstantiateModel(transform);
        if (model != null)
        {
            SetupModel(model);
            model.SetEquipWeapons(equipWeapons);
            model.SetEquipItems(equipItems);
        }
    }

    /// <summary>
    /// Override this to do stuffs when class Id changed
    /// </summary>
    /// <param name="classId"></param>
    protected virtual void OnClassIdChange(string classId)
    {
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

    protected void UpdateBuffIndexes()
    {
        buffIndexes.Clear();
        for (var i = 0; i < buffs.Count; ++i)
        {
            var entry = buffs[i];
            var buffKey = GetBuffKey(entry.skillId, entry.isDebuff);
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

    protected abstract void SetupModel(CharacterModel characterModel);
    protected abstract Vector3 GetMovementVelocity();

    public static string GetBuffKey(string skillId, bool isDebuff)
    {
        var keyPrefix = isDebuff ? GameDataConst.CHARACTER_DEBUFF_PREFIX : GameDataConst.CHARACTER_BUFF_PREFIX;
        return keyPrefix + skillId;
    }
}
