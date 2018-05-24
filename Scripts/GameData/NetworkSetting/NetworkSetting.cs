using UnityEngine;

[CreateAssetMenu(fileName = "NetworkSetting", menuName = "Create NetworkSetting/NetworkSetting")]
public class NetworkSetting : ScriptableObject
{
    public string networkAddress = "127.0.0.1";
    public int networkPort = 7770;
    public int maxConnections = 4;
}
