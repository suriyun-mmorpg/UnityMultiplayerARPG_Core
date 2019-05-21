using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Network Setting", menuName = "Create NetworkSetting/Network Setting", order = -3999)]
    public class NetworkSetting : ScriptableObject
    {
        public string networkAddress = "127.0.0.1";
        public int networkPort = 7770;
        public int maxConnections = 4;
    }
}
