using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class MountEntity : BaseVehicleEntity
    {

        public override sealed bool IsDestroyWhenDriverExit
        {
            get
            {
                // Mount always destroyed when driver exit
                return true;
            }
        }
    }
}
