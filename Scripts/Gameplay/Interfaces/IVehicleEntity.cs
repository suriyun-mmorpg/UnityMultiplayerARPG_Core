using UnityEngine;

namespace MultiplayerARPG
{
    public interface IVehicleEntity : IGameEntity
    {
        Transform[] Seats { get; set; }
    }
}
