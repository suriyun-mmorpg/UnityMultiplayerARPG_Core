using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISceneHome : UIHistory
{
    public UICharacterCreate uiCharacterCreate;
    public UICharacterList uiCharacterList;
    public UILanConnection uiLanConnection;

    public void OnClickSinglePlayer()
    {
        LanRpgNetworkManager.StartType = LanRpgNetworkManager.GameStartType.SinglePlayer;
        Next(uiCharacterList);
    }

    public void OnClickMultiplayer()
    {
        Next(uiLanConnection);
    }

    public void OnClickJoin()
    {
        LanRpgNetworkManager.StartType = LanRpgNetworkManager.GameStartType.Client;
        LanRpgNetworkManager.ConnectingNetworkAddress = uiLanConnection.NetworkAddress;
        Next(uiCharacterList);
    }

    public void OnClickHost()
    {
        LanRpgNetworkManager.StartType = LanRpgNetworkManager.GameStartType.Host;
        Next(uiCharacterList);
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
