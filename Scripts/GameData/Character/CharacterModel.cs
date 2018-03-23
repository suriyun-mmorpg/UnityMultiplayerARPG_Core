using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterModel : MonoBehaviour
{
    public string Id { get { return name; } }

    [Header("Collider")]
    public Vector3 center;
    public float radius = 0.5f;
    public float height = 2f;
    [Header("Equipment Containers")]
    public Transform rightHandContainer;
    public Transform leftHandContainer;
    public Transform shieldContainer;
    public CharacterModelContainer[] equipmentContainers;

    private Transform cacheTransform;
    public Transform CacheTransform
    {
        get
        {
            if (cacheTransform == null)
                cacheTransform = GetComponent<Transform>();
            return cacheTransform;
        }
    }

    private Animator cacheAnimator;
    public Animator CacheAnimator
    {
        get
        {
            if (cacheAnimator == null)
                cacheAnimator = GetComponent<Animator>();
            return cacheAnimator;
        }
    }

    private Dictionary<string, Transform> cacheEquipmentContainers = null;
    public Dictionary<string, Transform> CacheEquipmentContainers
    {
        get
        {
            if (cacheEquipmentContainers == null)
            {
                cacheEquipmentContainers = new Dictionary<string, Transform>();
                foreach (var equipmentContainer in equipmentContainers)
                {
                    if (equipmentContainer.container != null && !cacheEquipmentContainers.ContainsKey(equipmentContainer.equipPosition))
                        cacheEquipmentContainers[equipmentContainer.equipPosition] = equipmentContainer.container;
                }
            }
            return cacheEquipmentContainers;
        }
    }

    private readonly Dictionary<Transform, GameObject> cacheModels = new Dictionary<Transform, GameObject>();

    private void CreateCacheModel(Transform container, GameObject model)
    {
        DestroyCacheModel(container);
        if (model == null)
            return;
        cacheModels[container] = model;
    }

    private void DestroyCacheModel(Transform container)
    {
        GameObject oldModel;
        if (container != null && cacheModels.TryGetValue(container, out oldModel))
        {
            Destroy(oldModel);
            cacheModels.Remove(container);
        }
    }

    public void SetEquipWeapons(EquipWeapons equipWeapons)
    {
        DestroyCacheModel(rightHandContainer);
        DestroyCacheModel(leftHandContainer);
        DestroyCacheModel(shieldContainer);

        var rightHandWeapon = equipWeapons.rightHand.GetWeaponItem();
        var leftHandWeapon = equipWeapons.leftHand.GetWeaponItem();
        var leftHandShield = equipWeapons.leftHand.GetShieldItem();
        if (rightHandWeapon != null)
            InstantiateEquipModel(rightHandWeapon.equipmentModel, rightHandContainer);
        if (leftHandWeapon != null)
            InstantiateEquipModel(leftHandWeapon.equipmentModel, leftHandContainer);
        if (leftHandShield != null)
            InstantiateEquipModel(leftHandShield.equipmentModel, shieldContainer);
    }
    
    public void SetEquipItems(IList<CharacterItem> equipItems)
    {
        var containers = CacheEquipmentContainers.Values;
        // Clear equipped item models
        foreach (var container in containers)
        {
            DestroyCacheModel(container);
        }

        foreach (var equipItem in equipItems)
        {
            var armorItem = equipItem.GetArmorItem();
            if (armorItem == null)
                continue;
            
            Transform container;
            if (CacheEquipmentContainers.TryGetValue(armorItem.EquipPosition, out container))
                InstantiateEquipModel(armorItem.equipmentModel, container);
        }
    }

    private void InstantiateEquipModel(GameObject prefab, Transform container)
    {
        if (prefab == null || container == null)
            return;
        var equipmentModel = Instantiate(prefab, container);
        equipmentModel.transform.localPosition = Vector3.zero;
        equipmentModel.transform.localEulerAngles = Vector3.zero;
        equipmentModel.transform.localScale = Vector3.one;
        equipmentModel.gameObject.SetActive(true);
        CreateCacheModel(container, equipmentModel);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        var topCorner = CacheTransform.position + center + (Vector3.up * height * 0.5f) - (Vector3.up * radius);
        var bottomCorner = CacheTransform.position + center - (Vector3.up * height * 0.5f) + (Vector3.up * radius);
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
