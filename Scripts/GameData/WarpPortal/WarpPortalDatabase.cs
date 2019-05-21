using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Warp Portal Database", menuName = "Create GameDatabase/Warp Portal Database", order = -5898)]
    public class WarpPortalDatabase : ScriptableObject
    {
        public WarpPortals[] maps;
    }
}
