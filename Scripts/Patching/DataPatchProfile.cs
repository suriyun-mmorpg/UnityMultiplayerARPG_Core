using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.DATA_PATCH_PROFILE_FILE, menuName = GameDataMenuConsts.DATA_PATCH_PROFILE_MENU, order = GameDataMenuConsts.DATA_PATCH_PROFILE_ORDER)]
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
