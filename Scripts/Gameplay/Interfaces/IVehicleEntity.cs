using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IVehicleEntity : IEntityMovement
    {
        VehicleType VehicleType { get; }
        bool IsDrivable { get; }
        List<VehicleSeat> Seats { get; }
    }
}
