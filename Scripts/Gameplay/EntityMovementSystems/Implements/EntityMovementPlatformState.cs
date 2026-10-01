using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>Vehicle-relative feet position, in addition to the world-space movement fallback.</summary>
    public struct EntityMovementPlatformState
    {
        public uint objectId;
        public Vector3 localPosition;

        public void Write(NetDataWriter writer)
        {
            writer.PutPackedUInt(objectId);
            if (objectId != 0) writer.PutVector3(localPosition);
        }

        public static EntityMovementPlatformState Read(NetDataReader reader)
        {
            uint id = reader.GetPackedUInt();
            return new EntityMovementPlatformState { objectId = id,
                localPosition = id != 0 ? reader.GetVector3() : Vector3.zero };
        }

        public bool TryResolve(LiteNetLibGameManager manager, out Transform support, bool validateBounds = false)
        {
            support = null;
            if (objectId == 0 || manager == null ||
                !manager.TryGetEntityByObjectId(objectId, out BaseGameEntity entity) ||
                !(entity is IVehicleEntity) || !entity.gameObject.activeInHierarchy ||
                !IsFinite(localPosition.x) || !IsFinite(localPosition.y) || !IsFinite(localPosition.z))
                return false;
            support = entity.EntityTransform;
            if (!validateBounds) return true;
            // Check bounds on receipt, not every render frame (vehicle bounds can require collider enumeration).
            // Client-reported support must be near the vehicle, never an arbitrary remote attachment.
            if (entity.Movement == null) { support = null; return false; }
            Bounds bounds = entity.Movement.GetMovementBounds();
            bounds.Expand(2f);
            if (!bounds.Contains(support.TransformPoint(localPosition))) { support = null; return false; }
            return true;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
