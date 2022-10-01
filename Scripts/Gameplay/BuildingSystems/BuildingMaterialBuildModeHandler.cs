using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MultiplayerARPG
{
    public class BuildingMaterialBuildModeHandler : MonoBehaviour
    {
        private class BuilingMaterialInteractData
        {
            internal BuildingMaterial Material { get; set; }
            internal Func<bool> IntersectFunc { get; set; }
        }
        public const float BUILDING_MATERIAL_INTERSECT_DELAY = 0.25f;
        private BuildingMaterial buildingMaterial;
        private Dictionary<int, BuilingMaterialInteractData> interactingMaterials = new Dictionary<int, BuilingMaterialInteractData>();
        private float intersectCountDown = 0f;

        public async void Setup(BuildingMaterial buildingMaterial)
        {
            this.buildingMaterial = buildingMaterial;
            gameObject.SetActive(false);
            // Wait for next frame to make a Unity physics works properly
            await UniTask.NextFrame();
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (interactingMaterials.Count == 0)
            {
                intersectCountDown = 0;
                return;
            }

            if (intersectCountDown <= 0)
            {
                foreach (BuilingMaterialInteractData interactData in interactingMaterials.Values)
                {
                    HandleBuildingMaterialInteraction(interactData);
                }
                intersectCountDown = BUILDING_MATERIAL_INTERSECT_DELAY;
            }
            else
            {
                intersectCountDown -= Time.unscaledDeltaTime;
            }
        }

        private bool HandleBuildingMaterialInteraction(BuilingMaterialInteractData interactData)
        {
            if (SameBuildingAreaTransform(interactData.Material.gameObject) || !interactData.IntersectFunc())
            {
                buildingMaterial.BuildingEntity.TriggerExitEntity(interactData.Material.Entity);
                return false;
            }
            buildingMaterial.BuildingEntity.TriggerEnterEntity(interactData.Material.Entity);
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter(other.gameObject, () => buildingMaterial.CacheCollider.ColliderIntersect(other, buildingMaterial.boundsSizeRateWhilePlacing));
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExit(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter(other.gameObject, () => buildingMaterial.CacheCollider2D.ColliderIntersect(other, buildingMaterial.boundsSizeRateWhilePlacing));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit(other.gameObject);
        }

        private void TriggerEnter(GameObject other, Func<bool> materialIntersectFunc)
        {
            if (!ValidateTriggerLayer(other))
                return;

            if (buildingMaterial.BuildingEntity.TriggerEnterComponent(other.GetComponent<NoConstructionArea>()))
                return;

            if (buildingMaterial.BuildingEntity.TriggerEnterComponent(other.GetComponent<TilemapCollider2D>()))
                return;

            BuildingMaterial material = other.GetComponent<BuildingMaterial>();
            if (material != null)
            {
                if (!interactingMaterials.ContainsKey(material.GetInstanceID()))
                {
                    interactingMaterials.Add(material.GetInstanceID(), new BuilingMaterialInteractData()
                    {
                        Material = material,
                        IntersectFunc = materialIntersectFunc,
                    });
                }
            }
            else
            {
                if (SameBuildingAreaTransform(other))
                    return;
                IGameEntity gameEntity = other.GetComponent<IGameEntity>();
                if (gameEntity != null)
                    buildingMaterial.BuildingEntity.TriggerEnterEntity(gameEntity.Entity);
            }
        }

        private void TriggerExit(GameObject other)
        {
            if (buildingMaterial.BuildingEntity.TriggerExitComponent(other.GetComponent<NoConstructionArea>()))
                return;

            if (buildingMaterial.BuildingEntity.TriggerExitComponent(other.GetComponent<TilemapCollider2D>()))
                return;

            // Removing the material from the intersect checking list
            BuildingMaterial material = other.GetComponent<BuildingMaterial>();
            if (material != null)
                interactingMaterials.Remove(material.GetInstanceID());

            // Material is derived from `IGameEntity`, so just find for `IGameEntity` is fine
            IGameEntity gameEntity = other.GetComponent<IGameEntity>();
            if (gameEntity != null)
                buildingMaterial.BuildingEntity.TriggerExitEntity(gameEntity.Entity);
        }

        private bool SameBuildingAreaTransform(GameObject other)
        {
            return buildingMaterial.BuildingEntity.BuildingArea != null && buildingMaterial.BuildingEntity.BuildingArea.transform.root == other.transform.root;
        }

        public bool ValidateTriggerLayer(GameObject gameObject)
        {
            return gameObject.layer != PhysicLayers.TransparentFX;
        }
    }
}
