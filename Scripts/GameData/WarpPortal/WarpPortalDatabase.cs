using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Warp Portal Database", menuName = "Create GameData/Warp Portal Database")]
    public class WarpPortalDatabase : ScriptableObject
    {
        public WarpPortals[] maps;
    }
}
