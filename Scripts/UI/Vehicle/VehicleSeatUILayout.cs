using UnityEngine;

namespace MultiplayerARPG
{
    public static class VehicleSeatUILayout
    {
        public static Vector2 GetNormalizedPosition(IVehicleEntity vehicle, int seatIndex)
        {
            int count = vehicle.Seats.Count;
            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            Vector2 position = Vector2.zero;
            for (int i = 0; i < count; ++i)
            {
                Transform seatTransform = vehicle.Seats[i]?.passengingTransform;
                if (seatTransform == null)
                    return GetFallbackPosition(count, seatIndex);
                Vector3 local = vehicle.Entity.transform.InverseTransformPoint(seatTransform.position);
                Vector2 point = new Vector2(local.x, local.z);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
                if (i == seatIndex)
                    position = point;
            }
            Vector2 span = max - min;
            if (span.sqrMagnitude < 0.0001f)
                return GetFallbackPosition(count, seatIndex);
            return new Vector2(span.x > 0.0001f ? (position.x - min.x) / span.x - 0.5f : 0f,
                span.y > 0.0001f ? (position.y - min.y) / span.y - 0.5f : 0f);
        }

        public static Vector2 GetFallbackPosition(int count, int seatIndex)
        {
            if (count <= 1)
                return Vector2.zero;
            int rows = (count + 1) / 2;
            return new Vector2(seatIndex % 2 == 0 ? -0.5f : 0.5f,
                rows == 1 ? 0f : 0.5f - (float)(seatIndex / 2) / (rows - 1));
        }
    }
}
