using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PlayerCharacterBlendshapeComponent : BaseGameEntityComponent<BasePlayerCharacterEntity>
    {
        [System.Serializable]
        public struct Setting
        {
            public string settingId;
            public string blendshapeName;
            [Range(0f, 100f)]
            public float defaultValue;
        }

        public SkinnedMeshRenderer skinnedMeshRenderer;
        public Setting[] settings = new Setting[0];
        private bool _setupProperly;
        private bool _applying;
        private float[] _currentValues;
        private Dictionary<string, int> _blendshapeIndexesByName = new Dictionary<string, int>();

        public override void EntityAwake()
        {
            if (skinnedMeshRenderer == null)
                return;
            _applying = true;
            _setupProperly = true;
            _currentValues = new float[settings.Length];
            for (int i = 0; i < settings.Length; ++i)
            {
                _currentValues[i] = settings[i].defaultValue;
            }
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; ++i)
            {
                _blendshapeIndexesByName[skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i)] = i;
            }
        }

        public override void EntityUpdate()
        {
            if (_applying)
            {
                Apply();
                _applying = false;
            }
        }

        public void SetData(int optionIndex, float value)
        {
            if (!_setupProperly || optionIndex < 0 || optionIndex >= _currentValues.Length)
                return;
            _currentValues[optionIndex] = value;
            _applying = true;
            // TODO: Save to entity's `PublicFloats`
        }

        public float GetData(int optionIndex)
        {
            if (!_setupProperly || optionIndex < 0 || optionIndex >= _currentValues.Length)
                return 0f;
            return _currentValues[optionIndex];
        }

        public void Apply()
        {
            for (int i = 0; i < settings.Length; ++i)
            {
                if (!_blendshapeIndexesByName.TryGetValue(settings[i].blendshapeName, out int blendshapeIndex))
                    continue;
                skinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndex, _currentValues[i]);
            }
        }
    }
}
