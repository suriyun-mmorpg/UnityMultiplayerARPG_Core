using UnityEngine;
using XNode;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract partial class BaseNpcDialog : Node
    {
        [Input]
        public BaseNpcDialog input;
        [Header("NPC Dialog Configs")]
        [Tooltip("Default title")]
        public string title;
        [Tooltip("Titles by language keys")]
        public LanguageData[] titles;
        [Tooltip("Default description")]
        [TextArea]
        public string description;
        [Tooltip("Descriptions by language keys")]
        public LanguageData[] descriptions;
        public Sprite icon;

        #region Generic Data
        public string Id { get { return name; } }
        public string Title
        {
            get { return Language.GetText(titles, title); }
        }
        public string Description
        {
            get { return Language.GetText(descriptions, description); }
        }
        public int DataId { get { return MakeDataId(Id); } }

        public static int MakeDataId(string id)
        {
            return id.GenerateHashId();
        }
        #endregion

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (Validate())
                EditorUtility.SetDirty(this);
        }
#endif

        public virtual bool Validate()
        {
            return false;
        }

        public virtual void PrepareRelatesData()
        {

        }

        public virtual void RenderUI()
        {

        }

        public override object GetValue(NodePort port)
        {
            return port.node;
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            SetDialogByPort(from, to);
        }

        public override void OnRemoveConnection(NodePort port)
        {
            SetDialogByPort(port, null);
        }
        
        public abstract bool ValidateDialog(BasePlayerCharacterEntity characterEntity);
        public abstract NpcDialog GetNextDialog(BasePlayerCharacterEntity characterEntity, byte menuIndex);
        protected abstract void SetDialogByPort(NodePort from, NodePort to);
    }
}
