using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINpcDialog : UISelectionEntry<NpcDialog>
{
    [Header("Generic Info Format")]
    [Tooltip("Title Format => {0} = {Title}")]
    public string titleFormat = "{0}";
    [Tooltip("Description Format => {0} = {Description}")]
    public string descriptionFormat = "{0}";

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public UICharacterQuest uiCharacterQuest;
    public UINpcDialogMenu uiMenuPrefab;
    public Transform uiMenuContainer;
    public string messageQuestAccept = "Accept";
    public string messageQuestDecline = "Decline";
    public string messageQuestAbandon = "Abandon";
    public string messageQuestComplete = "Complete";

    private UIList cacheList;
    public UIList CacheList
    {
        get
        {
            if (cacheList == null)
            {
                cacheList = gameObject.AddComponent<UIList>();
                cacheList.uiPrefab = uiMenuPrefab.gameObject;
                cacheList.uiContainer = uiMenuContainer;
            }
            return cacheList;
        }
    }

    protected override void UpdateData()
    {
        var dialog = Data;
        var quest = dialog.quest;
        var owningCharacter = BasePlayerCharacterController.OwningCharacter;

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, dialog == null ? "Unknow" : dialog.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, dialog == null ? "N/A" : dialog.description);

        List<UINpcDialogMenuAction> menuActions = new List<UINpcDialogMenuAction>();
        switch(dialog.type)
        {
            case NpcDialogType.Quest:
                if (uiCharacterQuest != null)
                {
                    if (quest == null)
                        uiCharacterQuest.Hide();
                    else
                    {
                        var acceptMenuAction = new UINpcDialogMenuAction();
                        var declineMenuAction = new UINpcDialogMenuAction();
                        var abandonMenuAction = new UINpcDialogMenuAction();
                        var completeMenuAction = new UINpcDialogMenuAction();
                        acceptMenuAction.title = messageQuestAccept;
                        acceptMenuAction.menuIndex = NpcDialog.QUEST_ACCEPT_MENU_INDEX;
                        declineMenuAction.title = messageQuestDecline;
                        declineMenuAction.menuIndex = NpcDialog.QUEST_DECLINE_MENU_INDEX;
                        abandonMenuAction.title = messageQuestAbandon;
                        abandonMenuAction.menuIndex = NpcDialog.QUEST_ABANDON_MENU_INDEX;
                        completeMenuAction.title = messageQuestComplete;
                        completeMenuAction.menuIndex = NpcDialog.QUEST_COMPLETE_MENU_INDEX;

                        CharacterQuest characterQuest;
                        var index = owningCharacter.IndexOfQuest(quest.HashId);
                        if (index >= 0)
                        {
                            characterQuest = owningCharacter.quests[index];
                            if (!characterQuest.IsAllTasksDone(owningCharacter))
                                menuActions.Add(abandonMenuAction);
                            else
                                menuActions.Add(completeMenuAction);
                        }
                        else
                        {
                            characterQuest = CharacterQuest.Create(quest);
                            menuActions.Add(acceptMenuAction);
                            menuActions.Add(declineMenuAction);
                        }
                        uiCharacterQuest.Setup(characterQuest, owningCharacter, index);
                        uiCharacterQuest.Show();
                    }
                }
                break;
            case NpcDialogType.Normal:
                if (uiCharacterQuest != null)
                    uiCharacterQuest.Hide();
                var menus = dialog.menus;
                for (var i = 0; i < menus.Length; ++i)
                {
                    var menu = menus[i];
                    if (menu.IsPassConditions(owningCharacter))
                    {
                        var menuAction = new UINpcDialogMenuAction();
                        menuAction.title = menu.title;
                        menuAction.menuIndex = i;
                        menuActions.Add(menuAction);
                    }
                }
                break;
        }

        CacheList.Generate(menuActions, (index, menuAction, ui) =>
        {
            var uiNpcDialogMenu = ui.GetComponent<UINpcDialogMenu>();
            uiNpcDialogMenu.Data = menuAction;
            uiNpcDialogMenu.uiNpcDialog = this;
            uiNpcDialogMenu.Show();
        });
    }
}
