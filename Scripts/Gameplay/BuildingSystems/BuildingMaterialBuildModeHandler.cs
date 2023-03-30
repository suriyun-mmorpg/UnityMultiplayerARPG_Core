using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MultiplayerARPG
{
    public class BuildingMaterialBuildModeHandler : MonoBehaviour
    {
        private BuildingMaterial buildingMaterial;

        public void Setup(BuildingMaterial buildingMaterial)
        {
            this.buildingMaterial = buildingMaterial;
        }

        private void OnTriggerStay(Collider other)
        {
            TriggerEnter(other.gameObject, other, null);
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExit(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TriggerEnter(other.gameObject, null, other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit(other.gameObject);
        }

        private void TriggerEnter(GameObject other, Collider collider, Collider2D collider2D)
        {
            if (!ValidateTriggerLayer(other) || SameBuildingAreaTransform(other))
            {
                buildingMaterial.BuildingEntity.TriggerExitGameObject(other);
                return;
            }

            if (buildingMaterial.BuildingEntity.TriggerEnterComponent(other.GetComponent<NoConstructionArea>()))
                return;

            if (buildingMaterial.BuildingEntity.TriggerEnterComponent(other.GetComponent<TilemapCollider2D>()))
                return;

            BuildingArea buildingArea = other.GetComponent<BuildingArea>();
            if (buildingArea != null)
                return;

            bool foundMaterial = other.GetComponent<BuildingMaterial>() != null;
            IGameEntity gameEntity = other.GetComponent<IGameEntity>();
            if (!foundMaterial && !gameEntity.IsNull())
            {
                buildingMaterial.BuildingEntity.TriggerEnterEntity(gameEntity.Entity);
                return;
            }

            bool isMaterialOrNonTrigger = false;
            if (!isMaterialOrNonTrigger && collider != null && !collider.isTrigger && buildingMaterial.CacheCollider.ColliderIntersect(collider, buildingMaterial.boundsSizeRateWhilePlacing))
                isMaterialOrNonTrigger = true;
            if (!isMaterialOrNonTrigger && collider2D != null && !collider2D.isTrigger && buildingMaterial.CacheCollider2D.ColliderIntersect(collider2D, buildingMaterial.boundsSizeRateWhilePlacing))
                isMaterialOrNonTrigger = true;

            if (isMaterialOrNonTrigger)
                buildingMaterial.BuildingEntity.TriggerEnterGameObject(other);
            else
                buildingMaterial.BuildingEntity.TriggerExitGameObject(other);

        }

        private void TriggerExit(GameObject other)
        {
            IGameEntity gameEntity = other.GetComponent<IGameEntity>();
            if (!gameEntity.IsNull())
            {
                buildingMaterial.BuildingEntity.TriggerExitEntity(gameEntity.Entity);
                return;
            }
            buildingMaterial.BuildingEntity.TriggerExitGameObject(other);
        }

        private bool SameBuildingAreaTransform(GameObject other)
        {
            return buildingMaterial.BuildingEntity.BuildingArea != null && buildingMaterial.BuildingEntity.BuildingArea.transform == other.transform;
        }

        public bool ValidateTriggerLayer(GameObject gameObject)
        {
            return gameObject.layer != PhysicLayers.TransparentFX;
        }
    }
}
