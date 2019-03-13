using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class WorkbenchEntity : BuildingEntity
    {
        [Header("Workbench data")]
        public ItemCraft[] itemCrafts;
        public override bool Activatable { get { return true; } }
    }
}
