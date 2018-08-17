using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class GameEffectCollection
    {
        private static uint idCount = 0;
        protected uint? id;
        public uint Id
        {
            get { return !id.HasValue ? 0 : id.Value; }
        }

        public GameEffect[] effects;

        /// <summary>
        /// Initialize effect id, will return false if it's already initialized
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Initialize()
        {
            if (effects == null || effects.Length == 0 || id.HasValue)
                return false;

            ++idCount;
            id = idCount;
            return true;
        }

        public static void ResetId()
        {
            idCount = 0;
        }
    }
}
