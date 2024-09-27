using UnityEngine;

namespace MultiplayerARPG
{
    public class SheathWhileUnderWater : MonoBehaviour
    {
        private BasePlayerCharacterEntity _entity;
        private bool _previouslyUnderWater = false;

        void Start()
        {
            _entity = GetComponent<BasePlayerCharacterEntity>();
            if (_entity == null)
                enabled = false;
        }

        void Update()
        {
            if (!_entity.IsOwnerClient)
                return;
            bool isUnderWater = _entity.MovementState.Has(MovementState.IsUnderWater);
            if (isUnderWater != _previouslyUnderWater || (isUnderWater && !_entity.IsWeaponsSheathed))
            {
                _entity.IsWeaponsSheathed = isUnderWater;
                _previouslyUnderWater = isUnderWater;
            }
        }
    }
}
