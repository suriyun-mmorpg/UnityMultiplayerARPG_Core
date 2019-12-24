using UnityEngine;

namespace MultiplayerARPG
{
    public partial class MonsterCharacterEntity : BaseMonsterCharacterEntity
    {
        public override void InitialRequiredComponents()
        {
            if (Movement == null)
                Debug.LogError("[" + ToString() + "] Did not setup entity movement component to this entity.");
        }
    }
}
