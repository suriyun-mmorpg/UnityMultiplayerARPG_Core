using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterModel : MonoBehaviour
{
    public const string ANIM_ACTION_STATE = "_Action";
    public string Id { get { return name; } }
    [Header("Animator")]
    [SerializeField]
    private RuntimeAnimatorController animatorController;
    [Header("Collider")]
    public Vector3 center;
    public float radius = 0.5f;
    public float height = 2f;
    [Header("Damage transform")]
    [SerializeField]
    private Transform meleeDamageTransform;
    [SerializeField]
    private Transform missileDamageTransform;
    [Header("Equipment Containers")]
    [SerializeField]
    private EquipmentModelContainer[] equipmentContainers;
    [Header("Effect Containers")]
    [SerializeField]
    private EffectContainer[] effectContainers;

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

    public Transform MeleeDamageTransform
    {
        get
        {
            if (meleeDamageTransform == null)
                meleeDamageTransform = CacheTransform;
            return meleeDamageTransform;
        }
    }

    public Transform MissileDamageTransform
    {
        get
        {
            if (missileDamageTransform == null)
                missileDamageTransform = CacheTransform;
            return missileDamageTransform;
        }
    }

    private Dictionary<string, EquipmentModelContainer> cacheEquipmentModelContainers = null;
    /// <summary>
    /// Dictionary[equipSocket(String), container(EquipmentModelContainer)]
    /// </summary>
    public Dictionary<string, EquipmentModelContainer> CacheEquipmentModelContainers
    {
        get
        {
            if (cacheEquipmentModelContainers == null)
            {
                cacheEquipmentModelContainers = new Dictionary<string, EquipmentModelContainer>();
                foreach (var equipmentContainer in equipmentContainers)
                {
                    if (equipmentContainer.transform != null && !cacheEquipmentModelContainers.ContainsKey(equipmentContainer.equipSocket))
                        cacheEquipmentModelContainers[equipmentContainer.equipSocket] = equipmentContainer;
                }
            }
            return cacheEquipmentModelContainers;
        }
    }

    private Dictionary<string, EffectContainer> cacheEffectContainers = null;
    /// <summary>
    /// Dictionary[effectSocket(String), container(CharacterModelContainer)]
    /// </summary>
    public Dictionary<string, EffectContainer> CacheEffectContainers
    {
        get
        {
            if (cacheEffectContainers == null)
            {
                cacheEffectContainers = new Dictionary<string, EffectContainer>();
                foreach (var effectContainer in effectContainers)
                {
                    if (effectContainer.transform != null && !cacheEffectContainers.ContainsKey(effectContainer.effectSocket))
                        cacheEffectContainers[effectContainer.effectSocket] = effectContainer;
                }
            }
            return cacheEffectContainers;
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
            EquipmentModelContainer container;
            if (!CacheEquipmentModelContainers.TryGetValue(model.Key, out container))
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
                EquipmentModelContainer container;
                if (!CacheEquipmentModelContainers.TryGetValue(model.Key, out container))
                    continue;
                if (container.defaultModel != null)
                    container.defaultModel.SetActive(true);
            }
            cacheModels.Remove(equipPosition);
        }
    }

    public void SetEquipWeapons(EquipWeapons equipWeapons)
    {
        var rightHandWeapon = equipWeapons.rightHand.GetWeaponItem();
        var leftHandWeapon = equipWeapons.leftHand.GetWeaponItem();
        var leftHandShield = equipWeapons.leftHand.GetShieldItem();

        // Clear equipped item models
        var keepingKeys = new List<string>();
        if (rightHandWeapon != null)
            keepingKeys.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
        if (leftHandWeapon != null || leftHandShield != null)
            keepingKeys.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND);

        var keys = new List<string>(cacheModels.Keys);
        foreach (var key in keys)
        {
            if (!keepingKeys.Contains(key) &&
                (key.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) ||
                key.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND)))
                DestroyCacheModel(key);
        }

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
        var keepingKeys = new List<string>();
        foreach (var equipItem in equipItems)
        {
            var armorItem = equipItem.GetArmorItem();
            if (armorItem != null)
                keepingKeys.Add(armorItem.EquipPosition);
        }

        var keys = new List<string>(cacheModels.Keys);
        foreach (var key in keys)
        {
            if (!keepingKeys.Contains(key) && 
                !key.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) &&
                !key.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
                DestroyCacheModel(key);
        }

        foreach (var equipItem in equipItems)
        {
            var armorItem = equipItem.GetArmorItem();
            if (armorItem == null)
                continue;
            var equipPosition = armorItem.EquipPosition;
            if (keepingKeys.Contains(equipPosition))
                InstantiateEquipModel(equipPosition, armorItem.equipmentModels);
        }
    }
    
    private void InstantiateEquipModel(string equipPosition, EquipmentModel[] equipmentModels)
    {
        if (equipmentModels == null || equipmentModels.Length == 0)
            return;
        var gameInstance = GameInstance.Singleton;
        var models = new Dictionary<string, GameObject>();
        foreach (var equipmentModel in equipmentModels)
        {
            var equipSocket = equipmentModel.equipSocket;
            var model = equipmentModel.model;
            if (string.IsNullOrEmpty(equipSocket) || model == null)
                continue;
            EquipmentModelContainer container;
            if (!CacheEquipmentModelContainers.TryGetValue(equipSocket, out container))
                continue;
            var newModel = Instantiate(model, container.transform);
            newModel.transform.localPosition = Vector3.zero;
            newModel.transform.localEulerAngles = Vector3.zero;
            newModel.transform.localScale = Vector3.one;
            newModel.gameObject.SetActive(true);
            newModel.gameObject.layer = gameInstance.characterLayer;
            newModel.RemoveComponentsInChildren<Collider>(false);
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
public struct EquipmentModelContainer
{
    public string equipSocket;
    public GameObject defaultModel;
    public Transform transform;
}

[System.Serializable]
public struct EffectContainer
{
    public string effectSocket;
    public Transform transform;
}