using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UtilsComponents
{
    public class RepeatingEvent : MonoBehaviour
    {
        public float firstDelay;
        public float repeatDelay;
        public UnityEvent repeating;

        private void Start()
        {
            InvokeRepeating(nameof(Repeating), firstDelay, repeatDelay);
        }

        private void Repeating()
        {
            repeating.Invoke();
        }
    }
}
