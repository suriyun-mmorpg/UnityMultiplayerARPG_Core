using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    public UICharacterHotkeys uiHotkeys;
    public UIToggleUI[] toggleUis;

    [Header("Events")]
    public UnityEvent onAbleToRespawn;
    public UnityEvent onUnableToRespawn;
    public UnityEvent onCharacterDead;
    public UnityEvent onCharacterRespawn;

    public UICharacterItem SelectedEquipItem { get; private set; }
    public UICharacterItem SelectedNonEquipItem { get; private set; }
    public UICharacterSkill SelectedSkillLevel { get; private set; }

    protected int lastCharacterHp = 0;
    protected CharacterEntity lastSelectedCharacter;

    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        var currentCharacterHp = 0;
        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (owningCharacter != null)
            currentCharacterHp = owningCharacter.CurrentHp;

        foreach (var toggleUi in toggleUis)
        {
            if (Input.GetKeyDown(toggleUi.key))
            {
                var ui = toggleUi.ui;
                ui.Toggle();
            }
        }

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

        // Event when character dead
        if (owningCharacter.CurrentHp <= 0 && lastCharacterHp != owningCharacter.CurrentHp)
            onCharacterDead.Invoke();
        else if (lastCharacterHp != owningCharacter.CurrentHp)
            onCharacterRespawn.Invoke();

        // Respawn button will show when character dead
        if (owningCharacter.CurrentHp <= 0)
            onAbleToRespawn.Invoke();
        else
            onUnableToRespawn.Invoke();

        // Update last character hp to compare on next frame
        if (owningCharacter != null)
            lastCharacterHp = currentCharacterHp;
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

    public void UpdateHotkeys()
    {
        if (uiHotkeys != null)
            uiHotkeys.UpdateData(PlayerCharacterEntity.OwningCharacter);
    }

    public void DeselectSelectedItem()
    {
        if (uiEquipItems != null)
            uiEquipItems.SelectionManager.DeselectSelectedUI();
        if (uiNonEquipItems != null)
            uiNonEquipItems.SelectionManager.DeselectSelectedUI();
    }

    public void OnClickRespawn()
    {
        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.RequestRespawn();
    }

    public void OnClickExit()
    {
        var networkManager = FindObjectOfType<BaseRpgNetworkManager>();
        networkManager.StopHost();
    }
}
