using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PrepareActionDurationsBehaviour : StateMachineBehaviour
    {
        [Tooltip("This will be in used with attacking/skill animations, This is rate of total animation duration at when it should hit enemy or apply skill")]
        [Range(0f, 1f)]
        public float[] triggerDurationRates = new float[0];
        private CharacterActionComponentManager _actionManager;
        private static readonly List<float> s_triggerDurations = new List<float>();

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_actionManager == null)
                _actionManager = animator.gameObject.GetComponentInParent<CharacterActionComponentManager>();
            s_triggerDurations.Clear();
            if (triggerDurationRates != null && triggerDurationRates.Length > 0)
            {
                for (int i = 0; i < triggerDurationRates.Length; ++i)
                {
                    s_triggerDurations.Add(triggerDurationRates[i] * stateInfo.length);
                }
            }
            else
            {
                s_triggerDurations.Add(stateInfo.length);
            }
            _actionManager.SetPreparingActionDurations(s_triggerDurations.ToArray(), stateInfo.length);
        }
    }
}
