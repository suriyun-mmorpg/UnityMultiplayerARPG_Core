using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

namespace MultiplayerARPG
{
    [CustomNodeEditor(typeof(NpcDialog))]
    public class NpcDialogNodeEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            serializedObject.Update();

            NpcDialog node = target as NpcDialog;
            NodeEditorGUILayout.PortField(target.GetInputPort(node.GetMemberName(a => a.input)));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.title)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.titles)), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.description)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.descriptions)), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.icon)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.type)));
            switch (node.type)
            {
                case NpcDialogType.Normal:
                    NodeEditorGUILayout.DynamicPortList(node.GetMemberName(a => a.menus), typeof(NpcDialog), serializedObject, NodePort.IO.Output, Node.ConnectionType.Override);
                    break;
                case NpcDialogType.Quest:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.quest)));
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.questAcceptedDialog)));
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.questDeclinedDialog)));
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.questAbandonedDialog)));
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.questCompletedDialog)));
                    break;
                case NpcDialogType.Shop:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.sellItems)));
                    break;
                case NpcDialogType.CraftItem:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.itemCraft)));
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.craftDoneDialog)));
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.craftItemWillOverwhelmingDialog)));
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.craftNotMeetRequirementsDialog)));
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.craftCancelDialog)));
                    break;
                case NpcDialogType.SaveRespawnPoint:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.saveRespawnMap)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.saveRespawnPosition)));
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.saveRespawnConfirmDialog)));
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.saveRespawnCancelDialog)));
                    break;
                case NpcDialogType.Warp:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.warpPortalType)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.warpMap)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(node.GetMemberName(a => a.warpPosition)));
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.warpCancelDialog)));
                    break;
                case NpcDialogType.RefineItem:
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.refineItemCancelDialog)));
                    break;
                case NpcDialogType.PlayerStorage:
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.storageCancelDialog)));
                    break;
                case NpcDialogType.GuildStorage:
                    NodeEditorGUILayout.PortField(target.GetOutputPort(node.GetMemberName(a => a.storageCancelDialog)));
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        public override Color GetTint()
        {
            if (target != null && target.graph != null && target.graph.nodes[0] == target)
                return new Color(0.3f, 0.6f, 0.3f);
            return base.GetTint();
        }

        public override int GetWidth()
        {
            return 340;
        }
    }
}
