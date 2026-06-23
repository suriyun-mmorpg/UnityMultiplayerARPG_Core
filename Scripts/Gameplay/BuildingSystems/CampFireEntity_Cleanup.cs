namespace MultiplayerARPG
{
    public partial class CampFireEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                onInitialTurnOn?.RemoveAllListeners();
                onInitialTurnOn = null;
                onInitialTurnOff?.RemoveAllListeners();
                onInitialTurnOff = null;
                onTurnOn?.RemoveAllListeners();
                onTurnOn = null;
                onTurnOff?.RemoveAllListeners();
                onTurnOff = null;
                _cacheFuelItems?.Clear();
                _cacheFuelItems = null;
                _cacheConvertItems?.Clear();
                _cacheConvertItems = null;
            }
            _convertRemainsDuration?.Clear();
            _preparedConvertItems?.Clear();
            _convertCountDown = 1f;
        }
    }
}
