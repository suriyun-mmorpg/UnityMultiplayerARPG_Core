using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanGameServiceConnection : BaseGameServiceConnection
{
    public string networkAddress = "127.0.0.1";
    public int networkPort = 7770;
    public int maxConnections = 4;
}
