using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIStatusEffectApplyings : UIBase
    {
        public UIStatusEffectApplying uiDialog;
        public UIStatusEffectApplying uiPrefab;
        public Transform uiContainer;

        private UIList _cacheList;
        public UIList CacheList
        {
            get
            {
                if (_cacheList == null)
                {
                    _cacheList = gameObject.AddComponent<UIList>();
                    _cacheList.uiPrefab = uiPrefab.gameObject;
                    _cacheList.uiContainer = uiContainer;
                }
                return _cacheList;
            }
        }

        private UIStatusEffectApplyingSelectionManager _cacheSelectionManager;
        public UIStatusEffectApplyingSelectionManager CacheSelectionManager
        {
            get
            {
                if (_cacheSelectionManager == null)
                    _cacheSelectionManager = gameObject.GetOrAddComponent<UIStatusEffectApplyingSelectionManager>();
                _cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return _cacheSelectionManager;
            }
        }

        public UIStatusEffectApplyingTarget target;
        private UISelectionManagerShowOnSelectEventManager<UIStatusEffectApplyingData, UIStatusEffectApplying> _listEventSetupManager = new UISelectionManagerShowOnSelectEventManager<UIStatusEffectApplyingData, UIStatusEffectApplying>();

        protected virtual void OnEnable()
        {
            _listEventSetupManager.OnEnable(CacheSelectionManager, uiDialog);
        }

        protected virtual void OnDisable()
        {
            _listEventSetupManager.OnDisable();
        }

        public virtual void UpdateData(IEnumerable<StatusEffectApplying> statusEffectApplyings, int level, UIStatusEffectApplyingTarget target)
        {
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();
            CacheList.HideAll();
            CacheList.Generate(statusEffectApplyings, (index, data, ui) =>
            {
                UIStatusEffectApplying uiComp = ui.GetComponent<UIStatusEffectApplying>();
                uiComp.Data = new UIStatusEffectApplyingData(data, level, target);
                uiComp.Show();
                CacheSelectionManager.Add(uiComp);
                if (index == 0)
                    uiComp.SelectByManager();
            });
        }
    }
}