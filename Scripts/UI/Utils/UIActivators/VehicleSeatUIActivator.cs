using UnityEngine;

namespace MultiplayerARPG
{
    public class VehicleSeatUIActivator : MonoBehaviour
    {
        [System.Serializable]
        public class VehicleSeatCondition
        {
            [Tooltip("Leave empty to match any vehicle type.")]
            public VehicleType vehicleType;
            [Tooltip("Use -1 for any seat, 0 for the driver, or another seat index for a passenger.")]
            [Min(-1)]
            public int seatIndex = -1;
        }

        [Tooltip("Match any listed condition. An empty list matches any vehicle and seat.")]
        public VehicleSeatCondition[] conditions = new VehicleSeatCondition[0];
        [Tooltip("Shown when a condition matches, hidden otherwise. Keep this component on an active UI object outside its targets.")]
        public GameObject[] activateObjects = new GameObject[0];
        [Tooltip("Hidden when a condition matches, shown otherwise. This component's GameObject and its ancestors cannot be targets.")]
        public GameObject[] deactivateObjects = new GameObject[0];

        private void OnEnable()
        {
            Refresh();
        }

        private void LateUpdate()
        {
            Refresh();
        }

        public void Refresh()
        {
            bool matches = false;
            BasePlayerCharacterEntity character = GameInstance.PlayingCharacterEntity;
            if (character != null)
            {
                IVehicleEntity vehicle = character.PassengingVehicleEntity;
                byte seatIndex = character.PassengingVehicleSeatIndex;
                if (!vehicle.IsNull() && vehicle.Seats != null && seatIndex < vehicle.Seats.Count)
                    matches = MatchesVehicleSeat(vehicle.VehicleType, seatIndex);
            }
            SetObjectsActive(activateObjects, matches);
            SetObjectsActive(deactivateObjects, !matches);
        }

        private bool MatchesVehicleSeat(VehicleType vehicleType, byte seatIndex)
        {
            if (conditions == null || conditions.Length == 0)
                return true;
            foreach (VehicleSeatCondition condition in conditions)
            {
                if (condition == null)
                    continue;
                bool matchesType = condition.vehicleType == null ||
                    (vehicleType != null && condition.vehicleType.DataId == vehicleType.DataId);
                if (matchesType && (condition.seatIndex == -1 || condition.seatIndex == seatIndex))
                    return true;
            }
            return false;
        }

        private void SetObjectsActive(GameObject[] objects, bool active)
        {
            if (objects == null)
                return;
            foreach (GameObject obj in objects)
            {
                // Keep the observer running so it can restore panels after leaving the vehicle.
                if (obj == null || transform.IsChildOf(obj.transform))
                    continue;
                if (obj.activeSelf != active)
                    obj.SetActive(active);
            }
        }
    }
}
