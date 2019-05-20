using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class TextSetterByGameDataTitle : MonoBehaviour
    {
        public BaseGameData gameData;
        public TextWrapper textWrapper;
        private void Update()
        {
            if (textWrapper == null)
                return;
            textWrapper.text = gameData.Title;
        }
    }
}
