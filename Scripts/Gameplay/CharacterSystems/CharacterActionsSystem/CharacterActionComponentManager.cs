using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterActionComponentManager : MonoBehaviour
    {
        public delegate void PrepareActionDurationsResultDelegate(float[] triggerDurations, float totalDuration, float remainsDuration, float endTime);
        public delegate void PrepareActionDurationsDelegate(float[] triggerDurations, float totalDuration);

        public const float DEFAULT_TOTAL_DURATION = 1f;
        public const float DEFAULT_TRIGGER_DURATION = 0.5f;
        public const float STATE_PREPARING_DURATION = 1f;
        private PrepareActionDurationsDelegate _onSetPreparingActionDurations = null;

        public static float PrepareActionEndTime(float totalDuration, float animSpeedRate, float additionalTime = 0f)
        {
            if (totalDuration <= 0f)
                totalDuration = DEFAULT_TOTAL_DURATION;
            return Time.unscaledTime + (totalDuration / animSpeedRate) + additionalTime;
        }

        /// <summary>
        /// Calculate times for animation playing
        /// <param name="triggerDurations"></param>
        /// <param name="totalDuration"></param>
        /// <param name="totalDurationChange"></param>
        /// <param name="animSpeedRate"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="resultCallback"></param>
        /// <returns></returns>
        /// </summary>
        public async UniTask PrepareActionDurations(float[] triggerDurations, float totalDuration, float totalDurationChange, float animSpeedRate, CancellationToken cancellationToken, PrepareActionDurationsResultDelegate resultCallback)
        {
            float remainsDuration = totalDuration + totalDurationChange;
            // Try setup state data (maybe by animation clip events or state machine behaviours), if it was not set up
            if (triggerDurations == null || triggerDurations.Length == 0 || totalDuration < 0f)
            {
                _onSetPreparingActionDurations += (__triggerDurations, __totalDuration) =>
                {
                    triggerDurations = __triggerDurations;
                    totalDuration = __totalDuration;
                };
                // Wait some components to setup proper `triggerDurations` and `totalDuration` within `STATE_PREPARING_DURATION`
                float setupDelayCountDown = STATE_PREPARING_DURATION;
                do
                {
                    await UniTask.Yield(cancellationToken);
                    setupDelayCountDown -= Time.unscaledDeltaTime;
                } while (setupDelayCountDown > 0 && (triggerDurations == null || triggerDurations.Length == 0 || totalDuration < 0f));
                if (setupDelayCountDown > 0f)
                {
                    remainsDuration = totalDuration + totalDurationChange;
                }
                else
                {
                    // Can't setup properly, so try to setup manually to make it still workable
                    remainsDuration = DEFAULT_TOTAL_DURATION + totalDurationChange;
                    triggerDurations = new float[1]
                    {
                        DEFAULT_TRIGGER_DURATION,
                    };
                }
            }
            resultCallback?.Invoke(triggerDurations, totalDuration, remainsDuration, PrepareActionEndTime(remainsDuration, animSpeedRate));
        }

        public bool ShouldPrepareActionDurations()
        {
            return _onSetPreparingActionDurations != null;
        }

        public void PrepareActionDurations(float[] triggerDurations, float totalDuration)
        {
            _onSetPreparingActionDurations?.Invoke(triggerDurations, totalDuration);
            _onSetPreparingActionDurations = null;
        }
    }
}
