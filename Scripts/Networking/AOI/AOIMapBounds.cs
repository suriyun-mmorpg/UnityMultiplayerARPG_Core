using UnityEngine;

namespace MultiplayerARPG
{
    public class AOIMapBounds : MonoBehaviour
    {
        public Bounds bounds = default;

#if UNITY_EDITOR
        public Color gizmosColor = Color.cyan;
        private void OnDrawGizmos()
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = gizmosColor;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.color = prevColor;
        }
#endif
    }
}
