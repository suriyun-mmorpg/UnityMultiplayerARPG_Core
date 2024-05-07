using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterActionComponentManager : MonoBehaviour
    {
        public const float DEFAULT_TOTAL_DURATION = 2f;
        public const float DEFAULT_TRIGGER_DURATION = 1f;
        public const float DEFAULT_STATE_SETUP_DELAY = 1f;

        public float actionAcceptanceDuration = 0.05f;
        protected float _lastAcceptTime;
        protected float[] _preparingTriggerDurations;
        protected float _preparingTotalDuration;
        protected float _preparingTotalDurationChange;

        public bool IsAcceptNewAction()
        {
            return Time.unscaledTime - _lastAcceptTime > actionAcceptanceDuration;
        }

        public void ActionAccepted()
        {
            _lastAcceptTime = Time.unscaledTime;
        }

        public static float PrepareActionDefaultEndTime(float totalDuration, float animSpeedRate, float additionalTime = 0f)
        {
            if (totalDuration <= 0f)
                totalDuration = DEFAULT_TOTAL_DURATION;
            return Time.unscaledTime + (totalDuration / animSpeedRate) + additionalTime;
        }

        /// <summary>
        /// Calculate times for animation playing
        /// <param name="listener"></param>
        /// <param name="triggerDurations"></param>
        /// <param name="totalDuration"></param>
        /// <param name="totalDurationChange"></param>
        /// <param name="animSpeedRate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// </summary>
        public async UniTask PrepareActionDurations(ICharacterActionComponentPreparation listener, float[] triggerDurations, float totalDuration, float totalDurationChange, float animSpeedRate, CancellationToken cancellationToken)
        {
            float remainsDurationWithoutSpeedRate = totalDuration + totalDurationChange;
            // Try setup state data (maybe by animation clip events or state machine behaviours), if it was not set up
            if (triggerDurations == null || triggerDurations.Length == 0 || totalDuration < 0f)
            {
                _preparingTriggerDurations = triggerDurations;
                _preparingTotalDuration = totalDuration;
                _preparingTotalDurationChange = totalDurationChange;
                // Wait some components to setup proper `attackTriggerDurations` and `attackTotalDuration` within `DEFAULT_STATE_SETUP_DELAY`
                float setupDelayCountDown = DEFAULT_STATE_SETUP_DELAY;
                do
                {
                    await UniTask.Yield(cancellationToken);
                    setupDelayCountDown -= Time.unscaledDeltaTime;
                } while (setupDelayCountDown > 0 && (_preparingTriggerDurations == null || _preparingTriggerDurations.Length == 0 || _preparingTotalDuration < 0f));
                if (setupDelayCountDown > 0f)
                {
                    remainsDurationWithoutSpeedRate = _preparingTotalDuration + _preparingTotalDurationChange;
                    triggerDurations = _preparingTriggerDurations;
                }
                else
                {
                    // Can't setup properly, so try to setup manually to make it still workable
                    remainsDurationWithoutSpeedRate = DEFAULT_TOTAL_DURATION - DEFAULT_STATE_SETUP_DELAY;
                    triggerDurations = new float[1]
                    {
                        DEFAULT_TRIGGER_DURATION,
                    };
                }
            }
            listener?.OnPrepareActionDurations(triggerDurations, totalDuration, remainsDurationWithoutSpeedRate, PrepareActionDefaultEndTime(remainsDurationWithoutSpeedRate, animSpeedRate));
        }

        public void SetPreparingActionDurations(float[] preparingTriggerDurations, float preparingTotalDuration)
        {
            _preparingTriggerDurations = preparingTriggerDurations;
            _preparingTotalDuration = preparingTotalDuration;
        }
    }
}
