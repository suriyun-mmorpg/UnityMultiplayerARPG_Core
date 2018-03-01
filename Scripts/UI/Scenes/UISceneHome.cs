using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISceneHome : UIHistory
{
    public UICharacterCreate uiCharacterCreate;
    public UICharacterSelection uiCharacterSelection; 

    public void OnClickSinglePlayer()
    {
        LanRpgNetworkManager.StartType = LanRpgNetworkManager.GameStartType.SinglePlayer;
        Next(uiCharacterSelection);
    }

    public void OnClickMultiplayer()
    {
        // TODO: Show LAN host list
    }

    public void OnClickCreateCharacter()
    {
        Next(uiCharacterCreate);
    }

    public void OnClickExit()
    {
        Application.Quit();
    }
}
