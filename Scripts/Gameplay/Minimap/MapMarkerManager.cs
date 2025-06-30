using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class MapMarkerManager
    {
        private static Dictionary<string, IMapMarker> s_allMarkers = new Dictionary<string, IMapMarker>();
        public static IReadOnlyDictionary<string, IMapMarker> AllMarkers => s_allMarkers;
        public static event System.Action<IMapMarker> OnAdded;
        public static event System.Action<IMapMarker> OnRemoved;
        
        public static void AddMarker(IMapMarker mapMarker)
        {
            if (!s_allMarkers.ContainsKey(mapMarker.MapMarkerId))
            {
                s_allMarkers.Add(mapMarker.MapMarkerId, mapMarker);
                OnAdded?.Invoke(mapMarker);
            }
        }

        public static void RemoveMarker(IMapMarker mapMarker)
        {
            if (s_allMarkers.Remove(mapMarker.MapMarkerId))
            {
                OnRemoved?.Invoke(mapMarker);
            }
        }
    }
}
