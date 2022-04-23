using UnityEngine;
using UnityEngine.Events;

namespace UtilsComponents
{
    public class OnDisableEvent : MonoBehaviour
    {
        public UnityEvent onDisable;

        private void OnDisable()
        {
            onDisable.Invoke();
        }
    }
}
