using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class SimpleLodGroupGenerator : MonoBehaviour
    {
        public float screenRelativeTransitionHeight = 1f;
        public bool generateOnAwake;

        private void Awake()
        {
            if (generateOnAwake)
                Generate();
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                Debug.LogWarning("No renderers so it will not generate LOD group");
                return;
            }
            LODGroup tempLODGroup = GetComponent<LODGroup>();
            if (tempLODGroup == null)
                tempLODGroup = gameObject.AddComponent<LODGroup>();

            var lods = new LOD[1];
            lods[0] = new LOD(screenRelativeTransitionHeight, renderers);
            tempLODGroup.SetLODs(lods);
        }
    }
}
