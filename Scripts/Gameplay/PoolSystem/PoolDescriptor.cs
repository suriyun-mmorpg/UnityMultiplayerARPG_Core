using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class PoolDescriptor : MonoBehaviour, IPoolDescriptor
    {
        public IPoolDescriptor ObjectPrefab { get; set; }

        [SerializeField]
        private int poolSize = 30;
        public int PoolSize { get { return poolSize; } set { poolSize = value; } }

        public UnityEvent onInitPrefab;
        public UnityEvent onGetInstance;

        public virtual void InitPrefab()
        {
            if (onInitPrefab != null)
                onInitPrefab.Invoke();
        }

        public virtual void OnGetInstance()
        {
            if (onGetInstance != null)
                onGetInstance.Invoke();
        }

        protected void PushBack(float delay)
        {
            PushBackRoutine(delay).Forget();
        }

        private async UniTaskVoid PushBackRoutine(float delay)
        {
            await UniTask.Delay((int)(delay * 1000));
            PushBack();
        }

        protected virtual void PushBack()
        {
            OnPushBack();
            PoolSystem.PushBack(this);
        }

        protected virtual void OnPushBack()
        {

        }
    }
}
