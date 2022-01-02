using System.Collections.Generic;

namespace MultiplayerARPG
{
    public struct UIEquipmentSocketsData
    {
        public List<int> sockets;
        public int maxSocket;
        public UIEquipmentSocketsData(List<int> sockets, int maxSocket)
        {
            this.sockets = sockets;
            this.maxSocket = maxSocket;
        }
    }
}
