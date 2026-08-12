using System.Collections.Generic;

namespace MultiplayerARPG
{
    public struct UIEquipmentSocketsData
    {
        public CharacterItemSockets sockets;
        public SocketEnhancerType[] availableSocketEnhancerTypes;
        public UIEquipmentSocketsData(CharacterItemSockets sockets, SocketEnhancerType[] availableSocketEnhancerTypes)
        {
            this.sockets = sockets;
            this.availableSocketEnhancerTypes = availableSocketEnhancerTypes;
        }
    }
}
