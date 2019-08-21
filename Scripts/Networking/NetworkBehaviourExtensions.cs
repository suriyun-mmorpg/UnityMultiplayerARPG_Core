using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static partial class NetworkBehaviourExtensions
    {
        public static bool TryGetEntityByObjectId<T>(this LiteNetLibBehaviour behaviour, uint objectId, out T result) where T : class
        {
            result = null;
            LiteNetLibIdentity identity;
            if (!behaviour.Manager.Assets.TryGetSpawnedObject(objectId, out identity))
                return false;

            result = identity.GetComponent<T>();
            if (result == null)
                return false;

            return true;
        }
    }
}
