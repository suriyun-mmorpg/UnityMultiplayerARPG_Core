using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseDayNightTimeUpdater : ScriptableObject
    {
        public float TimeOfDay { get; protected set; }

        /// <summary>
        /// Init day of time, this function will be called at server to init time 
        /// </summary>
        /// <returns>Current time of day (0-24)</returns>
        public abstract void Init(BaseGameNetworkManager manager);

        /// <summary>
        /// Update time of day, this function will be called at server to update time of day by delta time (or other time system up to how developer will implement)
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns>Current time of day (0-24)</returns>
        public abstract void Update(float deltaTime);
    }
}
