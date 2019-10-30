using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Npc Dialog Graph", menuName = "Create GameData/Npc Dialog Graph", order = -4797)]
    public class NpcDialogGraph : NodeGraph
    {
        public List<NpcDialog> GetDialogs()
        {
            List<NpcDialog> dialogs = new List<NpcDialog>();
            if (nodes != null && nodes.Count > 0)
            {
                foreach (Node node in nodes)
                {
                    dialogs.Add(node as NpcDialog);
                }
            }
            return dialogs;
        }

        private void OnValidate()
        {
            SetDialogName();
        }

        public void SetDialogName()
        {
            if (nodes != null && nodes.Count > 0)
            {
                for (int i = 0; i < nodes.Count; ++i)
                {
                    nodes[i].name = name + " " + i;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(nodes[i]);
#endif
                }
            }
            EditorUtility.SetDirty(this);
        }

        public override Node AddNode(System.Type type)
        {
            if (type == typeof(NpcDialog))
            {
                NpcDialog npcDialog = AddNode<NpcDialog>();
                SetDialogName();
            }
            return base.AddNode(type);
        }

        public override Node CopyNode(Node original)
        {
            if (original is NpcDialog)
            {
                NpcDialog npcDialog = AddNode<NpcDialog>();
                CopyNode(original as NpcDialog, npcDialog);
                SetDialogName();
            }
            return base.CopyNode(original);
        }

        public NpcDialog CopyNode(NpcDialog from, NpcDialog to)
        {
            to.title = from.title;
            to.titles = from.titles;
            to.description = from.description;
            to.descriptions = from.descriptions;
            to.icon = from.icon;
            to.type = from.type;
            to.menus = new List<NpcDialogMenu>(from.menus).ToArray();
            to.quest = from.quest;
            to.questAcceptedDialog = from.questAcceptedDialog;
            to.questDeclinedDialog = from.questDeclinedDialog;
            to.questAbandonedDialog = from.questAbandonedDialog;
            to.questCompletedDialog = from.questCompletedDialog;
            to.sellItems = new List<NpcSellItem>(from.sellItems).ToArray();
            to.itemCraft = from.itemCraft;
            to.craftDoneDialog = from.craftDoneDialog;
            to.craftItemWillOverwhelmingDialog = from.craftItemWillOverwhelmingDialog;
            to.craftNotMeetRequirementsDialog = from.craftNotMeetRequirementsDialog;
            to.craftCancelDialog = from.craftCancelDialog;
            to.saveRespawnMap = from.saveRespawnMap;
            to.saveRespawnPosition = from.saveRespawnPosition;
            to.saveRespawnConfirmDialog = from.saveRespawnConfirmDialog;
            to.saveRespawnCancelDialog = from.saveRespawnCancelDialog;
            to.warpPortalType = from.warpPortalType;
            to.warpMap = from.warpMap;
            to.warpPosition = from.warpPosition;
            to.warpCancelDialog = from.warpCancelDialog;
            to.storageCancelDialog = from.storageCancelDialog;
            return to;
        }

        public override NodeGraph Copy()
        {
            NpcDialogGraph graph = base.Copy() as NpcDialogGraph;
            graph.SetDialogName();
            return graph;
        }
    }
}
