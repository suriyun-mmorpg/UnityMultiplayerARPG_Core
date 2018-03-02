using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILanConnection : UIBase
{
    public InputField inputNetworkAddress;
    public string DefaultNetworkAddress
    {
        get { return GameInstance.Singleton.GetExtra<LanGameInstanceExtra>().networkAddress; }
    }
    public string NetworkAddress
    {
        get { return inputNetworkAddress == null ? DefaultNetworkAddress : inputNetworkAddress.text; }
    }

    public override void Show()
    {
        base.Show();
        if (inputNetworkAddress != null)
            inputNetworkAddress.text = DefaultNetworkAddress;
    }
}
