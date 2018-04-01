using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterModel : MonoBehaviour
{
    public const string ANIM_ACTION_STATE = "_Action";
    public string Id { get { return name; } }
    [Header("Animator")]
    public RuntimeAnimatorController animatorController;
    [Header("Collider")]
    public Vector3 center;
    public float radius = 0.5f;
    public float height = 2f;
    [Header("Equipment Containers")]
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
            {
                cacheAnimator = GetComponent<Animator>();
                cacheAnimator.runtimeAnimatorController = CacheAnimatorController;
            }
            return cacheAnimator;
        }
    }

    private AnimatorOverrideController cacheAnimatorController;
    public AnimatorOverrideController CacheAnimatorController
    {
        get
        {
            if (cacheAnimatorController == null)
                cacheAnimatorController = new AnimatorOverrideController(animatorController);
            return cacheAnimatorController;
        }
    }
    
    private Dictionary<string, CharacterModelContainer> cacheEquipmentContainers = null;
    /// <summary>
    /// Dictionary[equipSocket(String), container(CharacterModelContainer)]
    /// </summary>
    public Dictionary<string, CharacterModelContainer> CacheEquipmentContainers
    {
        get
        {
            if (cacheEquipmentContainers == null)
            {
                cacheEquipmentContainers = new Dictionary<string, CharacterModelContainer>();
                foreach (var equipmentContainer in equipmentContainers)
                {
                    if (equipmentContainer.transform != null && !cacheEquipmentContainers.ContainsKey(equipmentContainer.equipSocket))
                        cacheEquipmentContainers[equipmentContainer.equipSocket] = equipmentContainer;
                }
            }
            return cacheEquipmentContainers;
        }
    }
    
    /// <summary>
    /// Dictionary[equipPosition(String), Dictionary[equipSocket(String), model(GameObject)]]
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, GameObject>> cacheModels = new Dictionary<string, Dictionary<string, GameObject>>();

    private void CreateCacheModel(string equipPosition, Dictionary<string, GameObject> models)
    {
        DestroyCacheModel(equipPosition);
        if (models == null)
            return;
        foreach (var model in models)
        {
            CharacterModelContainer container;
            if (!CacheEquipmentContainers.TryGetValue(model.Key, out container))
                continue;
            if (container.defaultModel != null)
                container.defaultModel.SetActive(false);
        }
        cacheModels[equipPosition] = models;
    }

    private void DestroyCacheModel(string equipPosition)
    {
        Dictionary<string, GameObject> oldModels;
        if (!string.IsNullOrEmpty(equipPosition) && cacheModels.TryGetValue(equipPosition, out oldModels) && oldModels != null)
        {
            foreach (var model in oldModels)
            {
                Destroy(model.Value);
                CharacterModelContainer container;
                if (!CacheEquipmentContainers.TryGetValue(model.Key, out container))
                    continue;
                if (container.defaultModel != null)
                    container.defaultModel.SetActive(true);
            }
            cacheModels.Remove(equipPosition);
        }
    }

    public void SetEquipWeapons(EquipWeapons equipWeapons)
    {
        // Clear equipped item models
        DestroyCacheModel(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
        DestroyCacheModel(GameDataConst.EQUIP_POSITION_LEFT_HAND);

        var rightHandWeapon = equipWeapons.rightHand.GetWeaponItem();
        var leftHandWeapon = equipWeapons.leftHand.GetWeaponItem();
        var leftHandShield = equipWeapons.leftHand.GetShieldItem();
        if (rightHandWeapon != null)
            InstantiateEquipModel(GameDataConst.EQUIP_POSITION_RIGHT_HAND, rightHandWeapon.equipmentModels);
        if (leftHandWeapon != null)
            InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandWeapon.subEquipmentModels);
        if (leftHandShield != null)
            InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandShield.equipmentModels);
    }
    
    public void SetEquipItems(IList<CharacterItem> equipItems)
    {
        // Clear equipped item models
        var keys = new List<string>(cacheModels.Keys);
        foreach (var equipPosition in keys)
        {
            if (!GameDataConst.EQUIP_POSITION_RIGHT_HAND.Equals(equipPosition) &&
                !GameDataConst.EQUIP_POSITION_LEFT_HAND.Equals(equipPosition))
                DestroyCacheModel(equipPosition);
        }

        foreach (var equipItem in equipItems)
        {
            var equipmentItem = equipItem.GetEquipmentItem();
            if (equipmentItem == null)
                continue;
            InstantiateEquipModel(equipmentItem.EquipPosition, equipmentItem.equipmentModels);
        }
    }
    
    private void InstantiateEquipModel(string equipPosition, EquipmentModel[] equipmentModels)
    {
        if (equipmentModels == null || equipmentModels.Length == 0)
            return;
        var models = new Dictionary<string, GameObject>();
        foreach (var equipmentModel in equipmentModels)
        {
            var equipSocket = equipmentModel.equipSocket;
            var model = equipmentModel.model;
            if (string.IsNullOrEmpty(equipSocket) || model == null)
                continue;
            CharacterModelContainer container;
            if (!CacheEquipmentContainers.TryGetValue(equipSocket, out container))
                continue;
            var newModel = Instantiate(model, container.transform);
            newModel.transform.localPosition = Vector3.zero;
            newModel.transform.localEulerAngles = Vector3.zero;
            newModel.transform.localScale = Vector3.one;
            newModel.gameObject.SetActive(true);
            models.Add(equipSocket, newModel);
        }
        CreateCacheModel(equipPosition, models);
    }

    public void ChangeActionClip(AnimationClip clip)
    {
        CacheAnimatorController[ANIM_ACTION_STATE] = clip;
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
    public string equipSocket;
    public GameObject defaultModel;
    public Transform transform;
}
