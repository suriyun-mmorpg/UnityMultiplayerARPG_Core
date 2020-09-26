using System.Collections;
using System.Collections.Generic;
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
