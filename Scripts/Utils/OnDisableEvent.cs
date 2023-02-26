using UnityEngine;
using UnityEngine.Events;

namespace UtilsComponents
{
    public class OnDisableEvent : MonoBehaviour
    {
        public UnityEvent onDisable;

        protected virtual void OnDisable()
        {
            onDisable.Invoke();
        }
    }
}
