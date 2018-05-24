using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;

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
    public UICharacterQuests uiQuests;
    public UINpcDialog uiNpcDialog;
    public UIToggleUI[] toggleUis;

    [Header("Combat Text")]
    public Transform combatTextTransform;
    public UICombatText uiCombatTextMiss;
    public UICombatText uiCombatTextNormalDamage;
    public UICombatText uiCombatTextCriticalDamage;
    public UICombatText uiCombatTextBlockedDamage;
    public UICombatText uiCombatTextHpRecovery;
    public UICombatText uiCombatTextMpRecovery;

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
        var owningCharacter = BasePlayerCharacterController.OwningCharacter;
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
                uiCharacter.Data = BasePlayerCharacterController.OwningCharacter;
        }
    }

    public void UpdateEquipItems()
    {
        if (uiEquipItems != null)
            uiEquipItems.UpdateData(BasePlayerCharacterController.OwningCharacter);
    }

    public void UpdateNonEquipItems()
    {
        if (uiNonEquipItems != null)
            uiNonEquipItems.UpdateData(BasePlayerCharacterController.OwningCharacter);
    }

    public void UpdateSkills()
    {
        if (uiSkills != null)
            uiSkills.UpdateData(BasePlayerCharacterController.OwningCharacter);
    }

    public void UpdateHotkeys()
    {
        if (uiHotkeys != null)
            uiHotkeys.UpdateData(BasePlayerCharacterController.OwningCharacter);
    }

    public void UpdateQuests()
    {
        if (uiQuests != null)
            uiQuests.UpdateData(BasePlayerCharacterController.OwningCharacter);
    }

    public void SetTargetCharacter(BaseCharacterEntity character)
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
        var owningCharacter = BasePlayerCharacterController.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.RequestRespawn();
    }

    public void OnClickExit()
    {
        var gameHandler = FindObjectOfType<RpgGameHandler>();
        gameHandler.Disconnect();
    }

    public void OnCharacterDead(bool isInitialize)
    {
        onCharacterDead.Invoke();
    }

    public void OnCharacterRespawn(bool isInitialize)
    {
        onCharacterRespawn.Invoke();
    }

    public void OnShowNpcDialog(string npcDialogId)
    {
        if (uiNpcDialog == null)
            return;
        NpcDialog npcDialog;
        if (string.IsNullOrEmpty(npcDialogId) || !GameInstance.NpcDialogs.TryGetValue(npcDialogId, out npcDialog))
        {
            uiNpcDialog.Hide();
            return;
        }
        uiNpcDialog.Data = npcDialog;
        uiNpcDialog.Show();
    }
}
