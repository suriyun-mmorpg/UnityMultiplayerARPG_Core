using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct SkillMount
    {
        public static readonly SkillMount Empty = new SkillMount();
        [Tooltip("Leave `Mount Entity` to NULL to not summon mount entity")]
        [SerializeField]
        private VehicleEntity mountEntity;
        public VehicleEntity MountEntity { get { return mountEntity; } }

        public SkillMount(VehicleEntity mountEntity)
        {
            this.mountEntity = mountEntity;
        }
    }
}
