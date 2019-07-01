using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class VehicleEntityExtension
    {
        public static bool IsDriver(this IVehicleEntity vehicleEntity, byte seatIndex)
        {
            // Only first seat is driver
            return vehicleEntity != null && seatIndex == 0;
        }

        public static bool IsDestroyWhenExit(this IVehicleEntity vehicleEntity, byte seatIndex)
        {
            return vehicleEntity.IsDriver(seatIndex) && vehicleEntity.IsDestroyWhenDriverExit;
        }
    }
}