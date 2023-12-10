using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct UIStatusEffectResistanceTextPair
    {
        public StatusEffect statusEffect;
        public TextWrapper uiText;
        public Image imageIcon;
        public GameObject root;
    }
}
