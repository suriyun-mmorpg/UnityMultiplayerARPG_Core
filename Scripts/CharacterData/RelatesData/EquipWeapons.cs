using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class EquipWeapons
    {
        // NOTE: May add some functions later
    }

    [System.Serializable]
    public class SyncFieldEquipWeapons : LiteNetLibSyncField<EquipWeapons>
    {
        protected override bool IsValueChanged(EquipWeapons newValue)
        {
            return true;
        }
    }


    [System.Serializable]
    public class SyncListEquipWeapons : LiteNetLibSyncList<EquipWeapons>
    {
    }
}
