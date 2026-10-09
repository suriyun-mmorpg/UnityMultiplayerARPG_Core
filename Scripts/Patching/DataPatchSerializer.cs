using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class DataPatchSerializer
    {
        private static bool IsDataReference(Type type) => typeof(IPatchableData).IsAssignableFrom(type);

        public static bool IsSupportedFieldType(Type type)
        {
            if (IsDataReference(type))
                return true;
            if (typeof(IDictionary).IsAssignableFrom(type))
                return false;
            if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) || type == typeof(Vector2Int) || type == typeof(Vector3Int) || type == typeof(Color) || type == typeof(Color32) || type == typeof(Quaternion) || type == typeof(Bounds) || type == typeof(Rect) || type == typeof(LayerMask))
                return true;
            if (typeof(UnityEngine.Object).IsAssignableFrom(type) || typeof(UnityEngine.AddressableAssets.AssetReference).IsAssignableFrom(type) || type.Name == "SceneField")
                return false;
            if (type.IsArray)
                return type.GetArrayRank() == 1 && !type.GetElementType().IsArray && !type.GetElementType().IsGenericType && IsSupportedFieldType(type.GetElementType());
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition() == typeof(List<>) && !type.GetGenericArguments()[0].IsArray && !type.GetGenericArguments()[0].IsGenericType && IsSupportedFieldType(type.GetGenericArguments()[0]);
            if (type.IsPrimitive)
                return type != typeof(IntPtr) && type != typeof(UIntPtr);
            if (type.IsEnum)
                return true;
            if (type.Namespace == "System")
                return type == typeof(string);
            return type.IsEnum || type == typeof(string) || !type.IsAbstract && !typeof(Delegate).IsAssignableFrom(type) && type.IsDefined(typeof(SerializableAttribute), false);
        }

        private static bool IsProtected(FieldInfo field)
        {
            if (field.Name == "maxHp" && typeof(Harvestable).IsAssignableFrom(field.DeclaringType) || field.Name == "id" || field.Name == "patchId")
                return true;
            if (field.DeclaringType == typeof(CashPackage) && field.Name == "productId")
                return true;
            // Inspector actions are editor controls, not runtime configuration.
            return field.GetCustomAttributes(false).Any(attribute => attribute.GetType().Name == "InspectorButtonAttribute");
        }

        private static bool HasProtectedState(Type type, HashSet<Type> visited = null)
        {
            if (IsDataReference(type) || type.IsPrimitive || type.IsEnum || type == typeof(string))
                return false;
            if (!IsSupportedFieldType(type))
                return true;
            if (type.IsArray)
                return HasProtectedState(type.GetElementType(), visited);
            if (type.IsGenericType)
                return HasProtectedState(type.GetGenericArguments()[0], visited);
            if (visited == null)
                visited = new HashSet<Type>();
            if (!visited.Add(type))
                return false;
            return Fields(type, false).Any(field => IsProtected(field) || HasProtectedState(field.FieldType, visited));
        }

        private static bool HasProtectedValue(object value, int depth = 0)
        {
            if (value == null || depth > 32)
                return false;
            if (value is UnityEngine.AddressableAssets.AssetReference reference)
                return !string.IsNullOrEmpty(reference.AssetGUID);
            if (value is UnityEngine.Object unityObject)
                return unityObject != null;
            if (value is IList list)
            {
                foreach (object row in list)
                    if (HasProtectedValue(row, depth + 1))
                        return true;
                return false;
            }

            foreach (FieldInfo field in Fields(value.GetType(), false))
            {
                object child = field.GetValue(value);
                if (IsProtected(field) && child != null && !Equals(child, field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : null))
                    return true;
                if (!IsSupportedFieldType(field.FieldType) && child != null && HasProtectedValue(child, depth + 1))
                    return true;
                if (IsSupportedFieldType(field.FieldType) && !IsDataReference(field.FieldType) && HasProtectedState(field.FieldType) && HasProtectedValue(child, depth + 1))
                    return true;
            }

            return false;
        }

        public static IEnumerable<FieldInfo> Fields(Type type, bool patchOnly = true)
        {
            for (; type != null && type != typeof(UnityEngine.Object); type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.IsStatic || field.IsInitOnly || field.IsLiteral || field.IsDefined(typeof(NonSerializedAttribute), false))
                        continue;
                    if (!(field.IsPublic || field.IsDefined(typeof(SerializeField), false)))
                        continue;
                    if (!patchOnly || !IsProtected(field) && IsSupportedFieldType(field.FieldType))
                        yield return field;
                }
            }
        }

        public static JObject Export(object value) => (JObject)Write(value, true, 0);

        private static JToken Write(object value, bool root, int depth)
        {
            if (depth > 32)
                throw new InvalidOperationException("Patch nesting exceeds 32 levels.");
            if (value == null || value is UnityEngine.Object obj && obj == null)
                return JValue.CreateNull();
            if (!root && value is IPatchableData patchableData)
                return new JObject { ["dataId"] = patchableData.DataId };
            Type type = value.GetType();
            if (value is Vector2 vector)
                return new JObject{["x"] = vector.x, ["y"] = vector.y};
            if (value is Vector3 vector3)
                return new JObject{["x"] = vector3.x, ["y"] = vector3.y, ["z"] = vector3.z};
            if (value is Vector2Int vector2Int)
                return new JObject{["x"] = vector2Int.x, ["y"] = vector2Int.y};
            if (value is Vector3Int vector3Int)
                return new JObject{["x"] = vector3Int.x, ["y"] = vector3Int.y, ["z"] = vector3Int.z};
            if (value is LayerMask mask)
                return new JObject{["value"] = mask.value};
            if (value is Rect rect)
                return new JObject{["x"] = rect.x, ["y"] = rect.y, ["width"] = rect.width, ["height"] = rect.height};
            if (value is Bounds bounds)
                return new JObject{["center"] = Write(bounds.center, false, depth + 1), ["size"] = Write(bounds.size, false, depth + 1)};
            if (type.IsEnum)
                return new JValue(Convert.ToInt64(value, CultureInfo.InvariantCulture));
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                return JToken.FromObject(value);
            if (value is IList list)
            {
                var result = new JArray();
                foreach (object entry in list)
                    result.Add(Write(entry, false, depth + 1));
                return result;
            }

            if (!root && value is UnityEngine.Object)
                throw new InvalidOperationException($"Unsupported patch reference: {type.Name}");
            var data = new JObject();
            foreach (FieldInfo field in Fields(type).OrderBy(field => field.Name, StringComparer.Ordinal))
                data.Add(field.Name, Write(field.GetValue(value), false, depth + 1));
            // Asset-only containers (for example SkillMount) intentionally export {}.
            return data;
        }

        public static void ReadInto(object target, JObject data, DataPatchRegistry registry, bool validate = true)
        {
            ReadObject(target, data, registry, 0, validate);
        }

        private static void ReadObject(object target, JObject data, DataPatchRegistry registry, int depth, bool validate)
        {
            if (depth > 32)
                throw new InvalidOperationException("Patch nesting exceeds 32 levels.");
            var fields = Fields(target.GetType()).ToDictionary(field => field.Name, StringComparer.Ordinal);
            foreach (JProperty entry in data.Properties())
            {
                if (!fields.TryGetValue(entry.Name, out FieldInfo field))
                    throw new InvalidOperationException($"{target.GetType().Name}.{entry.Name} is not patchable.");
                object value = ReadValue(field.FieldType, field.GetValue(target), entry.Value, registry, depth + 1, validate);
                field.SetValue(target, value);
            }

            // A generator-only patch must update the effective requirement rows too.
            // Full exports include both; explicitly reviewed rows take precedence.
            if (target is BaseSkill requirementSkill && requirementSkill.requirement != null && ((data["requirement"] != null || data["maxLevel"] != null) && data["requirementEachLevels"] == null || requirementSkill.requirementEachLevels?.Count == 0))
            {
                if (requirementSkill.maxLevel < 1 || requirementSkill.maxLevel > 20000)
                    throw new InvalidOperationException("Skill maximum level must be between 1 and 20000.");
                var source = requirementSkill.requirement;
                requirementSkill.requirementEachLevels = new List<SkillRequirementEntry>();
                for (int level = 1; level <= requirementSkill.maxLevel; ++level)
                    requirementSkill.requirementEachLevels.Add(new SkillRequirementEntry{disallow = source.disallow, characterLevel = source.characterLevel.GetAmount(level), skillPoint = source.skillPoint.GetAmount(level), gold = source.gold.GetAmount(level), attributeAmounts = source.attributeAmounts, skillLevels = source.skillLevels, currencyAmounts = source.currencyAmounts, itemAmounts = source.itemAmounts});
            }

            if (validate)
            {
                // Validate after assignment so JSON property order cannot change the rules.
                if (!(target is ItemRandomByWeight empty && empty.item == null))
                    foreach (JProperty entry in data.Properties())
                    {
                        if (target is DamageInfo damage && entry.Name == "hitFov" && (damage.damageType != DamageType.Melee || damage.hitDistance <= 0))
                            continue;
                        ValidateField(fields[entry.Name], fields[entry.Name].GetValue(target));
                    }

                ValidateObject(target);
            }
        }

        private static object ReadValue(Type type, object current, JToken token, DataPatchRegistry registry, int depth, bool validate)
        {
            if (type == typeof(Vector2Int) || type == typeof(Vector3Int) || type == typeof(LayerMask) || type == typeof(Rect) || type == typeof(Bounds))
            {
                string[] keys = type == typeof(LayerMask) ? new[]{"value"} : type == typeof(Bounds) ? new[]{"center", "size"} : type == typeof(Rect) ? new[]{"x", "y", "width", "height"} : type == typeof(Vector2Int) ? new[]{"x", "y"} : new[]{"x", "y", "z"};
                if (!(token is JObject native) || native.Count != keys.Length || keys.Any(key => native[key] == null))
                    throw new InvalidOperationException("Invalid " + type.Name + " fields.");
                object Part(string key, Type partType) => ReadValue(partType, null, native[key], registry, depth + 1, validate);
                if (type == typeof(LayerMask))
                    return (LayerMask)(int)Part("value", typeof(int));
                if (type == typeof(Bounds))
                    return new Bounds((Vector3)Part("center", typeof(Vector3)), (Vector3)Part("size", typeof(Vector3)));
                if (type == typeof(Rect))
                    return new Rect((float)Part("x", typeof(float)), (float)Part("y", typeof(float)), (float)Part("width", typeof(float)), (float)Part("height", typeof(float)));
                if (type == typeof(Vector2Int))
                    return new Vector2Int((int)Part("x", typeof(int)), (int)Part("y", typeof(int)));
                return new Vector3Int((int)Part("x", typeof(int)), (int)Part("y", typeof(int)), (int)Part("z", typeof(int)));
            }

            if (type == typeof(Vector3))
            {
                if (!(token is JObject vector3) || vector3.Count != 3 || vector3["x"] == null || vector3["y"] == null || vector3["z"] == null)
                    throw new InvalidOperationException("Vector3 must contain only x, y and z.");
                return new Vector3((float)ReadValue(typeof(float), null, vector3["x"], registry, depth + 1, validate), (float)ReadValue(typeof(float), null, vector3["y"], registry, depth + 1, validate), (float)ReadValue(typeof(float), null, vector3["z"], registry, depth + 1, validate));
            }

            if (type == typeof(Vector2))
            {
                if (!(token is JObject vector) || vector.Count != 2 || vector["x"] == null || vector["y"] == null)
                    throw new InvalidOperationException("Vector2 must contain only x and y.");
                return new Vector2((float)ReadValue(typeof(float), null, vector["x"], registry, depth + 1, validate), (float)ReadValue(typeof(float), null, vector["y"], registry, depth + 1, validate));
            }

            if (token.Type == JTokenType.Null)
            {
                if (type.IsValueType)
                    throw new InvalidOperationException($"Null is invalid for {type.Name}.");
                if (current != null && HasProtectedState(type) && HasProtectedValue(current))
                    throw new InvalidOperationException($"Cannot clear protected state in {type.Name}.");
                return null;
            }

            if (typeof(IPatchableData).IsAssignableFrom(type))
            {
                if (!(token is JObject reference) || reference.Count != 1 || reference["dataId"]?.Type != JTokenType.Integer)
                    throw new InvalidOperationException("References must contain only an integer dataId.");
                return registry.Resolve(type, checked((int)reference["dataId"]));
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                throw new InvalidOperationException($"Unsupported patch type: {type.Name}");
            if (type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                if (!(token is JArray array) || array.Count > 20000)
                    throw new InvalidOperationException("Expected a bounded array.");
                Type element = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                IList previous = current as IList;
                bool preserveRows = HasProtectedState(element);
                if (preserveRows && array.Count != (previous?.Count ?? 0))
                    throw new InvalidOperationException($"{element.Name} row count is not patchable.");
                Array result = Array.CreateInstance(element, array.Count);
                for (int i = 0; i < array.Count; ++i)
                    result.SetValue(ReadValue(element, previous != null && i < previous.Count ? previous[i] : null, array[i], registry, depth + 1, validate), i);
                if (type.IsArray)
                    return result;
                IList list = (IList)Activator.CreateInstance(type);
                foreach (object entry in result)
                    list.Add(entry);
                return list;
            }

            if (type.IsEnum)
            {
                if (token.Type != JTokenType.Integer)
                    throw new InvalidOperationException("Expected integer enum.");
                object result;
                try
                {
                    result = Enum.ToObject(type, token.ToObject(Enum.GetUnderlyingType(type)));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Enum value exceeds its underlying numeric range.", ex);
                }

                if (!Enum.IsDefined(type, result))
                {
                    if (!type.IsDefined(typeof(FlagsAttribute), false))
                        throw new InvalidOperationException("Unknown enum value.");
                    ulong Bits(object number) => Enum.GetUnderlyingType(type) == typeof(ulong) ? Convert.ToUInt64(number) : unchecked((ulong)Convert.ToInt64(number));
                    ulong allowed = 0;
                    foreach (object flag in Enum.GetValues(type))
                        allowed |= Bits(flag);
                    if ((Bits(result) & ~allowed) != 0)
                        throw new InvalidOperationException("Unknown enum flag.");
                }

                return result;
            }

            if (type == typeof(char))
            {
                if (token.Type != JTokenType.String || ((string)token).Length != 1)
                    throw new InvalidOperationException("Expected one character.");
                return ((string)token)[0];
            }

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                bool number = token.Type == JTokenType.Integer || token.Type == JTokenType.Float;
                if (type == typeof(bool) ? token.Type != JTokenType.Boolean : type == typeof(string) ? token.Type != JTokenType.String : !number)
                    throw new InvalidOperationException($"Incorrect JSON type for {type.Name}.");
                bool integral = type != typeof(float) && type != typeof(double) && type != typeof(decimal) && type != typeof(string) && type != typeof(bool);
                if (integral && token.Type != JTokenType.Integer)
                    throw new InvalidOperationException("Expected integer without rounding.");
                object result = token.ToObject(type, JsonSerializer.Create(new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None}));
                if (result is float f && (float.IsNaN(f) || float.IsInfinity(f)) || result is double d && (double.IsNaN(d) || double.IsInfinity(d)))
                    throw new InvalidOperationException("Non-finite number.");
                return result;
            }

            if (!(token is JObject nested))
                throw new InvalidOperationException($"Expected object for {type.Name}.");
            // Copy existing non-patchable state, but never mutate shared nested objects during staging.
            object copy = current == null ? Activator.CreateInstance(type) : typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(current, null);
            ReadObject(copy, nested, registry, depth, validate);
            return copy;
        }

        private static void ValidateField(FieldInfo field, object value)
        {
            if (value == null || !(value is IConvertible) || value is bool || value is string || value is char)
                return;
            double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            MinAttribute min = field.GetCustomAttribute<MinAttribute>();
            RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();
            if (min != null && number < min.min || range != null && (number < range.min || number > range.max))
                throw new InvalidOperationException($"{field.Name} is outside its allowed range.");
            bool signedBonus = field.DeclaringType == typeof(AttributeAmount) || field.DeclaringType == typeof(StatusEffectResistanceAmount);
            if ((!signedBonus && field.Name == "amount" || field.Name == "randomWeight" || field.Name == "noDropWeight" || field.Name == "minDropItems" || field.Name == "maxDropItems") && number < 0)
                throw new InvalidOperationException($"{field.Name} must be nonnegative.");
        }

        private static void ValidateObject(object value)
        {
            if (value is CrosshairSetting crosshair)
            {
                CheckBalanceNumber(crosshair.expandPerFrame, "crosshair expansion");
                CheckBalanceNumber(crosshair.shrinkPerFrame, "crosshair shrink");
                CheckBalanceNumber(crosshair.minSpread, "crosshair minimum spread");
                CheckBalanceNumber(crosshair.maxSpread, "crosshair maximum spread");
                CheckBalanceNumber(crosshair.addSpreadWhileAttackAndMoving, "crosshair moving spread");
                if (crosshair.minSpread > crosshair.maxSpread)
                    throw new InvalidOperationException("Crosshair minimum spread exceeds maximum.");
            }

            if (value is BuffRemoval removal)
            {
                CheckBalanceNumber(removal.maxChance, "buff removal maximum");
                if (removal.maxChanceEachLevels == null)
                    throw new InvalidOperationException("Buff removal caps must be an array.");
                foreach (float chance in removal.maxChanceEachLevels)
                    CheckChance(chance, "buff removal level cap");
            }

            if (value is BaseItem balanceItem)
            {
                foreach (FieldInfo field in Fields(value.GetType()))
                    if (field.FieldType == typeof(ItemRequirement))
                        ValidateUseRequirement((ItemRequirement)field.GetValue(value));
                CheckBalanceNumber(balanceItem.DismantleReturnGold, "dismantleReturnGold");
                ValidateBalanceItems(balanceItem.DismantleReturnItems);
                ValidateBalanceCurrencies(balanceItem.DismantleReturnCurrencies);
                }

            if (value is SkillItem skillItem && (skillItem.SkillData == null || skillItem.SkillLevel < 1 || skillItem.SkillLevel > skillItem.SkillData.maxLevel))
                throw new InvalidOperationException("Skill item requires a skill and a supported skill level.");
            if (value is AmmoItem ammo)
                for (int level = 1; level <= ammo.MaxLevel; ++level)
                    foreach (DamageIncremental damage in ammo.IncreaseDamages)
                        ValidateWeaponDamage(damage.amount.GetAmount(level));
            if (value is EntityMovementForceApplierData force)
            {
                CheckBalanceNumber(force.speed, "force speed");
                CheckBalanceNumber(force.deceleration, "force deceleration");
            // Nonpositive duration is the existing unlimited-duration mode.
            }

            if (value is SimpleDashAttackSkill dash)
            {
                CheckBalanceNumber(dash.dashToEnemyStoppingDistance, "dashToEnemyStoppingDistance");
                CheckBalanceNumber(dash.dashMovingTriggerInterval, "dashMovingTriggerInterval");
            // Nonpositive lookup radii disable the corresponding attack phase.
            }

            if (value is MonsterCharacter monster)
            {
                CheckBalanceNumber(monster.VisualRange, "visualRange");
                CheckBalanceNumber(monster.SummonedVisualRange, "summonedVisualRange");
                CheckBalanceNumber(monster.WanderMoveSpeed, "wanderMoveSpeed");
                CheckBalanceNumber(monster.MoveSpeedRateWhileAttacking, "moveSpeedRateWhileAttacking");
                ValidateBuff(monster.SummonerBuff, 1);
            }

            if (value is SkillRequirementEntry requirementEntry)
            {
                CheckBalanceNumber(requirementEntry.characterLevel, "required character level");
                CheckBalanceNumber(requirementEntry.skillPoint, "required skill points");
                CheckBalanceNumber(requirementEntry.gold, "required soft currency");
                ValidateBalanceItems(requirementEntry.itemAmounts);
                ValidateBalanceCurrencies(requirementEntry.currencyAmounts);
                ValidateSkillRequirementReferences(requirementEntry.attributeAmounts, requirementEntry.skillLevels);
            }

            if (value is SkillRequirement requirementSource)
            {
                ValidateBalanceItems(requirementSource.itemAmounts);
                ValidateBalanceCurrencies(requirementSource.currencyAmounts);
                ValidateSkillRequirementReferences(requirementSource.attributeAmounts, requirementSource.skillLevels);
            }

            if (value is DamageInfo damageInfo)
            {
                CheckBalanceNumber(damageInfo.hitDistance, "hitDistance");
                CheckBalanceNumber(damageInfo.missileDistance, "missileDistance");
                CheckBalanceNumber(damageInfo.missileSpeed, "missileSpeed");
                CheckBalanceNumber(damageInfo.throwForce, "throwForce");
                CheckBalanceNumber(damageInfo.throwableLifeTime, "throwableLifeTime");
            }

            if (value is BaseMapInfo map)
            {
                CheckBalanceNumber(map.MinimapBoundsWidth, "minimapBoundsWidth");
                CheckBalanceNumber(map.MinimapBoundsLength, "minimapBoundsLength");
                CheckBalanceNumber(map.MinimapOrthographicSize, "minimapOrthographicSize");
                if (map.ExcludeItems == null || map.ExcludeArmorTypes == null || map.ExcludeWeaponTypes == null)
                    throw new InvalidOperationException("Map exclusion and loading-content lists must be non-null.");
                if (map.ExcludeItems.Any(item => item == null))
                    throw new InvalidOperationException("Map item exclusions require valid item references.");
            }

            if (value is MapInfo mapInfo && mapInfo.respawnPointsByCondition == null)
                throw new InvalidOperationException("Conditional respawn points must be an array.");

            if (value is CashPackage cashPackage)
                CheckBalanceNumber(cashPackage.CashAmount, "cashAmount");
            if (value is ZoomWeaponAbility zoom && (zoom.zoomingFov <= 0 || zoom.zoomingFov >= 180))
                throw new InvalidOperationException("Zoom FOV must be between 0 and 180 degrees.");
            if (value is PlayerTitle title && title.Buff != null)
                ValidateBuff(title.Buff, 1);
            if (value is Faction faction && faction.Buff != null)
                ValidateBuff(faction.Buff, 1);

            if (value is Attribute attribute)
            {
                CheckBalanceNumber(attribute.BattlePointScore, "battlePointScore");
                if (attribute.IncreaseStatusEffectResistances == null)
                    throw new InvalidOperationException("Attribute status effect resistances must be an array.");
                foreach (var resistance in attribute.IncreaseStatusEffectResistances)
                    if (resistance.statusEffect == null)
                        throw new InvalidOperationException("Attribute resistance must reference a status effect.");
            // Signed stat/resistance bonuses and nonpositive (unlimited) attribute caps are valid.
            }

            if (value is ItemRandomBonus randomBonus)
            {
                CheckBalanceNumber(randomBonus.maxRandomStatsAmount, "maxRandomStatsAmount");
                if (randomBonus.randomAttributeAmounts == null || randomBonus.randomAttributeAmountRates == null || randomBonus.randomSkillLevels == null)
                    throw new InvalidOperationException("Random bonus entries must be arrays.");
            }

            if (value is AttributeRandomAmount randomAttribute && (randomAttribute.attribute == null || randomAttribute.minAmount > randomAttribute.maxAmount))
                throw new InvalidOperationException("Random attribute requires a reference and an ordered range.");
            if (value is RandomCharacterStats randomStats)
            {
                foreach (FieldInfo field in Fields(typeof(RandomCharacterStats)))
                {
                    if (!field.Name.StartsWith("min", StringComparison.Ordinal))
                        continue;
                    FieldInfo max = typeof(RandomCharacterStats).GetField("max" + field.Name.Substring(3));
                    if (max != null && (float)field.GetValue(randomStats) > (float)max.GetValue(randomStats))
                        throw new InvalidOperationException($"Random stat {field.Name} minimum exceeds maximum.");
                }
            }

            if (value is SkillRandomLevel randomSkill)
            {
                if (randomSkill.skill == null || randomSkill.minLevel < 0 || randomSkill.maxLevel < randomSkill.minLevel)
                    throw new InvalidOperationException("Random skill requires a reference and an ordered nonnegative level range.");
                CheckBalanceNumber(randomSkill.applyRate, "applyRate", 1);
            }

            if (value is SkillIncremental incrementalSkill && incrementalSkill.skill == null)
                throw new InvalidOperationException("Skill increment requires a skill reference.");
            if (value is SkillKnockback knockback)
            {
                CheckBalanceNumber(knockback.force, "knockback force");
                CheckBalanceNumber(knockback.deceleration, "knockback deceleration");
                CheckBalanceNumber(knockback.duration, "knockback duration");
            }

            if (value is MonsterSkill monsterSkill)
            {
                if (monsterSkill.skill == null || monsterSkill.level < 1)
                    throw new InvalidOperationException("Monster skill requires a skill reference and positive level.");
                CheckBalanceNumber(monsterSkill.useRate, "useRate", 1);
                CheckBalanceNumber(monsterSkill.useWhenHpRate, "useWhenHpRate", 1);
            }

            if (value is StatusEffect statusEffect)
            {
                if (statusEffect.Buff == null || statusEffect.MaxResistanceAmountEachLevels == null)
                    throw new InvalidOperationException("Status effect requires a buff and a resistance-cap array.");
                CheckBalanceNumber(statusEffect.MaxResistanceAmount, "maxResistanceAmount");
                foreach (float resistance in statusEffect.MaxResistanceAmountEachLevels)
                    CheckBalanceNumber(resistance, "maxResistanceAmountEachLevels", 1);
                // Status effects have no declared maximum level; validate their base buff values.
                ValidateBuff(statusEffect.Buff, 1);
            }

            if (value is PetItem pet)
                ValidateUseRequirement(pet.Requirement);
            if (value is MountItem mount)
                ValidateUseRequirement(mount.Requirement);
            if (value is ItemRequirement requirement)
                ValidateUseRequirement(requirement);
            if (value is GuildSkill guildSkill)
                ValidateGuildSkill(guildSkill);
            if (value is SocketEnhancerItem socket)
            {
                if (socket.SocketEnhanceEffect == null)
                    throw new InvalidOperationException("Socket enhancer bonus cannot be null.");
                ValidateEquipmentBonus(socket.SocketEnhanceEffect);
                foreach (StatusEffectApplying[] effects in new[]{socket.SelfStatusEffectsWhenAttacking, socket.EnemyStatusEffectsWhenAttacking, socket.SelfStatusEffectsWhenAttacked, socket.EnemyStatusEffectsWhenAttacked})
                {
                    if (effects == null)
                        throw new InvalidOperationException("Socket status effects must be arrays.");
                    foreach (StatusEffectApplying effect in effects)
                        if (effect.statusEffect == null || effect.buffLevel.GetAmount(1) < 1)
                            throw new InvalidOperationException("Socket status effects require existing references and a positive level.");
                }
            }

            if (value is EquipmentSet equipmentSet)
            {
                if (equipmentSet.Effects == null || equipmentSet.Effects.Any(effect => effect == null))
                    throw new InvalidOperationException("Equipment set tiers must be a non-null array of bonuses.");
                foreach (EquipmentBonus effect in equipmentSet.Effects)
                    ValidateEquipmentBonus(effect);
            }

            if (value is EquipmentBonus bonus)
                ValidateEquipmentBonus(bonus);
            if (value is ItemRefine refine && (refine.Levels == null || refine.RepairPrices == null || refine.Levels.Any(row => row == null) || refine.RepairPrices.Any(row => row == null)))
                throw new InvalidOperationException("Refining and repair tables must contain non-null arrays and rows.");
            if (value is ItemRefine refineTables)
            {
                foreach (ItemRefineLevel row in refineTables.Levels)
                    ValidateObject(row);
                foreach (ItemRepairPrice row in refineTables.RepairPrices)
                    ValidateObject(row);

            }

            if (value is ItemRefineLevel refineLevel)
            {
                CheckChance(refineLevel.SuccessRate, "successRate");
                CheckBalanceNumber(refineLevel.RequireGold, "requireGold");
                CheckBalanceNumber(refineLevel.RefineFailDecreaseLevels, "refineFailDecreaseLevels");
                if (refineLevel.RequireItems == null || refineLevel.AvailableEnhancers == null || refineLevel.FailReturnings == null || refineLevel.AvailableEnhancers.Any(row => row == null) || refineLevel.FailReturnings.Any(row => row == null))
                    throw new InvalidOperationException("Refining materials, enhancers, and refunds must be non-null arrays/entries.");
                ValidateBalanceCurrencies(refineLevel.RequireCurrencies);
                foreach (ItemAmount row in refineLevel.RequireItems)
                    ValidateObject(row);
                foreach (ItemRefineEnhancer row in refineLevel.AvailableEnhancers)
                    ValidateObject(row);
                foreach (ItemRefineFailReturning row in refineLevel.FailReturnings)
                    ValidateObject(row);
                double weight = refineLevel.FailReturnings.Sum(row => (double)row.randomWeight);
                if (refineLevel.FailReturnings.Length > 0 && (weight <= 0 || weight > float.MaxValue))
                    throw new InvalidOperationException("Refining refunds require a finite positive total weight.");
            }

            if (value is ItemRefineEnhancer enhancer)
            {
                if (enhancer.item == null)
                    throw new InvalidOperationException("Refining enhancer must reference an item.");
                CheckChance(enhancer.increaseSuccessRate, "increaseSuccessRate");
                CheckChance(enhancer.decreaseRequireGoldRate, "decreaseRequireGoldRate");
                CheckChance(enhancer.chanceToNotDecreaseLevels, "chanceToNotDecreaseLevels");
                CheckChance(enhancer.chanceToNotDestroyItem, "chanceToNotDestroyItem");
            }

            if (value is ItemRefineFailReturning refund)
            {
                CheckBalanceNumber(refund.randomWeight, "randomWeight");
                CheckBalanceNumber(refund.returnGold, "returnGold");
                ValidateBalanceItems(refund.returnItems);
                ValidateBalanceCurrencies(refund.returnCurrencies);
            }

            if (value is ItemRepairPrice repair)
            {
                if (repair.DurabilityRate <= 0)
                    throw new InvalidOperationException("Repair durability threshold must be positive.");
                CheckChance(repair.DurabilityRate, "durabilityRate");
                CheckBalanceNumber(repair.RequireGold, "requireGold");
                ValidateBalanceItems(repair.RequireItems);
                ValidateBalanceCurrencies(repair.RequireCurrencies);
            }

            if (value is BaseSkill skill)
                ValidateSkill(skill);
            if (value is WeaponItem weapon)
                ValidateWeapon(weapon);
            if (value is BaseEquipmentItem equipment)
                ValidateEquipment(equipment);
            if (value is BaseItem item && item is IUsableItem usable)
                ValidateConsumable(item, usable);
            if (value is ItemCraft craft)
                ValidateCraft(craft);
            if (value is ItemCraftFormula formula && (formula.ItemCraft == null || formula.CraftDuration < 0))
                throw new InvalidOperationException("Craft formula requires a recipe and nonnegative duration.");
            if (value is Skill craftingSkill && craftingSkill.itemCraft == null)
                throw new InvalidOperationException("Skill crafting recipe cannot be null.");
            if (value is Quest quest)
                ValidateQuestRewards(quest);
            if (value is Gacha gacha)
                ValidateGacha(gacha);
            if (value is CashShopItem offer)
                ValidateCashShop(offer);
            if (value is NpcSellItem shopRow)
                ValidateShopRow(shopRow);
            if (value is NpcDialog dialog && dialog.sellItems == null)
                throw new InvalidOperationException("NPC shop rows must be non-null arrays/entries.");
            if (value is Harvestable harvestable && (harvestable.harvestEffectivenesses == null || harvestable.skillHarvestEffectivenesses == null))
                throw new InvalidOperationException("Harvest rules must be arrays, not null.");
            if (value is HarvestEffectiveness harvest && harvest.items == null || value is SkillHarvestEffectiveness skillHarvest && skillHarvest.items == null)
                throw new InvalidOperationException("Harvest item entries must be an array, not null.");
            if (value is ItemDrop drop && (drop.maxAmount < Math.Max(1, drop.minAmount) || drop.maxLevel < Math.Max(1, drop.minLevel)))
                throw new InvalidOperationException("Invalid item drop amount or level range.");
            if (value is ItemRandomByWeight weightedDrop && (weightedDrop.randomWeight < 0 || weightedDrop.item != null && (weightedDrop.maxAmount < Math.Max(1, weightedDrop.minAmount) || weightedDrop.maxLevel < Math.Max(1, weightedDrop.minLevel))))
                throw new InvalidOperationException("Invalid weighted drop.");
            if (value is ItemDropManager manager && manager.maxDropItems < manager.minDropItems)
                throw new InvalidOperationException("Invalid drop count range.");
            if (value is ItemAmount amount && (amount.amount < 0 || amount.level < 0))
                throw new InvalidOperationException("Invalid ItemAmount.");
            if (value is MinMaxFloat floats && floats.min > floats.max || value is MinMaxInt ints && ints.min > ints.max)
                throw new InvalidOperationException("Minimum exceeds maximum.");
            if (value is CurrencyRandomAmount currency && (currency.maxAmount < Math.Max(1, currency.minAmount)))
                throw new InvalidOperationException("Invalid currency range.");
        }

        private static void CheckChance(float value, string field)
        {
            CheckBalanceNumber(value, field);
            if (value > 1)
                throw new InvalidOperationException($"{field} must be between zero and one.");
        }

        private static void ValidateEquipmentBonus(EquipmentBonus bonus)
        {
            // Inspect serialized fields rather than lazy dictionaries copied from the live bonus.
            foreach (FieldInfo field in Fields(typeof(EquipmentBonus)))
            {
                object data = field.GetValue(bonus);
                if (data == null)
                    throw new InvalidOperationException($"Equipment bonus {field.Name} cannot be null.");
                if (data is AttributeAmount[] attributes)
                    foreach (AttributeAmount entry in attributes)
                        if (entry.attribute == null || float.IsNaN(entry.amount) || float.IsInfinity(entry.amount))
                            throw new InvalidOperationException("Equipment attributes require a valid reference and finite amount.");
                if (data is SkillLevel[] skills)
                {
                    var totals = new Dictionary<BaseSkill, long>();
                    foreach (SkillLevel entry in skills)
                    {
                        if (entry.skill == null || entry.level < 0)
                            throw new InvalidOperationException("Equipment skills require a valid reference and nonnegative bonus level.");
                        totals.TryGetValue(entry.skill, out long total);
                        if (total + entry.level > int.MaxValue)
                            throw new InvalidOperationException("Equipment skill bonus total overflows.");
                        totals[entry.skill] = total + entry.level;
                    }
                }

                if (data is StatusEffectResistanceAmount[] resistances)
                    foreach (StatusEffectResistanceAmount entry in resistances)
                        if (entry.statusEffect == null || float.IsNaN(entry.amount) || float.IsInfinity(entry.amount))
                            throw new InvalidOperationException("Equipment resistances require a valid reference and finite amount.");
            }
        // Stat, attribute, and resistance bonuses can be signed; gameplay combines them with other sources.
        }

        private static void ValidateBalanceItems(ItemAmount[] items)
        {
            if (items == null)
                throw new InvalidOperationException("Item amounts must be an array.");
            foreach (ItemAmount item in items)
                if (item.item == null || item.amount < 1 || item.level < 0)
                    throw new InvalidOperationException("Item amounts require an item, positive amount, and nonnegative level.");
        }

        private static void ValidateBalanceCurrencies(CurrencyAmount[] currencies)
        {
            if (currencies == null)
                throw new InvalidOperationException("Currency amounts must be an array.");
            var totals = new Dictionary<Currency, int>();
            foreach (CurrencyAmount currency in currencies)
            {
                if (currency.currency == null || currency.amount < 1)
                    throw new InvalidOperationException("Currency amounts require a currency and positive amount.");
                totals.TryGetValue(currency.currency, out int total);
                if (currency.amount > int.MaxValue - total)
                    throw new InvalidOperationException("Currency amount total overflows.");
                totals[currency.currency] = total + currency.amount;
            }
        }

        private static void ValidateWeapon(WeaponItem weapon)
        {
            if (weapon.WeaponType.DamageInfo == null)
                throw new InvalidOperationException("Weapon damage info cannot be null.");
            foreach (FieldInfo field in Fields(typeof(WeaponItem)))
            {
                object value = field.GetValue(weapon);
                if (field.Name == "ammoItems")
                {
                    if (value == null)
                        throw new InvalidOperationException("Weapon ammo items must be an array.");
                    foreach (BaseItem ammo in (BaseItem[])value)
                        if (ammo == null)
                            throw new InvalidOperationException("Weapon ammo entries cannot be null.");
                }

            }

            for (int level = 1; level <= weapon.MaxLevel; ++level)
            {
                ValidateWeaponDamage(weapon.DamageAmount.amount.GetAmount(level));
                ValidateWeaponDamage(weapon.HarvestDamageAmount.GetAmount(level));
            }

            DamageInfo damage = weapon.WeaponType.DamageInfo;
            CheckWeaponNumber(damage.hitDistance, "hitDistance");
            CheckWeaponNumber(damage.missileDistance, "missileDistance");
            CheckWeaponNumber(damage.missileSpeed, "missileSpeed");
            CheckWeaponNumber(damage.throwForce, "throwForce");
            CheckWeaponNumber(damage.throwableLifeTime, "throwableLifeTime");
            if (damage.damageType == DamageType.Melee && (damage.hitFov < 10 || damage.hitFov > 360))
                throw new InvalidOperationException("Melee hitFov must be between 10 and 360.");
        // startAttackDistance <= 0 and per-view recoil/spread sentinels keep their existing fallback semantics.
        }

        private static void ValidateEquipment(BaseEquipmentItem equipment)
        {
            if (equipment.RandomBonus == null)
                throw new InvalidOperationException("Equipment random bonus cannot be null.");
            if (float.IsNaN(equipment.MaxDurability) || float.IsInfinity(equipment.MaxDurability) || equipment.MaxDurability < 0)
                throw new InvalidOperationException("Equipment maximum durability must be finite and nonnegative.");
            if (equipment.IncreaseAttributes == null || equipment.IncreaseAttributesRate == null || equipment.IncreaseStatusEffectResistances == null)
                throw new InvalidOperationException("Equipment bonus entries must be arrays, not null.");
            foreach (StatusEffectResistanceIncremental resistance in equipment.IncreaseStatusEffectResistances)
            {
                if (resistance.statusEffect == null)
                    throw new InvalidOperationException("Equipment resistance must reference a status effect.");
                for (int level = 1; level <= equipment.MaxLevel; ++level)
                {
                    float amount = resistance.amount.GetAmount(level);
                    if (float.IsNaN(amount) || float.IsInfinity(amount))
                        throw new InvalidOperationException("Equipment resistance overflows at a supported item level.");
                }
            }
        // Resistance/stat bonuses can be signed and are combined with the character's other bonuses.
        }

        private static void ValidateUseRequirement(ItemRequirement requirement)
        {
            if (requirement == null || requirement.level < 0 || requirement.attributeAmounts == null)
                throw new InvalidOperationException("Item use requirements need a nonnegative level and an attribute array.");
            var totals = new Dictionary<Attribute, double>();
            foreach (AttributeAmount entry in requirement.attributeAmounts)
            {
                if (entry.attribute == null)
                    throw new InvalidOperationException("Item use requirement must reference an attribute.");
                CheckBalanceNumber(entry.amount, "required attribute amount");
                totals.TryGetValue(entry.attribute, out double total);
                if (total + entry.amount > float.MaxValue)
                    throw new InvalidOperationException("Required attribute total overflows.");
                totals[entry.attribute] = total + entry.amount;
            }
        }

        private static void ValidateConsumable(BaseItem item, IUsableItem usable)
        {
            foreach (FieldInfo field in Fields(item.GetType()))
            {
                if (field.Name == "useItemCooldown")
                    CheckBalanceNumber(usable.UseItemCooldown, "useItemCooldown");
                if (field.Name == "exp" || field.Name == "maxDropAmount")
                    CheckBalanceNumber((int)field.GetValue(item), field.Name);
                if (field.Name == "rewardingItems")
                {
                    var rewards = (ItemAmount[])field.GetValue(item);
                    if (rewards == null)
                        throw new InvalidOperationException("Consumable rewards must be an array.");
                    foreach (ItemAmount reward in rewards)
                        if (reward.item == null)
                            throw new InvalidOperationException("Consumable rewards must reference an item.");
                }
            }

            if (!(item is IPotionItem potion))
                return;
            if (potion.BuffData == null)
                throw new InvalidOperationException("Potion buff cannot be null.");
            for (int level = 1; level <= item.MaxLevel; ++level)
            {
                Buff buff = potion.BuffData;
                ValidateBuff(buff, level);
                float duration = buff.duration.GetAmount(level);
                if (float.IsNaN(duration) || float.IsInfinity(duration))
                    throw new InvalidOperationException("Potion duration overflows at a supported item level.");
                CheckBalanceNumber(buff.recoveryHp.GetAmount(level), "recoveryHp");
                CheckBalanceNumber(buff.recoveryMp.GetAmount(level), "recoveryMp");
                CheckBalanceNumber(buff.recoveryStamina.GetAmount(level), "recoveryStamina");
                CheckBalanceNumber(buff.recoveryFood.GetAmount(level), "recoveryFood");
                CheckBalanceNumber(buff.recoveryWater.GetAmount(level), "recoveryWater");
            }
        }

        private static void ValidateCraft(ItemCraft craft)
        {
            foreach (FieldInfo field in Fields(typeof(ItemCraft)))
            {
                object value = field.GetValue(craft);
                if (value is int count && (count < 0 || field.Name == "amount" && count < 1))
                    throw new InvalidOperationException($"Craft {field.Name} is invalid.");
            }

            if (craft.RequireItems == null || craft.RequireCurrencies == null)
                throw new InvalidOperationException("Craft requirements must be arrays.");
            foreach (ItemAmount required in craft.RequireItems)
                if (required.item == null)
                    throw new InvalidOperationException("Craft material must reference an item.");
            foreach (CurrencyAmount required in craft.RequireCurrencies)
                if (required.currency == null || required.amount < 0)
                    throw new InvalidOperationException("Craft currency must reference a currency and have a nonnegative amount.");
        }

        private static void ValidateQuestRewards(Quest quest)
        {
            CheckBalanceNumber(quest.rewardExp, "rewardExp");
            CheckBalanceNumber(quest.rewardGold, "rewardGold");
            CheckBalanceNumber(quest.rewardStatPoints, "rewardStatPoints");
            CheckBalanceNumber(quest.rewardSkillPoints, "rewardSkillPoints");
            if (quest.rewardCurrencies == null || quest.rewardItems == null || quest.selectableRewardItems == null || quest.randomRewardItems == null)
                throw new InvalidOperationException("Quest reward fields must be arrays.");
            if (quest.selectableRewardItems.Length > 256)
                throw new InvalidOperationException("Quest reward selection supports at most 256 entries.");
            foreach (ItemAmount reward in quest.rewardItems.Concat(quest.selectableRewardItems))
                if (reward.item == null || reward.amount < 1 || reward.level < 0)
                    throw new InvalidOperationException("Quest item rewards require an item and positive amount.");
            var totals = new Dictionary<Currency, int>();
            foreach (CurrencyAmount reward in quest.rewardCurrencies)
            {
                if (reward.currency == null || reward.amount < 1)
                    throw new InvalidOperationException("Quest currency rewards require a currency and positive amount.");
                totals.TryGetValue(reward.currency, out int previous);
                if (reward.amount > int.MaxValue - previous)
                    throw new InvalidOperationException("Quest currency reward total overflows.");
                totals[reward.currency] = previous + reward.amount;
            }

            double weight = 0;
            foreach (ItemRandomByWeight reward in quest.randomRewardItems)
            {
                if (reward.randomWeight < 0 || float.IsNaN(reward.randomWeight) || float.IsInfinity(reward.randomWeight))
                    throw new InvalidOperationException("Invalid quest reward weight.");
                if (reward.item != null && reward.maxAmount > 0 && reward.randomWeight > 0)
                    weight += reward.randomWeight;
            }

            if (quest.randomRewardItems.Length > 0 && (weight <= 0 || weight > float.MaxValue))
                throw new InvalidOperationException("Quest random rewards require a finite positive total weight for valid items.");
        }

        private static void ValidateGacha(Gacha gacha)
        {
            if (gacha.SingleModeOpenPrice < 0 || gacha.MultipleModeOpenPrice < 0 || gacha.MultipleModeOpenCount < 1 || gacha.MultipleModeOpenCount > 20000 || gacha.RandomItems == null)
                throw new InvalidOperationException("Invalid gacha prices, draw count or pool.");
            double weight = 0;
            foreach (ItemRandomByWeight item in gacha.RandomItems)
                if (item.item != null && item.maxAmount > 0 && item.randomWeight > 0)
                    weight += item.randomWeight;
            if (weight <= 0 || weight > float.MaxValue)
                throw new InvalidOperationException("Gacha requires a finite positive usable reward weight.");
        }

        private static void ValidateCashShop(CashShopItem offer)
        {
            CheckBalanceNumber(offer.SellPriceGold, "sellPriceGold");
            CheckBalanceNumber(offer.SellPriceCash, "sellPriceCash");
            CheckBalanceNumber(offer.ReceiveGold, "receiveGold");
            if (offer.ReceiveItems == null || offer.ReceiveCurrencies == null)
                throw new InvalidOperationException("Cash shop reward fields must be arrays.");
            foreach (ItemAmount item in offer.ReceiveItems)
                if (item.item == null || item.amount < 1)
                    throw new InvalidOperationException("Cash shop items require a reference and positive amount.");
            foreach (CurrencyAmount currency in offer.ReceiveCurrencies)
                if (currency.currency == null || currency.amount < 1)
                    throw new InvalidOperationException("Cash shop currencies require a reference and positive amount.");
        }

        private static void ValidateShopRow(NpcSellItem row)
        {
            CheckBalanceNumber(row.sellPrice, "sellPrice");
            CheckBalanceNumber(row.level, "level");
            CheckBalanceNumber(row.amount, "amount");
            if (row.item == null)
                throw new InvalidOperationException("NPC item rows require an item.");
            if (row.sellPrices != null)
                foreach (CurrencyAmount price in row.sellPrices)
                    if (price.currency == null || price.amount < 0)
                        throw new InvalidOperationException("Invalid NPC custom currency price.");
        // amount == 0 keeps the existing maximum-stack fallback. Null sellPrices means no custom prices.
        }

        private static void ValidateWeaponDamage(MinMaxFloat range)
        {
            CheckWeaponNumber(range.min, "damage minimum");
            CheckWeaponNumber(range.max, "damage maximum");
            if (range.min > range.max)
                throw new InvalidOperationException("Weapon damage minimum exceeds maximum.");
        }

        private static void CheckWeaponNumber(double value, string field)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
                throw new InvalidOperationException($"Weapon {field} must be finite and nonnegative.");
        }

        private static void ValidateSkillRequirementReferences(AttributeAmount[] attributes, SkillLevel[] skills)
        {
            if (attributes == null || skills == null)
                throw new InvalidOperationException("Skill requirement references must be arrays.");
            foreach (var attribute in attributes)
                if (attribute.attribute == null || attribute.amount < 0)
                    throw new InvalidOperationException("Invalid required attribute.");
            foreach (var skill in skills)
                if (skill.skill == null || skill.level < 0 || skill.level > skill.skill.maxLevel)
                    throw new InvalidOperationException("Invalid required skill.");
        }

        private static void ValidateSkill(BaseSkill skill)
        {
            CheckBalanceNumber(skill.battlePointScore, "battlePointScore");
            if (skill.requirement == null || skill.requirementEachLevels == null || skill.requirementEachLevels.Count == 0)
                throw new InvalidOperationException("Skill requires requirement settings and at least one effective requirement row.");
            foreach (var row in skill.requirementEachLevels)
            {
                if (row == null)
                    throw new InvalidOperationException("Skill requirement rows cannot be null.");
                ValidateObject(row);
            }

            if (skill.requireItems == null || skill.requireAmmoAmount < 0)
                throw new InvalidOperationException("Skill item requirements must be an array and ammo cost must be nonnegative.");
            // Negative per-level increments are valid when every supported level remains valid.
            for (int level = 1; level <= skill.maxLevel; ++level)
            {
                CheckBalanceNumber(skill.requirement.characterLevel.GetAmount(level), "required character level");
                CheckBalanceNumber(skill.requirement.gold.GetAmount(level), "required soft currency");
                CheckBalanceNumber(skill.requirement.skillPoint.GetAmount(level), "required skill points");
                if (skill is Skill summoningSkill)
                {
                    var summon = summoningSkill.summon;
                    if (summon == null || summoningSkill.mount == null || summoningSkill.damageInfo == null)
                        throw new InvalidOperationException("Skill summon, mount and damage settings cannot be null.");
                    CheckBalanceNumber(summon.AmountEachTime.GetAmount(level), "summon amount");
                    CheckBalanceNumber(summon.MaxStack.GetAmount(level), "summon max stack");
                    CheckBalanceNumber(summon.Level.GetAmount(level), "summon level");
                    float duration = summon.Duration.GetAmount(level);
                    if (float.IsNaN(duration) || float.IsInfinity(duration))
                        throw new InvalidOperationException("Summon duration must be finite.");
                // Duration <= 0 preserves the existing immediate-death behavior.
                }

                CheckBalanceNumber(skill.consumeHp.GetAmount(level), "consumeHp");
                CheckBalanceNumber(skill.consumeMp.GetAmount(level), "consumeMp");
                CheckBalanceNumber(skill.consumeStamina.GetAmount(level), "consumeStamina");
                CheckBalanceNumber(skill.consumeHpRate.GetAmount(level), "consumeHpRate", 1);
                CheckBalanceNumber(skill.consumeMpRate.GetAmount(level), "consumeMpRate", 1);
                CheckBalanceNumber(skill.consumeStaminaRate.GetAmount(level), "consumeStaminaRate", 1);
                CheckBalanceNumber(skill.coolDownDuration.GetAmount(level), "coolDownDuration");
                CheckBalanceNumber(skill.castDuration.GetAmount(level), "castDuration");
                foreach (FieldInfo field in Fields(skill.GetType()))
                {
                    object fieldValue = field.GetValue(skill);
                    if (field.FieldType == typeof(Buff))
                        ValidateBuff((Buff)fieldValue, level);
                    if (fieldValue is IncrementalMinMaxFloat damage)
                    {
                        MinMaxFloat range = damage.GetAmount(level);
                        if (range.min > range.max)
                            throw new InvalidOperationException($"{field.Name} minimum exceeds maximum at level {level}.");
                    }

                    if (fieldValue is IncrementalFloat number && (field.Name == "castDistance" || field.Name == "buffDistance" || field.Name == "areaDuration" || field.Name == "applyDuration" || field.Name == "weaponDamageMultiplicator"))
                        CheckBalanceNumber(number.GetAmount(level), field.Name);
                }
            }
        }

        private static void ValidateGuildSkill(GuildSkill skill)
        {
            if (skill.Buff == null || skill.MaxLevel < 1 || skill.MaxLevel > 100)
                throw new InvalidOperationException("Guild skill requires a buff and supported maximum level.");
            for (int level = 1; level <= skill.MaxLevel; ++level)
            {
                foreach (FieldInfo field in Fields(typeof(GuildSkill)))
                    if (field.GetValue(skill)is IncrementalFloat value)
                        CheckBalanceNumber(value.GetAmount(level), field.Name, field.Name == "decreaseExpLostPercentage" ? 1 : double.MaxValue);
                CheckBalanceNumber(skill.GetIncreaseMaxMember(level), "increaseMaxMember");
                ValidateBuff(skill.Buff, level);
                float duration = skill.Buff.duration.GetAmount(level);
                if (float.IsNaN(duration) || float.IsInfinity(duration))
                    throw new InvalidOperationException("Guild buff duration overflows at a supported level.");
                foreach (FieldInfo field in Fields(typeof(Buff)))
                    if (field.Name.StartsWith("recovery", StringComparison.Ordinal) && field.GetValue(skill.Buff)is IncrementalInt recovery)
                        CheckBalanceNumber(recovery.GetAmount(level), field.Name);
            }
        // Gain bonuses use percentage points; EXP-loss reduction uses a fraction from zero to one.
        }

        private static void ValidateBuff(Buff buff, int level)
        {
            if (buff == null)
                return;
            if (buff.buffRemovals == null)
                throw new InvalidOperationException("Buff removal/condition rows must be arrays.");
            foreach (BuffRemoval removal in buff.buffRemovals)
            {
                if (removal == null)
                    throw new InvalidOperationException("Buff removal rows cannot be null.");
                CheckBalanceNumber(removal.removalChance.GetAmount(level), "buff removal chance");
            }

            if (buff.increaseAttributes == null || buff.increaseAttributesRate == null)
                throw new InvalidOperationException("Buff attribute entries must be arrays.");
            foreach (FieldInfo field in Fields(typeof(Buff)))
                if (field.Name.EndsWith("Chance", StringComparison.Ordinal) && field.GetValue(buff) is IncrementalFloat chance)
                    CheckBalanceNumber(chance.GetAmount(level), field.Name, 1);
            CheckBalanceNumber(buff.maxStack.GetAmount(level), "maxStack");
        // Duration <= 0 is the existing instant recovery mode; signed stat changes are debuffs.
        }

        private static void CheckBalanceNumber(double value, string field, double maximum = double.MaxValue)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value > maximum)
                throw new InvalidOperationException($"Patch field {field} is outside its allowed range.");
        }
    }
}
