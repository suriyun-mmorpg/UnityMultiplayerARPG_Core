using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISceneGameplay : MonoBehaviour
{
    [System.Serializable]
    public struct UIToggleUI
    {
        public UIBase ui;
        public KeyCode key;
    }

    public static UISceneGameplay Singleton { get; private set; }

    public UICharacter[] uiCharacters;
    public UICharacterBuffs uiBuffList;
    public UIEquipItems uiEquipItems;
    public UINonEquipItems uiNonEquipItems;
    public UICharacterSkills uiSkills;
    public UIToggleUI[] toggleUis;
    public Button buttonRespawn;
    public Button buttonExit;
    
    public UICharacterItem SelectedEquipItem { get; private set; }
    public UICharacterItem SelectedNonEquipItem { get; private set; }
    public UICharacterSkill SelectedSkillLevel { get; private set; }

    private void Awake()
    {
        Singleton = this;
        if (buttonRespawn != null)
            buttonRespawn.onClick.AddListener(OnClickRespawn);
        if (buttonExit != null)
            buttonExit.onClick.AddListener(OnClickExit);
    }

    private void Update()
    {
        foreach (var toggleUi in toggleUis)
        {
            if (Input.GetKeyDown(toggleUi.key))
            {
                var ui = toggleUi.ui;
                ui.Toggle();
            }
        }
        if (buttonRespawn != null)
            buttonRespawn.gameObject.SetActive(PlayerCharacterEntity.OwningCharacter.CurrentHp <= 0);
    }

    public void UpdateCharacter()
    {
        foreach (var uiCharacter in uiCharacters)
        {
            if (uiCharacter != null)
                uiCharacter.Data = PlayerCharacterEntity.OwningCharacter;
        }
    }

    public void UpdateBuffs()
    {
        if (uiBuffList != null)
            uiBuffList.UpdateData(PlayerCharacterEntity.OwningCharacter);
    }

    public void UpdateEquipItems()
    {
        if (uiEquipItems != null)
            uiEquipItems.UpdateData(PlayerCharacterEntity.OwningCharacter);
    }

    public void UpdateNonEquipItems()
    {
        if (uiNonEquipItems != null)
            uiNonEquipItems.UpdateData(PlayerCharacterEntity.OwningCharacter);
    }

    public void UpdateSkills()
    {
        if (uiSkills != null)
            uiSkills.UpdateData(PlayerCharacterEntity.OwningCharacter);
    }

    public void DeselectSelectedItem()
    {
        if (uiEquipItems != null)
            uiEquipItems.SelectionManager.DeselectSelectedUI();
        if (uiNonEquipItems != null)
            uiNonEquipItems.SelectionManager.DeselectSelectedUI();
    }

    private void OnClickRespawn()
    {

    }

    private void OnClickExit()
    {
        var networkManager = FindObjectOfType<BaseRpgNetworkManager>();
        networkManager.StopHost();
    }
}
