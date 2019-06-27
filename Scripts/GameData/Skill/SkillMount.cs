using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct SkillMount
    {
        [Tooltip("Leave `Mount Entity` to NULL to not summon mount entity")]
        public MountEntity mountEntity;
    }
}
