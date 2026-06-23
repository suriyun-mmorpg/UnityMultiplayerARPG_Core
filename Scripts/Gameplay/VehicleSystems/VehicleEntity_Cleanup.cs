namespace MultiplayerARPG
{
    public partial class VehicleEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                vehicleType = null;
                seats?.Clear();
                onVehicleDestroy?.RemoveAllListeners();
                onVehicleDestroy = null;
                Resistances?.Clear();
                Armors?.Clear();
            }
            _passengers?.Clear();
            _spawnEvents?.Clear();
            _isDestroyed = false;
            _cacheBuff?.Clear();
            _dirtyLevel = int.MinValue;
        }
    }
}