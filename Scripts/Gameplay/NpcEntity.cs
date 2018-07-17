using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterModel))]
    public sealed class NpcEntity : RpgNetworkEntity
    {
        [Tooltip("Set it to force to not change character model by data Id, when set it model container will not be used")]
        public NpcDialog startDialog;

        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = gameInstance.npcTag;
            gameObject.layer = gameInstance.characterLayer;
        }
    }
}
