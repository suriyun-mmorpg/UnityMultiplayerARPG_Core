using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DefaultExecutionOrder(-900)]
    public class CharacterAlignOnGround : MonoBehaviour
    {
        public Transform rootBoneTransform;
        public Vector3 rootBoneRotation;
        public float alignOnGroundDistance = 1f;
        public LayerMask alignOnGroundLayerMask = ~0;
        public float alignSpeed = 20f;

        Quaternion aligningQuaternion;
        public Transform CacheTransform { get; private set; }
        private void Start()
        {
            CacheTransform = transform;
            aligningQuaternion = Quaternion.identity;
        }

        private void LateUpdate()
        {
            if (rootBoneTransform == null)
                return;

            RaycastHit raycastHit;
            if (Physics.Raycast(CacheTransform.position, Vector3.down, out raycastHit, alignOnGroundDistance, alignOnGroundLayerMask, QueryTriggerInteraction.Ignore))
                aligningQuaternion = Quaternion.Slerp(aligningQuaternion, Quaternion.FromToRotation(Vector3.up, raycastHit.normal), Time.deltaTime * alignSpeed);
            else
                aligningQuaternion = Quaternion.Slerp(aligningQuaternion, Quaternion.identity, Time.deltaTime * alignSpeed);
            rootBoneTransform.rotation = aligningQuaternion * Quaternion.AngleAxis(CacheTransform.eulerAngles.y, Vector3.up) * Quaternion.Euler(rootBoneRotation);
        }
    }
}
