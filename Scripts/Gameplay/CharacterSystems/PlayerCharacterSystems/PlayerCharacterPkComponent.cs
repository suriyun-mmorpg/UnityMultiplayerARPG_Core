using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class PlayerCharacterPkComponent : BaseNetworkedGameEntityComponent<BasePlayerCharacterEntity>
    {
        public void TogglePkMode()
        {
            RPC(ServerTogglePkMode);
        }

        [ServerRpc]
        protected void ServerTogglePkMode()
        {
            if (Entity.IsPkOn)
            {
                // Turn off
                Entity.IsPkOn = false;
                Entity.PkPoint = 0;
                Entity.ConsecutivePkKills = 0;
            }
            else
            {
                // Turn on
                Entity.IsPkOn = true;
                Entity.LastPkOnTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }
    }
}