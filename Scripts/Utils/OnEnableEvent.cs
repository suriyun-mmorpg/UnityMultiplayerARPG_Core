using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UtilsComponents
{
    public class OnEnableEvent : MonoBehaviour
    {
        public UnityEvent onEnable = new UnityEvent();
        public float delay = 0f;

        protected virtual void OnEnable()
        {
            if (delay <= 0f)
                Trigger();
            else
                StartCoroutine(DelayTrigger(delay));
        }

        IEnumerator DelayTrigger(float delay)
        {
            yield return null;
            yield return new WaitForSeconds(delay);
            Trigger();
        }

        public void Trigger()
        {
            onEnable.Invoke();
        }
    }
}
