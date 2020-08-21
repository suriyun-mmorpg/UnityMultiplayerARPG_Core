using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(NpcDialog))]
    [CanEditMultipleObjects]
    public class NpcDialogEditor : BaseCustomEditor
    {
        private static NpcDialog cacheNpcDialog;
        protected override void SetFieldCondition()
        {
            if (cacheNpcDialog == null)
                cacheNpcDialog = CreateInstance<NpcDialog>();

            if ((target as NpcDialog).graph == null)
            {
                hiddenFields.Add("graph");
                hiddenFields.Add("position");
                hiddenFields.Add("ports");
            }
            hiddenFields.Add("input");

            // Normal
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.Normal), nameof(cacheNpcDialog.menus));
            // Quest
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.Quest), nameof(cacheNpcDialog.quest));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.Quest), nameof(cacheNpcDialog.questAcceptedDialog));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.Quest), nameof(cacheNpcDialog.questDeclinedDialog));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.Quest), nameof(cacheNpcDialog.questAbandonedDialog));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.Quest), nameof(cacheNpcDialog.questCompletedDialog));
            // Shop
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.Shop), nameof(cacheNpcDialog.sellItems));
            // Craft Item
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.CraftItem), nameof(cacheNpcDialog.itemCraft));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.CraftItem), nameof(cacheNpcDialog.craftDoneDialog));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.CraftItem), nameof(cacheNpcDialog.craftItemWillOverwhelmingDialog));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.CraftItem), nameof(cacheNpcDialog.craftNotMeetRequirementsDialog));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.CraftItem), nameof(cacheNpcDialog.craftCancelDialog));
            // Save Spawn Point
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.SaveRespawnPoint), nameof(cacheNpcDialog.saveRespawnMap));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.SaveRespawnPoint), nameof(cacheNpcDialog.saveRespawnPosition));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.SaveRespawnPoint), nameof(cacheNpcDialog.saveRespawnConfirmDialog));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.SaveRespawnPoint), nameof(cacheNpcDialog.saveRespawnCancelDialog));
            // Warp
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.Warp), nameof(cacheNpcDialog.warpPortalType));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.Warp), nameof(cacheNpcDialog.warpMap));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.Warp), nameof(cacheNpcDialog.warpPosition));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.Warp), nameof(cacheNpcDialog.warpCancelDialog));
            // Refine Item
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.RefineItem), nameof(cacheNpcDialog.refineItemCancelDialog));
            // Dismantle Item
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.DismantleItem), nameof(cacheNpcDialog.dismantleItemCancelDialog));
            // Repair Item
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.RepairItem), nameof(cacheNpcDialog.repairItemCancelDialog));
            // Storage
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.PlayerStorage), nameof(cacheNpcDialog.storageCancelDialog));
            ShowOnEnum(nameof(cacheNpcDialog.type), nameof(NpcDialogType.GuildStorage), nameof(cacheNpcDialog.storageCancelDialog));
        }
    }
}
