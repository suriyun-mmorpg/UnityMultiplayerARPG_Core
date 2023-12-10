using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct UIBuffRemovalTextPair
    {
        public BuffRemoval removal;
        public TextWrapper uiText;
        public Image imageIcon;
        public GameObject root;
    }
}
