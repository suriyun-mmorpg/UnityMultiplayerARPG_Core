using UnityEngine;

namespace MultiplayerARPG
{
    public class SimpleVehicleSeatUIActivator : MonoBehaviour
    {
        [Tooltip("Default controls shown on foot and hidden in any vehicle seat. Keep this component outside its targets.")]
        public GameObject[] deactivateObjects = new GameObject[0];
        [Tooltip("Shown in the driver seat (seat 0). Shared vehicle UI can also be added to Passenger Activate Objects.")]
        public GameObject[] driverActivateObjects = new GameObject[0];
        [Tooltip("Shown in any passenger seat. Shared vehicle UI can also be added to Driver Activate Objects.")]
        public GameObject[] passengerActivateObjects = new GameObject[0];
        private BasePlayerCharacterEntity _previousEntity;

        private void OnEnable()
        {
            GameInstance.OnSetPlayingCharacterEvent += GameInstance_OnSetPlayingCharacter;
            GameInstance_OnSetPlayingCharacter(GameInstance.PlayingCharacterEntity);
        }

        private void OnDisable()
        {
            GameInstance.OnSetPlayingCharacterEvent -= GameInstance_OnSetPlayingCharacter;
            GameInstance_OnSetPlayingCharacter(null);
        }

        private void GameInstance_OnSetPlayingCharacter(IPlayerCharacterData playingCharacterData)
        {
            if (_previousEntity != null)
                _previousEntity.onSetPassengingVehicle -= Character_onSetPassengingVehicle;
            _previousEntity = playingCharacterData as BasePlayerCharacterEntity;
            if (_previousEntity != null)
                _previousEntity.onSetPassengingVehicle += Character_onSetPassengingVehicle;
            Refresh();
        }

        private void Character_onSetPassengingVehicle(BaseGameEntity target)
        {
            Refresh();
        }

        public void Refresh()
        {
            bool mounted = false;
            bool driving = false;
            if (_previousEntity != null)
            {
                IVehicleEntity vehicle = _previousEntity.PassengingVehicleEntity;
                byte seatIndex = _previousEntity.PassengingVehicleSeatIndex;
                mounted = !vehicle.IsNull() && vehicle.Seats != null && seatIndex < vehicle.Seats.Count;
                driving = mounted && vehicle.IsDriver(seatIndex);
            }
            SetObjectsActive(deactivateObjects, mounted, driving);
            SetObjectsActive(driverActivateObjects, mounted, driving);
            SetObjectsActive(passengerActivateObjects, mounted, driving);
        }

        private void SetObjectsActive(GameObject[] objects, bool mounted, bool driving)
        {
            if (objects == null)
                return;
            foreach (GameObject obj in objects)
            {
                // Keep the observer running so it can restore controls after leaving the vehicle.
                if (obj == null || transform.IsChildOf(obj.transform))
                    continue;
                // Resolve all memberships before applying, so shared panels never toggle off on seat changes.
                bool active = (!mounted && Contains(deactivateObjects, obj)) ||
                    (driving && Contains(driverActivateObjects, obj)) ||
                    (mounted && !driving && Contains(passengerActivateObjects, obj));
                if (obj.activeSelf != active)
                    obj.SetActive(active);
            }
        }

        private static bool Contains(GameObject[] objects, GameObject target)
        {
            return objects != null && System.Array.IndexOf(objects, target) >= 0;
        }
    }
}
