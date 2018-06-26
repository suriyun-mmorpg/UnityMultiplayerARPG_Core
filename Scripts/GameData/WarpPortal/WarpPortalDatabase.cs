using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "WarpPortalDatabase", menuName = "Create GameData/WarpPortalDatabase")]
    public class WarpPortalDatabase : ScriptableObject
    {
        public WarpPortals[] maps;
    }
}
