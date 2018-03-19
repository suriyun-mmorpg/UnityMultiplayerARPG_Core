using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibHighLevel;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerCharacterEntity : CharacterEntity
{
    public static PlayerCharacterEntity OwningCharacter { get; private set; }

    #region Net Functions
    protected LiteNetLibFunction<NetFieldInt, NetFieldInt> netFuncSwapOrMergeItem;
    protected LiteNetLibFunction<NetFieldInt> netFuncAddAttribute;
    protected LiteNetLibFunction<NetFieldInt> netFuncAddSkill;
    #endregion

    #region Cache components
    public FollowCameraControls CacheFollowCameraControls { get; protected set; }
    public UISceneGameplay CacheUISceneGameplay { get; protected set; }
    #endregion

    protected virtual void Start()
    {
        var gameInstance = GameInstance.Singleton;
        if (IsLocalClient)
        {
            CacheCharacterMovement.enabled = true;
            CacheFollowCameraControls = Instantiate(gameInstance.gameplayCameraPrefab);
            CacheFollowCameraControls.target = CacheTransform;
            OwningCharacter = this;
            CacheUISceneGameplay = Instantiate(gameInstance.uiSceneGameplayPrefab);
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateSkills();
            CacheUISceneGameplay.UpdateBuffs();
            CacheUISceneGameplay.UpdateEquipItems();
            CacheUISceneGameplay.UpdateNonEquipItems();
        }
    }

    public override void OnSetup()
    {
        base.OnSetup();

        attributes.onOperation += OnAttributesOperation;
        nonEquipItems.onOperation += OnNonEquipItemsOperation;
        skills.onOperation += OnSkillsOperation;

        netFuncSwapOrMergeItem = new LiteNetLibFunction<NetFieldInt, NetFieldInt>(NetFuncSwapOrMergeItemCallback);
        netFuncAddAttribute = new LiteNetLibFunction<NetFieldInt>(NetFuncAddAttributeCallback);
        netFuncAddSkill = new LiteNetLibFunction<NetFieldInt>(NetFuncAddSkillCallback);

        RegisterNetFunction("SwapOrMergeItem", netFuncSwapOrMergeItem);
        RegisterNetFunction("AddAttribute", netFuncAddAttribute);
        RegisterNetFunction("AddSkill", netFuncAddSkill);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        attributes.onOperation -= OnAttributesOperation;
        nonEquipItems.onOperation -= OnNonEquipItemsOperation;
        skills.onOperation -= OnSkillsOperation;
    }

    #region Net functions callbacks
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
                nonEquipItems[fromIndex] = CharacterItem.Empty;
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

    protected void NetFuncAddAttributeCallback(NetFieldInt attributeIndex)
    {
        NetFuncAddAttribute(attributeIndex);
    }

    protected void NetFuncAddAttribute(int attributeIndex)
    {
        if (CurrentHp <= 0 || attributeIndex < 0 || attributeIndex >= attributes.Count)
            return;

        var attribute = attributes[attributeIndex];
        if (!attribute.CanIncrease(this))
            return;

        attribute.Increase(1);
        attributes[attributeIndex] = attribute;
    }

    protected void NetFuncAddSkillCallback(NetFieldInt skillIndex)
    {
        NetFuncAddSkill(skillIndex);
    }

    protected void NetFuncAddSkill(int skillIndex)
    {
        if (CurrentHp <= 0 || skillIndex < 0 || skillIndex >= skills.Count)
            return;

        var skill = skills[skillIndex];
        if (!skill.CanLevelUp(this))
            return;

        skill.LevelUp(1);
        skills[skillIndex] = skill;
    }
    #endregion

    #region Net functions callers
    public void SwapOrMergeItem(int fromIndex, int toIndex)
    {
        CallNetFunction("SwapOrMergeItem", FunctionReceivers.Server, fromIndex, toIndex);
    }

    public void AddAttribute(int attributeIndex)
    {
        CallNetFunction("AddAttribute", FunctionReceivers.Server, attributeIndex);
    }

    public void AddSkill(int skillIndex)
    {
        CallNetFunction("AddSkill", FunctionReceivers.Server, skillIndex);
    }
    #endregion

    protected override void OnPrototypeIdChange(string prototypeId)
    {
        base.OnPrototypeIdChange(prototypeId);

        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateSkills();
            CacheUISceneGameplay.UpdateEquipItems();
            CacheUISceneGameplay.UpdateNonEquipItems();
        }
    }

    protected override void OnChangeEquipWeapons(EquipWeapons equipWeapons)
    {
        base.OnChangeEquipWeapons(equipWeapons);

        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateEquipItems();
        }
    }

    protected void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (IsLocalClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateCharacter();
    }

    protected override void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnBuffsOperation(operation, index);

        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateBuffs();
        }
    }

    protected override void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnEquipItemsOperation(operation, index);

        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateEquipItems();
        }
    }

    protected void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateNonEquipItems();
        }
    }

    protected void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateSkills();
        }
    }
}
