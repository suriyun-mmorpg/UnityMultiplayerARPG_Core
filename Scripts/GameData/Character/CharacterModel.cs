using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterModel : MonoBehaviour
{
    [Header("Collider")]
    public Vector3 center;
    public float radius = 0.5f;
    public float height = 2f;
    [Header("Equipment Containers")]
    public Transform rightHandContainer;
    public Transform leftHandContainer;
    public CharacterModelContainer[] equipmentContainers;

    private Transform tempTransform;
    public Transform TempTransform
    {
        get
        {
            if (tempTransform == null)
                tempTransform = GetComponent<Transform>();
            return tempTransform;
        }
    }

    private Animator tempAnimator;
    public Animator TempAnimator
    {
        get
        {
            if (tempAnimator == null)
                tempAnimator = GetComponent<Animator>();
            return tempAnimator;
        }
    }

    private Dictionary<string, Transform> tempEquipmentContainers = null;
    public Dictionary<string, Transform> TempEquipmentContainers
    {
        get
        {
            if (tempEquipmentContainers == null)
            {
                tempEquipmentContainers = new Dictionary<string, Transform>();
                if (rightHandContainer != null)
                    tempEquipmentContainers.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND, rightHandContainer);
                if (leftHandContainer != null)
                    tempEquipmentContainers.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandContainer);
                foreach (var equipmentContainer in equipmentContainers)
                {
                    if (equipmentContainer.container != null && !tempEquipmentContainers.ContainsKey(equipmentContainer.equipPosition))
                        tempEquipmentContainers[equipmentContainer.equipPosition] = equipmentContainer.container;
                }
            }
            return tempEquipmentContainers;
        }
    }
    
    public void SetEquipItems(IList<CharacterItem> equipItems)
    {
        var containers = TempEquipmentContainers.Values;
        // Clear equipped item models
        foreach (var container in containers)
        {
            container.RemoveChildren();
        }

        foreach (var equipItem in equipItems)
        {
            var weaponItem = equipItem.GetWeaponItem();
            var shieldItem = equipItem.GetShieldItem();
            var equipmentItem = equipItem.GetEquipmentItem();
            if (equipmentItem == null)
                continue;

            var position = equipmentItem.equipPosition;
            if (weaponItem != null || shieldItem != null)
                position = equipItem.isSubWeapon ? GameDataConst.EQUIP_POSITION_LEFT_HAND : GameDataConst.EQUIP_POSITION_RIGHT_HAND;

            var equipmentModelPrefab = equipmentItem.equipmentModel;
            if (equipmentModelPrefab != null && TempEquipmentContainers.ContainsKey(position))
            {
                var container = TempEquipmentContainers[position];
                var equipmentModel = Instantiate(equipmentModelPrefab, container);
                equipmentModel.transform.localPosition = Vector3.zero;
                equipmentModel.transform.localEulerAngles = Vector3.zero;
                equipmentModel.transform.localScale = Vector3.one;
                equipmentModel.gameObject.SetActive(true);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        var topCorner = TempTransform.position + center + (Vector3.up * height * 0.5f) - (Vector3.up * radius);
        var bottomCorner = TempTransform.position + center - (Vector3.up * height * 0.5f) + (Vector3.up * radius);
        Gizmos.DrawWireSphere(topCorner, radius);
        Gizmos.DrawWireSphere(bottomCorner, radius);
        Gizmos.DrawLine(topCorner + Vector3.left * radius, bottomCorner + Vector3.left * radius);
        Gizmos.DrawLine(topCorner + Vector3.right * radius, bottomCorner + Vector3.right * radius);
        Gizmos.DrawLine(topCorner + Vector3.forward * radius, bottomCorner + Vector3.forward * radius);
        Gizmos.DrawLine(topCorner + Vector3.back * radius, bottomCorner + Vector3.back * radius);
    }
}

[System.Serializable]
public struct CharacterModelContainer
{
    public string equipPosition;
    public Transform container;
}
