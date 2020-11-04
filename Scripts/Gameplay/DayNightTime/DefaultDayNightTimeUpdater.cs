using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Default Day Night Time Updater", menuName = "Create DayNightTimeUpdater/Default Day Night Time Updater", order = -2099)]
    public class DefaultDayNightTimeUpdater : BaseDayNightTimeUpdater
    {
        [SerializeField]
        [Tooltip("If this value is 60 it will turn to a new day in 60 seconds")]
        [Min(10)]
        private float secondsToOneDay = 60;
        [Range(0, 23)]
        private int startHourOfDay = 12;

        public override void InitTimeOfDay(BaseGameNetworkManager manager)
        {
            // Initial time of day based on real world time of day
            double totalSeconds = DateTime.Now.TimeOfDay.TotalSeconds;
            TimeOfDay = ((float)(totalSeconds / (secondsToOneDay / 24f)) % 24f) + startHourOfDay;
            TimeOfDay %= 24;
        }

        public override void UpdateTimeOfDay(float deltaTime)
        {
            TimeOfDay += Time.deltaTime / (secondsToOneDay / 24f);
            TimeOfDay %= 24; // Modulus to ensure always between 0-24
        }
    }
}
