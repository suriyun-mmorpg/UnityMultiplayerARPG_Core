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
    public UICharacter uiEnemyCharacter;
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
        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        foreach (var toggleUi in toggleUis)
        {
            if (Input.GetKeyDown(toggleUi.key))
            {
                var ui = toggleUi.ui;
                ui.Toggle();
            }
        }
        // Respawn button will show when character dead
        if (buttonRespawn != null)
            buttonRespawn.gameObject.SetActive(PlayerCharacterEntity.OwningCharacter.CurrentHp <= 0);
        // Enemy status will be show when selected at enemy and enemy hp more than 0
        if (uiEnemyCharacter != null)
        {
            CharacterEntity targetCharacter = null;
            if (uiEnemyCharacter.IsVisible())
            {
                if (owningCharacter == null ||
                    !owningCharacter.TryGetTargetEntity(out targetCharacter) ||
                    targetCharacter.CurrentHp <= 0)
                    uiEnemyCharacter.Hide();
            }
            else
            {
                if (owningCharacter != null &&
                    owningCharacter.TryGetTargetEntity(out targetCharacter) &&
                    targetCharacter.CurrentHp > 0)
                {
                    uiEnemyCharacter.Data = targetCharacter;
                    uiEnemyCharacter.Show();
                }
            }
        }
    }

    public void UpdateCharacter()
    {
        foreach (var uiCharacter in uiCharacters)
        {
            if (uiCharacter != null)
                uiCharacter.Data = PlayerCharacterEntity.OwningCharacter;
        }
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
