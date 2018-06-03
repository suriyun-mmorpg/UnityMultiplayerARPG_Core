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
        var networkManager = LanRpgNetworkManager.Singleton;
        networkManager.startType = LanRpgNetworkManager.GameStartType.SinglePlayer;
        Next(uiCharacterList);
    }

    public void OnClickMultiplayer()
    {
        Next(uiLanConnection);
    }

    public void OnClickJoin()
    {
        var networkManager = LanRpgNetworkManager.Singleton;
        networkManager.startType = LanRpgNetworkManager.GameStartType.Client;
        networkManager.networkAddress = uiLanConnection.NetworkAddress;
        Next(uiCharacterList);
    }

    public void OnClickHost()
    {
        var networkManager = LanRpgNetworkManager.Singleton;
        networkManager.startType = LanRpgNetworkManager.GameStartType.Host;
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
