using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class Companion : BaseGameData
    {
        [Header("Companion Configs")]
        [SerializeField]
        private BaseMonsterCharacterEntity companionEntity;
        public BaseMonsterCharacterEntity CompanionEntity
        {
            get { return companionEntity; }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddCharacterEntities(CompanionEntity);
        }
    }
}
