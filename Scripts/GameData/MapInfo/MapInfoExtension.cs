using UnityEngine;
using System.Collections;

namespace MultiplayerARPG
{
    public static class MapInfoExtension
    {
        public static bool IsSceneSet(this MapInfo mapInfo)
        {
            return mapInfo != null && mapInfo.scene != null && mapInfo.scene.IsSet();
        }

        public static string GetSceneName(this MapInfo mapInfo)
        {
            if (mapInfo.IsSceneSet())
                return mapInfo.scene.SceneName;
            return string.Empty;
        }
    }
}
