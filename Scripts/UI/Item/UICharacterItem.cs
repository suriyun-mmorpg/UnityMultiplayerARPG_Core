using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterItem : UISelectionEntry<KeyValuePair<CharacterItem, int>>
{
    public int indexOfData { get; protected set; }
    public string equipPosition { get; protected set; }

    [Header("Generic Info Format")]
    [Tooltip("Title Format => {0} = {Title}")]
    public string titleFormat = "{0}";
    [Tooltip("Description Format => {0} = {Description}")]
    public string descriptionFormat = "{0}";
    [Tooltip("Level Format => {0} = {Level}")]
    public string levelFormat = "Lv: {0}";
    [Tooltip("Sell Price Format => {0} = {Sell price}")]
    public string sellPriceFormat = "{0}";
    [Tooltip("Stack Format => {0} = {Amount}, {1} = {Max stack}")]
    public string stackFormat = "{0}/{1}";
    [Tooltip("Weight Format => {0} = {Weight}")]
    public string weightFormat = "{0}";
    [Tooltip("Item Type Format => {0} = {Item Type title}")]
    public string itemTypeFormat = "Item Type: {0}";
    [Tooltip("General Item Type")]
    public string generalItemType = "General Item";
    [Tooltip("Shield Item Type")]
    public string shieldItemType = "Shield";

    [Header("Input Dialog Settings")]
    public string dropInputTitle = "Drop Item";
    public string dropInputDescription = "";

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public Text textLevel;
    public Image imageIcon;
    public Text textSellPrice;
    public Text textStack;
    public Text textWeight;
    public Text textItemType;

    [Header("Equipment - UI Elements")]
    public UIEquipmentItemRequirement uiRequirement;
    public UICharacterStats uiStats;
    public UIAttributeAmounts uiIncreaseAttributes;
    public UIResistanceAmounts uiIncreaseResistances;
    public UIDamageElementAmounts uiIncreaseDamageAttributes;

    [Header("Weapon - UI Elements")]
    public UIDamageElementAmount uiDamageAttribute;

    [Header("Action Buttons")]
    public Button buttonEquip;
    public Button buttonUnEquip;
    public Button buttonDrop;

    [Header("Options")]
    public UICharacterItem uiNextLevelItem;
    public GameObject[] levelZeroDeactivateObjects;
    public bool hideAmountWhenMaxIsOne;

    public void Setup(KeyValuePair<CharacterItem, int> data, int indexOfData, string equipPosition)
    {
        this.indexOfData = indexOfData;
        this.equipPosition = equipPosition;
        Data = data;
    }

    protected override void UpdateData()
    {
        var characterItem = Data.Key;
        var level = Data.Value;
        var item = characterItem.GetItem();
        var equipmentItem = characterItem.GetEquipmentItem();
        var armorItem = characterItem.GetArmorItem();
        var weaponItem = characterItem.GetWeaponItem();
        var shieldItem = characterItem.GetShieldItem();
        
        foreach (var levelZeroDeactivateObject in levelZeroDeactivateObjects)
        {
            levelZeroDeactivateObject.SetActive(level > 0);
        }

        if (buttonEquip != null)
        {
            buttonEquip.gameObject.SetActive(string.IsNullOrEmpty(equipPosition));
            buttonEquip.onClick.RemoveListener(OnClickEquip);
            buttonEquip.onClick.AddListener(OnClickEquip);
        }

        if (buttonUnEquip != null)
        {
            buttonUnEquip.gameObject.SetActive(!string.IsNullOrEmpty(equipPosition));
            buttonUnEquip.onClick.RemoveListener(OnClickUnEquip);
            buttonUnEquip.onClick.AddListener(OnClickUnEquip);
        }

        if (buttonDrop != null)
        {
            buttonDrop.gameObject.SetActive(string.IsNullOrEmpty(equipPosition));
            buttonDrop.onClick.RemoveListener(OnClickDrop);
            buttonDrop.onClick.AddListener(OnClickDrop);
        }

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, item == null ? "Unknow" : item.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, item == null ? "N/A" : item.description);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, level.ToString("N0"));

        if (imageIcon != null)
        {
            imageIcon.sprite = item == null ? null : item.icon;
            imageIcon.gameObject.SetActive(item != null);
        }

        if (textSellPrice != null)
            textSellPrice.text = string.Format(sellPriceFormat, item == null ? "0" : item.sellPrice.ToString("N0"));

        if (textStack != null)
        {
            var stackString = "";
            if (!hideAmountWhenMaxIsOne)
            {
                if (item == null)
                    stackString = string.Format(stackFormat, "0", "0");
                else
                    stackString = string.Format(stackFormat, characterItem.amount.ToString("N0"), item.maxStack);
            }
            textStack.text = stackString;
        }

        if (textWeight != null)
            textWeight.text = string.Format(weightFormat, item == null ? "0" : item.weight.ToString("N2"));

        if (textItemType != null)
        {
            if (armorItem != null)
                textItemType.text = string.Format(itemTypeFormat, armorItem.ArmorType.title);
            else if (weaponItem != null)
                textItemType.text = string.Format(itemTypeFormat, weaponItem.WeaponType.title);
            else if (shieldItem != null)
                textItemType.text = string.Format(itemTypeFormat, shieldItemType);
            else
                textItemType.text = string.Format(itemTypeFormat, generalItemType);
        }

        if (uiRequirement != null)
        {
            if (equipmentItem == null || (equipmentItem.requirement.level == 0 && equipmentItem.requirement.character == null && equipmentItem.CacheRequireAttributeAmounts.Count == 0))
                uiRequirement.Hide();
            else
            {
                uiRequirement.Show();
                uiRequirement.Data = new KeyValuePair<Item, int>(equipmentItem, level);
            }
        }

        if (uiStats != null)
        {
            var stats = equipmentItem.GetStats(level);
            if (equipmentItem == null || stats.IsEmpty())
                uiStats.Hide();
            else
            {
                uiStats.Show();
                uiStats.Data = stats;
            }
        }

        if (uiIncreaseAttributes != null)
        {
            var attributes = equipmentItem.GetIncreaseAttributes(level);
            if (equipmentItem == null || attributes == null || attributes.Count == 0)
                uiIncreaseAttributes.Hide();
            else
            {
                uiIncreaseAttributes.Show();
                uiIncreaseAttributes.Data = attributes;
            }
        }

        if (uiIncreaseResistances != null)
        {
            var resistances = equipmentItem.GetIncreaseResistances(level);
            if (equipmentItem == null || resistances == null || resistances.Count == 0)
                uiIncreaseResistances.Hide();
            else
            {
                uiIncreaseResistances.Show();
                uiIncreaseResistances.Data = resistances;
            }
        }

        if (uiIncreaseDamageAttributes != null)
        {
            var damageAttributes = equipmentItem.GetIncreaseDamageAttributes(level);
            if (equipmentItem == null || damageAttributes == null || damageAttributes.Count == 0)
                uiIncreaseDamageAttributes.Hide();
            else
            {
                uiIncreaseDamageAttributes.Show();
                uiIncreaseDamageAttributes.Data = damageAttributes;
            }
        }

        if (uiDamageAttribute != null)
        {
            if (weaponItem == null)
                uiDamageAttribute.Hide();
            else
            {
                uiDamageAttribute.Show();
                uiDamageAttribute.Data = weaponItem.GetDamageAttribute(level, 0f, 1f);
            }
        }

        if (uiNextLevelItem != null)
        {
            if (level + 1 > item.maxLevel)
                uiNextLevelItem.Hide();
            else
            {
                uiNextLevelItem.Setup(new KeyValuePair<CharacterItem, int>(characterItem, level + 1), indexOfData, equipPosition);
                uiNextLevelItem.Show();
            }
        }
    }

    private void OnClickEquip()
    {
        // Only unequpped equipment can be equipped
        if (!string.IsNullOrEmpty(equipPosition))
            return;
        
        if (selectionManager != null)
            selectionManager.DeselectSelectedUI();

        var characterItem = Data.Key;
        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (owningCharacter != null)
        {
            var armorItem = characterItem.GetArmorItem();
            var weaponItem = characterItem.GetWeaponItem();
            var shieldItem = characterItem.GetShieldItem();
            if (weaponItem != null)
            {
                if (weaponItem.EquipType == WeaponItemEquipType.OneHandCanDual)
                {
                    var equipWeapons = owningCharacter.EquipWeapons;
                    var rightWeapon = equipWeapons.rightHand.GetWeaponItem();
                    if (rightWeapon != null && rightWeapon.EquipType == WeaponItemEquipType.OneHandCanDual)
                        owningCharacter.RequestEquipItem(indexOfData, GameDataConst.EQUIP_POSITION_LEFT_HAND);
                    else
                        owningCharacter.RequestEquipItem(indexOfData, GameDataConst.EQUIP_POSITION_RIGHT_HAND);
                }
                else
                    owningCharacter.RequestEquipItem(indexOfData, GameDataConst.EQUIP_POSITION_RIGHT_HAND);
            }
            else if (shieldItem != null)
                owningCharacter.RequestEquipItem(indexOfData, GameDataConst.EQUIP_POSITION_LEFT_HAND);
            else if (armorItem != null)
                owningCharacter.RequestEquipItem(indexOfData, armorItem.EquipPosition);
        }
    }

    private void OnClickUnEquip()
    {
        // Only equipped equipment can be unequipped
        if (string.IsNullOrEmpty(equipPosition))
            return;

        if (selectionManager != null)
            selectionManager.DeselectSelectedUI();

        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.RequestUnEquipItem(equipPosition);
    }

    private void OnClickDrop()
    {
        // Only unequpped equipment can be dropped
        if (!string.IsNullOrEmpty(equipPosition))
            return;

        var characterItem = Data.Key;
        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (characterItem.amount == 1)
        {
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            if (owningCharacter != null)
                owningCharacter.RequestDropItem(indexOfData, 1);
        }
        else
            UISceneGlobal.Singleton.ShowInputDialog(dropInputTitle, dropInputDescription, OnDropAmountConfirmed, 1, characterItem.amount, characterItem.amount);
    }

    private void OnDropAmountConfirmed(int amount)
    {
        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (selectionManager != null)
            selectionManager.DeselectSelectedUI();
        if (owningCharacter != null)
            owningCharacter.RequestDropItem(indexOfData, amount);
    }
}

[System.Serializable]
public class UICharacterItemEvent : UnityEvent<UICharacterItem> { }
