using UnityEngine;

namespace MultiplayerARPG
{
    public class UITimer : MonoBehaviour
    {
        public TextWrapper textHours;
        public TextWrapper textMinutes;
        public TextWrapper textSeconds;
        public TextWrapper textMilliseconds;
        public TextWrapper textAll;
        public string allFormat = "{0}:{1}:{2}.{3}";

        private void OnEnable()
        {
            if (textHours != null)
                textHours.gameObject.SetActive(true);

            if (textMinutes != null)
                textMinutes.gameObject.SetActive(true);

            if (textSeconds != null)
                textSeconds.gameObject.SetActive(true);

            if (textMilliseconds != null)
                textMilliseconds.gameObject.SetActive(true);

            if (textAll != null)
                textAll.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (textHours != null)
                textHours.gameObject.SetActive(false);

            if (textMinutes != null)
                textMinutes.gameObject.SetActive(false);

            if (textSeconds != null)
                textSeconds.gameObject.SetActive(false);

            if (textMilliseconds != null)
                textMilliseconds.gameObject.SetActive(false);

            if (textAll != null)
                textAll.gameObject.SetActive(false);
        }

        public void UpdateTime(float seconds)
        {
            float hrs = Mathf.FloorToInt(seconds / 60f / 60f);
            float remainsSecFromHrs = seconds - (hrs * 60f * 60f);
            float min = Mathf.FloorToInt(remainsSecFromHrs / 60f);
            float secWithMilli = seconds % 60f;
            float sec = Mathf.FloorToInt(secWithMilli);
            float milli = (secWithMilli - sec) * 100;

            if (textHours != null)
                textHours.text = string.Format("{0:00}", hrs);

            if (textMinutes != null)
                textMinutes.text = string.Format("{0:00}", min);

            if (textSeconds != null)
                textSeconds.text = string.Format("{0:00}", sec);

            if (textMilliseconds != null)
                textMilliseconds.text = string.Format("{0:00}", milli);

            if (textAll != null)
                textAll.text = string.Format(allFormat, string.Format("{0:00}", hrs), string.Format("{0:00}", min), string.Format("{0:00}", sec), string.Format("{0:00}", milli));
        }
    }
}
