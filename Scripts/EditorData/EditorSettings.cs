using UnityEngine;

namespace MultiplayerARPG
{
    public class EditorSettings : ScriptableObject
    {
        public GameDatabase workingDatabase;
        public string[] socketEnhancerTypeTitles = new string[0];
    }
}