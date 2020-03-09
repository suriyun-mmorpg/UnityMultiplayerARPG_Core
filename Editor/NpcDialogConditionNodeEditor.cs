using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

namespace MultiplayerARPG
{
    [CustomNodeEditor(typeof(NpcDialogConditionNode))]
    public class NpcDialogConditionNodeEditor : NodeEditor
    {
        public override Color GetTint()
        {
            return new Color(0.3f, 0.3f, 0.3f);
        }

        public override int GetWidth()
        {
            return 300;
        }
    }
}
