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
    public UnityEvent onCharacterDead;
    public UnityEvent onCharacterRespawn;

    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        var currentCharacterHp = 0;
        var owningCharacter = PlayerCharacterController.OwningCharacter;
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
    }

    public void UpdateCharacter()
    {
        foreach (var uiCharacter in uiCharacters)
        {
            if (uiCharacter != null)
                uiCharacter.Data = PlayerCharacterController.OwningCharacter;
        }
    }

    public void UpdateEquipItems()
    {
        if (uiEquipItems != null)
            uiEquipItems.UpdateData(PlayerCharacterController.OwningCharacter);
    }

    public void UpdateNonEquipItems()
    {
        if (uiNonEquipItems != null)
            uiNonEquipItems.UpdateData(PlayerCharacterController.OwningCharacter);
    }

    public void UpdateSkills()
    {
        if (uiSkills != null)
            uiSkills.UpdateData(PlayerCharacterController.OwningCharacter);
    }

    public void UpdateHotkeys()
    {
        if (uiHotkeys != null)
            uiHotkeys.UpdateData(PlayerCharacterController.OwningCharacter);
    }

    public void SetTargetCharacter(CharacterEntity character)
    {
        if (uiTargetCharacter == null)
            return;

        if (character == null || character.CurrentHp <= 0)
        {
            uiTargetCharacter.Hide();
            return;
        }

        uiTargetCharacter.Data = character;
        uiTargetCharacter.Show();
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
        var owningCharacter = PlayerCharacterController.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.RequestRespawn();
    }

    public void OnClickExit()
    {
        var networkManager = FindObjectOfType<BaseRpgNetworkManager>();
        networkManager.StopHost();
    }

    public void OnCharacterDead()
    {
        onCharacterDead.Invoke();
    }

    public void OnCharacterRespawn()
    {
        onCharacterRespawn.Invoke();
    }
}
