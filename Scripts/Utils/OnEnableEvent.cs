using UnityEngine;
using UnityEngine.Events;

namespace UtilsComponents
{
    public class OnEnableEvent : MonoBehaviour
    {
        public UnityEvent onEnable;

        protected virtual void OnEnable()
        {
            onEnable.Invoke();
        }
    }
}
