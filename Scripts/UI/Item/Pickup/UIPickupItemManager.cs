using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIPickupItemManager : MonoBehaviour
    {
        public UIPickupItemList uiList;
        public GameObject[] signalObjects;
        public NearbyEntityDetector ItemDropEntityDetector { get; protected set; }

        private void Awake()
        {
            GameObject tempGameObject = new GameObject("_ItemDropEntityDetector");
            ItemDropEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
            ItemDropEntityDetector.detectingRadius = GameInstance.Singleton.pickUpItemDistance;
            ItemDropEntityDetector.findItemDrop = true;
            ItemDropEntityDetector.onUpdateList += OnUpdate;
        }

        private void OnDestroy()
        {
            if (ItemDropEntityDetector != null)
            {
                ItemDropEntityDetector.onUpdateList -= OnUpdate;
                Destroy(ItemDropEntityDetector.gameObject);
            }
        }

        private void OnUpdate()
        {
            // Prepare dropped items
            List<CharacterItem> droppedItems = new List<CharacterItem>();
            string tempEntryId;
            CharacterItem tempCharacterItem;
            foreach (ItemDropEntity entity in ItemDropEntityDetector.itemDrops)
            {
                if (entity.PlaceHolderItem == null)
                {
                    // Only place holder item drop entity will be shown in the list
                    continue;
                }
                tempEntryId = entity.ObjectId.ToString();
                tempCharacterItem = CharacterItem.Create(entity.PlaceHolderItem, entity.PlaceHolderLevel, entity.PlaceHolderAmount);
                tempCharacterItem.id = tempEntryId;
                droppedItems.Add(tempCharacterItem);
            }
            // Update list
            if (uiList != null)
            {
                uiList.UpdateData(droppedItems);
                if (droppedItems.Count == 0)
                {
                    if (uiList.IsVisible())
                        uiList.Hide();
                }
                else
                {
                    if (!uiList.IsVisible())
                        uiList.Show();
                }
            }
            // Update signal objects
            if (signalObjects != null && signalObjects.Length > 0)
            {
                foreach (GameObject signalObject in signalObjects)
                {
                    signalObject.SetActive(droppedItems.Count > 0);
                }
            }
        }
    }
}
