using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IVehicleEntity : ICharacterMovement
    {
        VehicleType VehicleType { get; }
        bool IsDrivable { get; }
        List<Transform> Seats { get; }
    }
}
