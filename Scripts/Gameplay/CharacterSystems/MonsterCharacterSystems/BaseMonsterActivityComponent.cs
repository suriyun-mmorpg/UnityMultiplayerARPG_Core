using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseMonsterActivityComponent : BaseGameEntityComponent<BaseMonsterCharacterEntity>
    {
        public MonsterCharacter MonsterDatabase
        {
            get { return CacheEntity.MonsterDatabase; }
        }
    }
}
