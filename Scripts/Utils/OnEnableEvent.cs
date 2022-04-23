using UnityEngine;
using UnityEngine.Events;

namespace UtilsComponents
{
    public class OnEnableEvent : MonoBehaviour
    {
        public UnityEvent onEnable;

        private void OnEnable()
        {
            onEnable.Invoke();
        }
    }
}
