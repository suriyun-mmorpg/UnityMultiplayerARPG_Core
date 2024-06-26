using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace MultiplayerARPG
{
    public class EditorSettings : ScriptableObject
    {
        public GameDatabase workingDatabase;
        public string[] socketEnhancerTypeTitles = new string[0];
        [Header("Addressable Assets")]
        public string clientBuildProfileName;
        public List<AddressableAssetGroup> clientAddressableGroups = new List<AddressableAssetGroup>();
        public string serverBuildProfileName;
        public List<AddressableAssetGroup> serverAddressableGroups = new List<AddressableAssetGroup>();
    }
}