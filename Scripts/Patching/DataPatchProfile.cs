using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "DataPatchProfile", menuName = "Multiplayer ARPG/Data Patch Profile")]
    public sealed class DataPatchProfile : ScriptableObject
    {
        public GameDatabase database;
        public string patchDatabaseId = "";
        public DataPatchSettings dataPatchSettings = new DataPatchSettings();
        [Tooltip("Additional data or prefabs to include when scanning dependencies.")]
        public Object[] additionalRoots = new Object[0];
        [HideInInspector]
        public ScriptableObject[] scannedData = new ScriptableObject[0];
    }
}
