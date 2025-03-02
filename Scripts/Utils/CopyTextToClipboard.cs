using UnityEngine;

namespace MultiplayerARPG
{
    public class CopyTextToClipboard : MonoBehaviour
    {
        public TextWrapper source;
        
        public void CopyToClipboard()
        {
            GUIUtility.systemCopyBuffer = source.text;
        }
    }
}
