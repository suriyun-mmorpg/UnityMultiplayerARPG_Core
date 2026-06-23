namespace MultiplayerARPG
{
    public partial class BuildingEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                buildingTypes?.Clear();
                droppingItems?.Clear();
                repairs?.Clear();
                onBuildingDestroy?.RemoveAllListeners();
                onBuildingDestroy = null;
                onBuildingConstruct?.RemoveAllListeners();
                onBuildingConstruct = null;
                BuildingTypes?.Clear();
                BuildingArea = null;
                Builder = null;
                _cacheRepairs?.Clear();
                _buildingMaterials?.Clear();
            }
            _triggerObjects?.Clear();
            _children?.Clear();
            _lastAddedTriggerObjectFrame = 0;
            _parentFound = true;
            _isDestroyed = false;
        }
    }
}
