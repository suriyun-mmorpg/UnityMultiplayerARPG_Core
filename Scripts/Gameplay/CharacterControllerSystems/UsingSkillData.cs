using UnityEngine;

namespace MultiplayerARPG
{
    public struct UsingSkillData
    {
        public Vector3? position;
        public int skillIndex;
        public UsingSkillData(Vector3? position, int skillIndex)
        {
            this.position = position;
            this.skillIndex = skillIndex;
        }
    }
}
