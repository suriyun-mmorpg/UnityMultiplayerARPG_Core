using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace MultiplayerARPG
{
    public class NpcDialogConditionNode : Node
    {
        [Input]
        public NpcDialog input;
        [Output]
        public NpcDialog pass;
        [Output]
        public NpcDialog fail;

        public NpcDialogCondition[] conditions;
    }
}
