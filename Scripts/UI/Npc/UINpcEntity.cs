using UnityEngine;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Canvas))]
    public class UINpcEntity : UIBaseGameEntity<NpcEntity>
    {
        public float visibleDistance = 30f;
        private BasePlayerCharacterEntity tempOwningCharacter;

        private Canvas cacheCanvas;
        public Canvas CacheCanvas
        {
            get
            {
                if (cacheCanvas == null)
                    cacheCanvas = GetComponent<Canvas>();
                return cacheCanvas;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            CacheCanvas.enabled = false;
        }

        protected override void UpdateUI()
        {
            Profiler.BeginSample("UINpcEntity - Update UI");
            if (Data == null || BasePlayerCharacterController.OwningCharacter == null)
            {
                CacheCanvas.enabled = false;
                return;
            }

            tempOwningCharacter = BasePlayerCharacterController.OwningCharacter;
            NpcEntity targetNpc;
            if (Vector3.Distance(tempOwningCharacter.CacheTransform.position, Data.CacheTransform.position) > visibleDistance)
                CacheCanvas.enabled = false;
            else if (tempOwningCharacter.TryGetTargetEntity(out targetNpc) && targetNpc.ObjectId == Data.ObjectId)
                CacheCanvas.enabled = true;
            else
                CacheCanvas.enabled = false;
            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
        }
    }
}
