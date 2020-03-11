using UnityEngine;
using System.Collections;

namespace MultiplayerARPG
{
    public static class MapInfoExtension
    {
        public static bool IsSceneSet(this BaseMapInfo mapInfo)
        {
            return mapInfo != null && mapInfo.Scene != null && mapInfo.Scene.IsSet();
        }

        public static string GetSceneName(this BaseMapInfo mapInfo)
        {
            if (mapInfo.IsSceneSet())
                return mapInfo.Scene.SceneName;
            return string.Empty;
        }
    }
}
