using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using UnityEngine;

namespace MultiplayerARPG
{
    public class GameDataContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            property.ShouldSerialize = instance =>
            {
                if (!(member is PropertyInfo propertyInfo))
                    return false;
                if (propertyInfo.PropertyType == typeof(GameObject))
                    return false;
                if (propertyInfo.PropertyType == typeof(Transform))
                    return false;
                if (typeof(Component).IsAssignableFrom(propertyInfo.PropertyType))
                    return false;
                if (propertyInfo.PropertyType == typeof(Sprite))
                    return false;
                if (propertyInfo.PropertyType == typeof(AudioClip))
                    return false;
                if (propertyInfo.PropertyType == typeof(Color))
                    return false;
                if (propertyInfo.PropertyType == typeof(EquipmentModel))
                    return false;
                if (propertyInfo.PropertyType == typeof(EquipmentModel[]))
                    return false;
                if (propertyInfo.PropertyType == typeof(AudioClipWithVolumeSettings))
                    return false;
                if (propertyInfo.PropertyType == typeof(AudioClipWithVolumeSettings[]))
                    return false;
                if (propertyInfo.PropertyType == typeof(WeaponType.PlayableCharacterModelSettingsData))
                    return false;
                return true;
            };
            return property;
        }
    }
}
