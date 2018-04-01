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
    public UICharacter uiTargetCharacter;
    public UIEquipItems uiEquipItems;
    public UINonEquipItems uiNonEquipItems;
    public UICharacterSkills uiSkills;
    public UIBase uiDeadDialog;
    public UIToggleUI[] toggleUis;
    public Button buttonRespawn;
    public Button buttonExit;
    
    public UICharacterItem SelectedEquipItem { get; private set; }
    public UICharacterItem SelectedNonEquipItem { get; private set; }
    public UICharacterSkill SelectedSkillLevel { get; private set; }

    protected int lastCharacterHp = 0;
    protected CharacterEntity lastSelectedCharacter;
    protected bool isDeadDialogShown;

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
            buttonRespawn.gameObject.SetActive(owningCharacter.CurrentHp <= 0);
        // Enemy status will be show when selected at enemy and enemy hp more than 0
        if (uiTargetCharacter != null)
        {
            CharacterEntity targetCharacter = null;
            if (uiTargetCharacter.IsVisible())
            {
                if (owningCharacter == null ||
                    !owningCharacter.TryGetTargetEntity(out targetCharacter) ||
                    targetCharacter.CurrentHp <= 0)
                    uiTargetCharacter.Hide();
                else if (targetCharacter != lastSelectedCharacter)
                    uiTargetCharacter.Data = lastSelectedCharacter = targetCharacter;
            }
            else
            {
                if (owningCharacter != null &&
                    owningCharacter.TryGetTargetEntity(out targetCharacter) &&
                    targetCharacter.CurrentHp > 0)
                {
                    uiTargetCharacter.Data = lastSelectedCharacter = targetCharacter;
                    uiTargetCharacter.Show();
                }
            }
        }

        if (uiDeadDialog != null)
        {
            var currentCharacterHp = 0;
            if (owningCharacter != null)
                currentCharacterHp = owningCharacter.CurrentHp;
            if (owningCharacter.CurrentHp <= 0)
            {
                // Avoid dead dialog showing when start game first time
                if (!isDeadDialogShown && lastCharacterHp != owningCharacter.CurrentHp)
                {
                    uiDeadDialog.Show();
                    isDeadDialogShown = true;
                }
            }
            else
                isDeadDialogShown = false;
        }

        if (owningCharacter != null)
            lastCharacterHp = owningCharacter.CurrentHp;
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
        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.RequestRespawn();
    }

    private void OnClickExit()
    {
        var networkManager = FindObjectOfType<BaseRpgNetworkManager>();
        networkManager.StopHost();
    }
}
