using UnityEngine;

namespace MultiplayerARPG
{
    public class SetLayerFollowGameObject : MonoBehaviour
    {
        public GameObject source;
        public bool setChildrenLayersRecursively = true;
        private int dirtyLayer;

        private void LateUpdate()
        {
            if (source.layer != dirtyLayer)
            {
                dirtyLayer = source.layer;
                gameObject.layer = dirtyLayer;
                if (setChildrenLayersRecursively)
                    gameObject.SetLayerRecursively(dirtyLayer, true);
            }
        }
    }
}