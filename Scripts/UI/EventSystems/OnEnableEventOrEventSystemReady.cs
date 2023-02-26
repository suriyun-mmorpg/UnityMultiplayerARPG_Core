using UtilsComponents;

namespace MultiplayerARPG
{
    public class OnEnableEventOrEventSystemReady : OnEnableEvent
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            EventSystemManager.Instance.onEventSystemReady += Instance_onEventSystemReady;
        }

        private void OnDisable()
        {
            EventSystemManager.Instance.onEventSystemReady -= Instance_onEventSystemReady;
        }

        private void Instance_onEventSystemReady()
        {
            onEnable.Invoke();
        }
    }
}
