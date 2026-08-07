#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class CodeGen_CharacterStatsUIClasses
    {
        private const string LocaleKeyPrefix = "UI_FORMAT_";

        [MenuItem("MMORPG KIT/Code Generation/Generate Character Stats UI Classes")]
        public static void Generate()
        {
            CharacterStatFieldData[] fields = GetStatFields();

            if (fields.Length == 0)
            {
                Debug.LogWarning(
                    $"No float fields using {nameof(CharacterStatTextGenAttribute)} " +
                    $"were found in {nameof(CharacterStats)}.");

                return;
            }

            string characterStatsTextPath = EditorUtility.SaveFilePanel(
                "Save CharacterStatsTextGenerateData",
                Application.dataPath,
                "CharacterStatsTextGenerateData.cs",
                "cs");

            if (string.IsNullOrWhiteSpace(characterStatsTextPath))
                return;

            string uiBaseEquipmentBonusPath = EditorUtility.SaveFilePanel(
                "Save UIBaseEquipmentBonus",
                Path.GetDirectoryName(characterStatsTextPath),
                "UIBaseEquipmentBonus.cs",
                "cs");

            if (string.IsNullOrWhiteSpace(uiBaseEquipmentBonusPath))
                return;

            string uiCharacterStatsPath = EditorUtility.SaveFilePanel(
                "Save UICharacterStats",
                Path.GetDirectoryName(uiBaseEquipmentBonusPath),
                "UICharacterStats.cs",
                "cs");

            if (string.IsNullOrWhiteSpace(uiCharacterStatsPath))
                return;

            WriteCodeFile(
                characterStatsTextPath,
                GenerateCharacterStatsTextCode(fields));

            WriteCodeFile(
                uiBaseEquipmentBonusPath,
                GenerateUIBaseEquipmentBonusCode(fields));

            WriteCodeFile(
                uiCharacterStatsPath,
                GenerateUICharacterStatsCode(fields));

            AssetDatabase.Refresh();

            Debug.Log(
                $"Generated {fields.Length} character stat UI definitions.\n\n" +
                $"{characterStatsTextPath}\n" +
                $"{uiBaseEquipmentBonusPath}\n" +
                $"{uiCharacterStatsPath}");
        }

        private static void WriteCodeFile(
            string path,
            string content)
        {
            string directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(
                path,
                content,
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: false));
        }

        private static CharacterStatFieldData[] GetStatFields()
        {
            return typeof(CharacterStats)
                .GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                .Select(field => new
                {
                    Field = field,
                    Attribute = field.GetCustomAttribute<
                        CharacterStatTextGenAttribute>(),
                })
                .Where(entry =>
                    entry.Attribute != null &&
                    entry.Field.FieldType == typeof(float))
                .OrderBy(entry => entry.Field.MetadataToken)
                .Select(entry => new CharacterStatFieldData(
                    entry.Field,
                    entry.Attribute))
                .ToArray();
        }

        #region CharacterStatsTextGenerateData

        private static string GenerateCharacterStatsTextCode(
            CharacterStatFieldData[] fields)
        {
            StringBuilder builder = new StringBuilder(128 * 1024);

            AppendGeneratedHeader(builder);

            builder.AppendLine("using Cysharp.Text;");
            builder.AppendLine("using Insthync.DevExtension;");
            builder.AppendLine("using System.Text;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();

            builder.AppendLine("namespace MultiplayerARPG");
            builder.AppendLine("{");
            builder.AppendLine("    [System.Serializable]");
            builder.AppendLine(
                "    public partial class CharacterStatsTextGenerateData");
            builder.AppendLine("    {");

            AppendCharacterStatsTextFields(builder, fields);
            AppendCharacterStatsGetText(builder, fields);
            AppendGetSingleStatsText(builder);
            AppendGetBooleanStatsText(builder);

            builder.AppendLine("    }");
            builder.AppendLine("}");

            return builder.ToString();
        }

        private static void AppendCharacterStatsTextFields(
            StringBuilder builder,
            CharacterStatFieldData[] fields)
        {
            builder.AppendLine(
                "        public CharacterStats data;");
            builder.AppendLine(
                "        public bool isRate;");
            builder.AppendLine(
                "        public bool isBonus;");

            foreach (CharacterStatFieldData field in fields)
            {
                builder
                    .Append("        public string ")
                    .Append(field.StatsFormatFieldName)
                    .Append(" = \"")
                    .Append(EscapeString(field.NormalUIFormatKey))
                    .AppendLine("\";");
            }

            foreach (CharacterStatFieldData field in fields)
            {
                builder
                    .Append("        public TextWrapper ")
                    .Append(field.TextWrapperFieldName)
                    .AppendLine(";");
            }

            builder.AppendLine();
            builder.AppendLine(
                "        public string numberFormatSimple = \"N0\";");
            builder.AppendLine(
                "        public string numberFormatRate = \"N2\";");
        }

        private static void AppendCharacterStatsGetText(
            StringBuilder builder,
            CharacterStatFieldData[] fields)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        public string GetText(Color bonusIncreaseColor, Color bonusDecreaseColor)");
            builder.AppendLine("        {");
            builder.AppendLine(
                "            StringBuilder statsStringBuilder = new StringBuilder();");

            foreach (CharacterStatFieldData field in fields)
            {
                builder.AppendLine();

                builder
                    .Append("            // ")
                    .AppendLine(field.DisplayName);

                builder
                    .Append(
                        "            GetSingleStatsText(statsStringBuilder, isRate || ")
                    .Append(field.IsRate ? "true" : "false")
                    .Append(", LanguageManager.GetText(")
                    .Append(field.StatsFormatFieldName)
                    .Append("), data.")
                    .Append(field.FieldName)
                    .Append(", ")
                    .Append(field.TextWrapperFieldName)
                    .AppendLine(
                        ", bonusIncreaseColor, bonusDecreaseColor);");
            }

            builder.AppendLine();
            AppendCharacterStatsDevExtensionComment(builder);

            builder.AppendLine(
                "            this.InvokeInstanceDevExtMethods(\"GetText\", statsStringBuilder, bonusIncreaseColor, bonusDecreaseColor);");

            builder.AppendLine();
            builder.AppendLine(
                "            return statsStringBuilder.ToString();");
            builder.AppendLine("        }");
        }

        private static void AppendCharacterStatsDevExtensionComment(
            StringBuilder builder)
        {
            builder.AppendLine("            // Dev Extension");
            builder.AppendLine("            // How to implement it?:");
            builder.AppendLine("            // /*");
            builder.AppendLine(
                "            //  * - Add `customStat1` to `CharacterStats` partial class file");
            builder.AppendLine(
                "            //  * - Add `customStat1StatsFormat` to `CharacterStatsTextGenerateData`");
            builder.AppendLine(
                "            //  * - Add `uiTextCustomStat1` to `CharacterStatsTextGenerateData`");
            builder.AppendLine("            //  */");
            builder.AppendLine(
                "            // [DevExtMethods(\"GetText\")]");
            builder.AppendLine(
                "            // public void GetText_Ext(StringBuilder statsString)");
            builder.AppendLine("            // {");
            builder.AppendLine("            //   string tempValue;");
            builder.AppendLine("            //   string statsStringPart;");
            builder.AppendLine(
                "            //   tempValue = isRate ? (data.customStat1 * 100).ToString(\"N2\") : data.customStat1.ToString(\"N0\");");
            builder.AppendLine(
                "            //   statsStringPart = ZString.Format(LanguageManager.GetText(customStat1StatsFormat), tempValue);");
            builder.AppendLine(
                "            //   if (data.customStat1 != 0)");
            builder.AppendLine("            //   {");
            builder.AppendLine(
                "            //       if (statsString.Length > 0)");
            builder.AppendLine(
                "            //           statsString.Append('\\n');");
            builder.AppendLine(
                "            //       statsString.Append(statsStringPart);");
            builder.AppendLine("            //   }");
            builder.AppendLine(
                "            //   if (uiTextCustomStat1 != null)");
            builder.AppendLine(
                "            //       uiTextCustomStat1.text = statsStringPart;");
            builder.AppendLine("            // }");
        }

        private static void AppendGetSingleStatsText(
            StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        public void GetSingleStatsText(StringBuilder builder, bool isRateStats, string format, float value, TextWrapper textComponent, Color bonusIncreaseColor, Color bonusDecreaseColor)");
            builder.AppendLine("        {");
            builder.AppendLine(
                "            // Determine the correct format string based on whether the stat is a rate");
            builder.AppendLine(
                "            string numberFormat = isRateStats ? numberFormatRate : numberFormatSimple;");
            builder.AppendLine();
            builder.AppendLine(
                "            // Calculate the value to display, adjusting for rates if necessary");
            builder.AppendLine(
                "            string tempValue = isRateStats ? (value * 100).ToString(numberFormat) : value.ToString(numberFormat);");
            builder.AppendLine();
            builder.AppendLine(
                "            // Construct the display string");
            builder.AppendLine(
                "            string statsStringPart = ZString.Concat(isBonus && value >= 0 ? \"+\" : string.Empty, ZString.Format(");
            builder.AppendLine("                format,");
            builder.AppendLine("                tempValue));");
            builder.AppendLine();
            builder.AppendLine(
                "            // Append the stat text to the builder if the value is not zero");
            builder.AppendLine("            if (value != 0)");
            builder.AppendLine("            {");
            builder.AppendLine(
                "                if (builder.Length > 0)");
            builder.AppendLine(
                "                    builder.Append('\\n');");
            builder.AppendLine(
                "                builder.Append(statsStringPart);");
            builder.AppendLine("            }");
            builder.AppendLine();
            builder.AppendLine(
                "            // Set the text component if it's provided");
            builder.AppendLine(
                "            if (textComponent != null)");
            builder.AppendLine("            {");
            builder.AppendLine(
                "                if (value != 0f)");
            builder.AppendLine("                {");
            builder.AppendLine(
                "                    if (isBonus)");
            builder.AppendLine("                    {");
            builder.AppendLine(
                "                        if (value >= 0)");
            builder.AppendLine(
                "                            textComponent.color = bonusIncreaseColor;");
            builder.AppendLine(
                "                        else");
            builder.AppendLine(
                "                            textComponent.color = bonusDecreaseColor;");
            builder.AppendLine("                    }");
            builder.AppendLine(
                "                    textComponent.text = statsStringPart;");
            builder.AppendLine(
                "                    textComponent.SetGameObjectActive(true);");
            builder.AppendLine("                }");
            builder.AppendLine(
                "                else");
            builder.AppendLine("                {");
            builder.AppendLine(
                "                    textComponent.SetGameObjectActive(false);");
            builder.AppendLine("                }");
            builder.AppendLine("            }");
            builder.AppendLine("        }");
        }

        private static void AppendGetBooleanStatsText(
            StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        public void GetBooleanStatsText(StringBuilder builder, string text, bool value, TextWrapper textComponent)");
            builder.AppendLine("        {");
            builder.AppendLine(
                "            if (isRate)");
            builder.AppendLine(
                "                return;");
            builder.AppendLine(
                "            string statsStringPart = value ? text : string.Empty;");
            builder.AppendLine(
                "            if (value)");
            builder.AppendLine("            {");
            builder.AppendLine(
                "                if (builder.Length > 0)");
            builder.AppendLine(
                "                    builder.Append('\\n');");
            builder.AppendLine(
                "                builder.Append(statsStringPart);");
            builder.AppendLine("            }");
            builder.AppendLine(
                "            if (textComponent != null)");
            builder.AppendLine(
                "                textComponent.text = statsStringPart;");
            builder.AppendLine("        }");
        }

        #endregion

        #region UIBaseEquipmentBonus

        private static string GenerateUIBaseEquipmentBonusCode(
            CharacterStatFieldData[] fields)
        {
            StringBuilder builder = new StringBuilder(256 * 1024);

            AppendGeneratedHeader(builder);

            builder.AppendLine("using Cysharp.Text;");
            builder.AppendLine("using Insthync.DevExtension;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();

            builder.AppendLine("namespace MultiplayerARPG");
            builder.AppendLine("{");
            builder.AppendLine(
                "    public abstract partial class UIBaseEquipmentBonus<T> : UISelectionEntry<T>");
            builder.AppendLine("    {");

            AppendNormalFormatFields(builder, fields);
            AppendRateFormatFields(builder, fields);
            AppendEquipmentOtherFormatFields(builder);
            AppendEquipmentUIFields(builder);
            AppendEquipmentOnDestroy(builder);
            AppendEquipmentGetBonusText(builder, fields);

            builder.AppendLine("    }");
            builder.AppendLine("}");

            return builder.ToString();
        }

        private static void AppendEquipmentOtherFormatFields(
            StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        [Header(\"String Formats (Attribute/Damage Element/Skill)\")]");

            builder.AppendLine(
                "        [Tooltip(\"Format => {0} = {Attribute Title}, {1} = {Amount}\")]");
            builder.AppendLine(
                "        public UILocaleKeySetting formatKeyAttributeAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTRIBUTE_AMOUNT);");

            builder.AppendLine(
                "        [Tooltip(\"Format => {0} = {Attribute Title}, {1} = {Amount * 100}\")]");
            builder.AppendLine(
                "        public UILocaleKeySetting formatKeyAttributeAmountRate = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTRIBUTE_RATE);");

            builder.AppendLine(
                "        [Tooltip(\"Format => {0} = {Damage Element Title}, {1} = {Amount * 100}\")]");
            builder.AppendLine(
                "        public UILocaleKeySetting formatKeyResistanceAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_RESISTANCE_AMOUNT);");

            builder.AppendLine(
                "        [Tooltip(\"Format => {0} = {Damage Element Title}, {1} = {Min Damage}, {2} = {Max Damage}\")]");
            builder.AppendLine(
                "        public UILocaleKeySetting formatKeyDamageAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_WITH_ELEMENTAL);");

            builder.AppendLine(
                "        [Tooltip(\"Format => {0} = {Damage Element Title}, {1} = {Min Damage * 100}, {2} = {Max Damage * 100}\")]");
            builder.AppendLine(
                "        public UILocaleKeySetting formatKeyDamageAmountRate = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_WITH_ELEMENTAL_RATE);");

            builder.AppendLine(
                "        [Tooltip(\"Format => {0} = {Damage Element Title}, {1} = {Target Amount}\")]");
            builder.AppendLine(
                "        public UILocaleKeySetting formatKeyArmorAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ARMOR_AMOUNT);");

            builder.AppendLine(
                "        [Tooltip(\"Format => {0} = {Damage Element Title}, {1} = {Target Amount * 100}\")]");
            builder.AppendLine(
                "        public UILocaleKeySetting formatKeyArmorAmountRate = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ARMOR_RATE);");

            builder.AppendLine(
                "        [Tooltip(\"Format => {0} = {Skill Title}, {1} = {Level}\")]");
            builder.AppendLine(
                "        public UILocaleKeySetting formatKeySkillLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_LEVEL);");
        }

        private static void AppendEquipmentUIFields(
            StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        [Header(\"UI Elements\")]");
            builder.AppendLine(
                "        public TextWrapper uiTextAllBonus;");
            builder.AppendLine(
                "        public Color bonusIncreaseColor = Color.green;");
            builder.AppendLine(
                "        public Color bonusDecreaseColor = Color.red;");
        }

        private static void AppendEquipmentOnDestroy(
            StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        protected override void OnDestroy()");
            builder.AppendLine("        {");
            builder.AppendLine(
                "            base.OnDestroy();");
            builder.AppendLine(
                "            uiTextAllBonus = null;");
            builder.AppendLine("        }");
        }

        private static void AppendEquipmentGetBonusText(
            StringBuilder builder,
            CharacterStatFieldData[] fields)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        public string GetEquipmentBonusText(EquipmentBonus equipmentBonus)");
            builder.AppendLine("        {");
            builder.AppendLine(
                "            using (Utf16ValueStringBuilder result = ZString.CreateStringBuilder(false))");
            builder.AppendLine("            {");
            builder.AppendLine(
                "                CharacterStatsTextGenerateData generateTextData;");

            AppendEquipmentDevExtensionComment(builder);
            AppendEquipmentStatsInitializer(
                builder,
                fields,
                isRate: true);
            AppendEquipmentStatsInitializer(
                builder,
                fields,
                isRate: false);

            builder.AppendLine();
            builder.AppendLine(
                "                if (!string.IsNullOrEmpty(statsText))");
            builder.AppendLine(
                "                    result.Append(statsText);");
            builder.AppendLine();
            builder.AppendLine(
                "                if (!string.IsNullOrEmpty(rateStatsText))");
            builder.AppendLine("                {");
            builder.AppendLine(
                "                    if (result.Length > 0)");
            builder.AppendLine(
                "                        result.Append('\\n');");
            builder.AppendLine(
                "                    result.Append(rateStatsText);");
            builder.AppendLine("                }");

            AppendEquipmentAttributeBlocks(builder);

            builder.AppendLine();
            builder.AppendLine(
                "                return result.ToString();");
            builder.AppendLine("            }");
            builder.AppendLine("        }");
        }

        private static void AppendEquipmentDevExtensionComment(
            StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine("                // Dev Extension");
            builder.AppendLine(
                "                // How to implement it?:");
            builder.AppendLine("                // /*");
            builder.AppendLine(
                "                //  * - Add `customStat1` to `CharacterStats` partial class file");
            builder.AppendLine(
                "                //  * - Add `customStat1StatsFormat` to `CharacterStatsTextGenerateData`");
            builder.AppendLine(
                "                //  * - Add `uiTextCustomStat1` to `CharacterStatsTextGenerateData`");
            builder.AppendLine(
                "                //  * - Add `formatKeyCustomStat1Stats` to `UIBaseEquipmentBonus` partial class file");
            builder.AppendLine(
                "                //  * - Add `formatKeyCustomStat1RateStats` to `UIBaseEquipmentBonus` partial class file");
            builder.AppendLine(
                "                //  * - Add `uiTextCustomStat1` to `UIBaseEquipmentBonus`");
            builder.AppendLine("                //  */");
            builder.AppendLine(
                "                // [DevExtMethods(\"SetStatsGenerateTextData\")]");
            builder.AppendLine(
                "                // public void SetStatsGenerateTextData_Ext(CharacterStatsTextGenerateData generateTextData)");
            builder.AppendLine("                // {");
            builder.AppendLine(
                "                //   generateTextData.customStat1StatsFormat = formatKeyCustomStat1Stats;");
            builder.AppendLine(
                "                //   generateTextData.uiTextCustomStat1 = uiTextCustomStat1;");
            builder.AppendLine("                // }");
            builder.AppendLine(
                "                // [DevExtMethods(\"SetRateStatsGenerateTextData\")]");
            builder.AppendLine(
                "                // public void SetRateStatsGenerateTextData_Ext(CharacterStatsTextGenerateData generateTextData)");
            builder.AppendLine("                // {");
            builder.AppendLine(
                "                //   generateTextData.customStat1StatsFormat = formatKeyCustomStat1RateStats;");
            builder.AppendLine(
                "                //   generateTextData.uiTextCustomStat1 = uiTextCustomStat1;");
            builder.AppendLine("                // }");
        }

        private static void AppendEquipmentStatsInitializer(
            StringBuilder builder,
            CharacterStatFieldData[] fields,
            bool isRate)
        {
            builder.AppendLine();

            builder.AppendLine(
                isRate
                    ? "                // Rate stats"
                    : "                // Non-rate stats");

            builder.AppendLine(
                "                generateTextData = new CharacterStatsTextGenerateData()");
            builder.AppendLine("                {");

            builder.AppendLine(
                isRate
                    ? "                    data = equipmentBonus.StatsRate,"
                    : "                    data = equipmentBonus.Stats,");

            builder
                .Append("                    isRate = ")
                .Append(isRate ? "true" : "false")
                .AppendLine(",");

            builder.AppendLine(
                "                    isBonus = true,");

            foreach (CharacterStatFieldData field in fields)
            {
                builder
                    .Append("                    ")
                    .Append(field.StatsFormatFieldName)
                    .Append(" = ")
                    .Append(
                        isRate
                            ? field.RateFormatFieldName
                            : field.NormalFormatFieldName)
                    .AppendLine(",");
            }

            builder.AppendLine("                };");

            builder.AppendLine(
                isRate
                    ? "                this.InvokeInstanceDevExtMethods(\"SetRateStatsGenerateTextData\", generateTextData);"
                    : "                this.InvokeInstanceDevExtMethods(\"SetStatsGenerateTextData\", generateTextData);");

            builder.AppendLine(
                isRate
                    ? "                string rateStatsText = generateTextData.GetText(bonusIncreaseColor, bonusDecreaseColor);"
                    : "                string statsText = generateTextData.GetText(bonusIncreaseColor, bonusDecreaseColor);");
        }

        private static void AppendEquipmentAttributeBlocks(
            StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine("                // Attributes");
            builder.AppendLine(
                "                foreach (KeyValuePair<Attribute, float> entry in equipmentBonus.Attributes)");
            builder.AppendLine("                {");
            builder.AppendLine(
                "                    if (entry.Key == null || entry.Value == 0)");
            builder.AppendLine(
                "                        continue;");
            builder.AppendLine(
                "                    if (result.Length > 0)");
            builder.AppendLine(
                "                        result.Append('\\n');");
            builder.AppendLine(
                "                    result.AppendFormat(");
            builder.AppendLine(
                "                        LanguageManager.GetText(formatKeyAttributeAmount),");
            builder.AppendLine(
                "                        entry.Key.Title,");
            builder.AppendLine(
                "                        entry.Value.ToBonusString(\"N0\"));");
            builder.AppendLine("                }");

            builder.AppendLine();
            builder.AppendLine(
                "                foreach (KeyValuePair<Attribute, float> entry in equipmentBonus.AttributesRate)");
            builder.AppendLine("                {");
            builder.AppendLine(
                "                    if (entry.Key == null || entry.Value == 0)");
            builder.AppendLine(
                "                        continue;");
            builder.AppendLine(
                "                    if (result.Length > 0)");
            builder.AppendLine(
                "                        result.Append('\\n');");
            builder.AppendLine(
                "                    result.AppendFormat(");
            builder.AppendLine(
                "                        LanguageManager.GetText(formatKeyAttributeAmountRate),");
            builder.AppendLine(
                "                        entry.Key.Title,");
            builder.AppendLine(
                "                        (entry.Value * 100).ToBonusString(\"N2\"));");
            builder.AppendLine("                }");

            builder.AppendLine();
            builder.AppendLine("                // Resistances");
            builder.AppendLine(
                "                foreach (KeyValuePair<DamageElement, float> entry in equipmentBonus.Resistances)");
            builder.AppendLine("                {");
            builder.AppendLine(
                "                    if (entry.Key == null || entry.Value == 0)");
            builder.AppendLine(
                "                        continue;");
            builder.AppendLine(
                "                    if (result.Length > 0)");
            builder.AppendLine(
                "                        result.Append('\\n');");
            builder.AppendLine(
                "                    result.AppendFormat(");
            builder.AppendLine(
                "                        LanguageManager.GetText(formatKeyResistanceAmount),");
            builder.AppendLine(
                "                        entry.Key.Title,");
            builder.AppendLine(
                "                        (entry.Value * 100).ToBonusString(\"N2\"));");
            builder.AppendLine("                }");

            builder.AppendLine();
            builder.AppendLine("                // Damages");
            builder.AppendLine(
                "                foreach (KeyValuePair<DamageElement, MinMaxFloat> entry in equipmentBonus.Damages)");
            builder.AppendLine("                {");
            builder.AppendLine(
                "                    if (entry.Key == null || (entry.Value.min == 0 && entry.Value.max == 0))");
            builder.AppendLine(
                "                        continue;");
            builder.AppendLine(
                "                    if (result.Length > 0)");
            builder.AppendLine(
                "                        result.Append('\\n');");
            builder.AppendLine(
                "                    result.AppendFormat(");
            builder.AppendLine(
                "                        LanguageManager.GetText(formatKeyDamageAmount),");
            builder.AppendLine(
                "                        entry.Key.Title,");
            builder.AppendLine(
                "                        entry.Value.min.ToBonusString(\"N0\"),");
            builder.AppendLine(
                "                        entry.Value.max.ToString(\"N0\"));");
            builder.AppendLine("                }");

            builder.AppendLine();
            builder.AppendLine(
                "                foreach (KeyValuePair<DamageElement, MinMaxFloat> entry in equipmentBonus.DamagesRate)");
            builder.AppendLine("                {");
            builder.AppendLine(
                "                    if (entry.Key == null || (entry.Value.min == 0 && entry.Value.max == 0))");
            builder.AppendLine(
                "                        continue;");
            builder.AppendLine(
                "                    if (result.Length > 0)");
            builder.AppendLine(
                "                        result.Append('\\n');");
            builder.AppendLine(
                "                    result.AppendFormat(");
            builder.AppendLine(
                "                        LanguageManager.GetText(formatKeyDamageAmountRate),");
            builder.AppendLine(
                "                        entry.Key.Title,");
            builder.AppendLine(
                "                        (entry.Value.min * 100).ToBonusString(\"N2\"),");
            builder.AppendLine(
                "                        (entry.Value.max * 100).ToString(\"N2\"));");
            builder.AppendLine("                }");

            builder.AppendLine();
            builder.AppendLine("                // Armors");
            builder.AppendLine(
                "                foreach (KeyValuePair<DamageElement, float> entry in equipmentBonus.Armors)");
            builder.AppendLine("                {");
            builder.AppendLine(
                "                    if (entry.Value == 0)");
            builder.AppendLine(
                "                        continue;");
            builder.AppendLine(
                "                    if (result.Length > 0)");
            builder.AppendLine(
                "                        result.Append('\\n');");
            builder.AppendLine(
                "                    result.AppendFormat(");
            builder.AppendLine(
                "                        LanguageManager.GetText(formatKeyArmorAmount),");
            builder.AppendLine(
                "                        entry.Key.Title,");
            builder.AppendLine(
                "                        entry.Value.ToBonusString(\"N0\"));");
            builder.AppendLine("                }");

            builder.AppendLine();
            builder.AppendLine(
                "                foreach (KeyValuePair<DamageElement, float> entry in equipmentBonus.ArmorsRate)");
            builder.AppendLine("                {");
            builder.AppendLine(
                "                    if (entry.Value == 0)");
            builder.AppendLine(
                "                        continue;");
            builder.AppendLine(
                "                    if (result.Length > 0)");
            builder.AppendLine(
                "                        result.Append('\\n');");
            builder.AppendLine(
                "                    result.AppendFormat(");
            builder.AppendLine(
                "                        LanguageManager.GetText(formatKeyArmorAmountRate),");
            builder.AppendLine(
                "                        entry.Key.Title,");
            builder.AppendLine(
                "                        (entry.Value * 100).ToBonusString(\"N2\"));");
            builder.AppendLine("                }");

            builder.AppendLine();
            builder.AppendLine("                // Skills");
            builder.AppendLine(
                "                foreach (KeyValuePair<BaseSkill, int> entry in equipmentBonus.Skills)");
            builder.AppendLine("                {");
            builder.AppendLine(
                "                    if (entry.Key == null || entry.Value == 0)");
            builder.AppendLine(
                "                        continue;");
            builder.AppendLine(
                "                    if (result.Length > 0)");
            builder.AppendLine(
                "                        result.Append('\\n');");
            builder.AppendLine(
                "                    result.AppendFormat(");
            builder.AppendLine(
                "                        LanguageManager.GetText(formatKeySkillLevel),");
            builder.AppendLine(
                "                        entry.Key.Title,");
            builder.AppendLine(
                "                        entry.Value.ToBonusString(\"N0\"));");
            builder.AppendLine("                }");
        }

        #endregion

        #region UICharacterStats

        private static string GenerateUICharacterStatsCode(
            CharacterStatFieldData[] fields)
        {
            StringBuilder builder = new StringBuilder(256 * 1024);

            AppendGeneratedHeader(builder);

            builder.AppendLine("using Insthync.DevExtension;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine("using UnityEngine.Serialization;");
            builder.AppendLine();
            builder.AppendLine("#if UNITY_EDITOR");
            builder.AppendLine("using UnityEditor;");
            builder.AppendLine("#endif");
            builder.AppendLine();

            builder.AppendLine("namespace MultiplayerARPG");
            builder.AppendLine("{");
            builder.AppendLine(
                "    public partial class UICharacterStats : UISelectionEntry<CharacterStats>");
            builder.AppendLine("    {");

            AppendUICharacterStatsDisplayType(builder);
            AppendNormalFormatFields(builder, fields);
            AppendRateFormatFields(builder, fields);
            AppendUICharacterStatsUIFields(builder, fields);
            AppendUICharacterStatsOptions(builder);
            AppendUICharacterStatsOnDestroy(builder, fields);
            AppendUICharacterStatsUpdateData(builder, fields);
            AppendUICharacterStatsSetAllSimple(builder, fields);

            builder.AppendLine("    }");
            builder.AppendLine("}");

            return builder.ToString();
        }

        private static void AppendUICharacterStatsDisplayType(
            StringBuilder builder)
        {
            builder.AppendLine(
                "        public enum DisplayType");
            builder.AppendLine("        {");
            builder.AppendLine("            Simple,");
            builder.AppendLine("            Rate");
            builder.AppendLine("        }");
        }

        private static void AppendNormalFormatFields(
            StringBuilder builder,
            CharacterStatFieldData[] fields)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        [Header(\"String Formats (Stats)\")]");

            foreach (CharacterStatFieldData field in fields)
            {
                builder
                    .Append("        [Tooltip(\"Format => {0} = {Amount")
                    .Append(field.IsRate ? " * 100" : string.Empty)
                    .AppendLine("}\")]");

                builder
                    .Append("        public UILocaleKeySetting ")
                    .Append(field.NormalFormatFieldName)
                    .Append(" = new UILocaleKeySetting(UIFormatKeys.")
                    .Append(field.NormalUIFormatKey)
                    .AppendLine(");");
            }
        }

        private static void AppendRateFormatFields(
            StringBuilder builder,
            CharacterStatFieldData[] fields)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        [Header(\"String Formats (Stats Rate)\")]");

            foreach (CharacterStatFieldData field in fields)
            {
                builder.AppendLine(
                    "        [Tooltip(\"Format => {0} = {Amount * 100}\")]");

                builder
                    .Append("        public UILocaleKeySetting ")
                    .Append(field.RateFormatFieldName)
                    .Append(" = new UILocaleKeySetting(UIFormatKeys.")
                    .Append(field.RateUIFormatKey)
                    .AppendLine(");");
            }
        }

        private static void AppendUICharacterStatsUIFields(
            StringBuilder builder,
            CharacterStatFieldData[] fields)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        [Header(\"UI Elements\")]");
            builder.AppendLine(
                "        public TextWrapper uiTextStats;");

            foreach (CharacterStatFieldData field in fields)
            {
                AppendFormerlySerializedAs(
                    builder,
                    field.StatName);

                builder
                    .Append("        public TextWrapper ")
                    .Append(field.TextWrapperFieldName)
                    .AppendLine(";");
            }

            builder.AppendLine(
                "        public DisplayType displayType;");
            builder.AppendLine(
                "        public bool isBonus;");
            builder.AppendLine(
                "        public Color bonusIncreaseColor = Color.green;");
            builder.AppendLine(
                "        public Color bonusDecreaseColor = Color.red;");
        }

        private static void AppendFormerlySerializedAs(
            StringBuilder builder,
            string statName)
        {
            switch (statName)
            {
                case nameof(CharacterStats.ammoCapacityModifier):
                    builder.AppendLine(
                        "        [FormerlySerializedAs(\"uiTextAmmoCapacity\")]");
                    break;

                case nameof(CharacterStats.rateOfFireModifier):
                    builder.AppendLine(
                        "        [FormerlySerializedAs(\"uiTextRateOfFire\")]");
                    break;

                case nameof(CharacterStats.reloadDurationModifier):
                    builder.AppendLine(
                        "        [FormerlySerializedAs(\"uiTextReloadDuration\")]");
                    break;

                case nameof(CharacterStats.fireSpreadRangeModifier):
                    builder.AppendLine(
                        "        [FormerlySerializedAs(\"uiTextFireSpreadRange\")]");
                    break;

                case nameof(CharacterStats.fireSpreadModifier):
                    builder.AppendLine(
                        "        [FormerlySerializedAs(\"uiTextFireSpread\")]");
                    break;
            }
        }

        private static void AppendUICharacterStatsOptions(
            StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        [Header(\"Options\")]");
            builder.AppendLine(
                "        public string numberFormatSimple = \"N0\";");
            builder.AppendLine(
                "        public string numberFormatRate = \"N2\";");
        }

        private static void AppendUICharacterStatsOnDestroy(
            StringBuilder builder,
            CharacterStatFieldData[] fields)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        protected override void OnDestroy()");
            builder.AppendLine("        {");
            builder.AppendLine(
                "            base.OnDestroy();");
            builder.AppendLine(
                "            uiTextStats = null;");

            foreach (CharacterStatFieldData field in fields)
            {
                builder
                    .Append("            ")
                    .Append(field.TextWrapperFieldName)
                    .AppendLine(" = null;");
            }

            builder.AppendLine("        }");
        }

        private static void AppendUICharacterStatsUpdateData(
            StringBuilder builder,
            CharacterStatFieldData[] fields)
        {
            builder.AppendLine();
            builder.AppendLine(
                "        protected override void UpdateData()");
            builder.AppendLine("        {");
            builder.AppendLine(
                "            CharacterStatsTextGenerateData generateTextData;");
            builder.AppendLine(
                "            string statsString;");

            AppendUICharacterStatsDevExtensionComment(builder);

            builder.AppendLine();
            builder.AppendLine(
                "            switch (displayType)");
            builder.AppendLine("            {");

            builder.AppendLine(
                "                case DisplayType.Rate:");

            AppendUICharacterStatsInitializer(
                builder,
                fields,
                isRate: true);

            builder.AppendLine(
                "                    this.InvokeInstanceDevExtMethods(\"SetRateStatsGenerateTextData\", generateTextData);");
            builder.AppendLine(
                "                    statsString = generateTextData.GetText(bonusIncreaseColor, bonusDecreaseColor);");
            builder.AppendLine(
                "                    break;");

            builder.AppendLine(
                "                default:");

            AppendUICharacterStatsInitializer(
                builder,
                fields,
                isRate: false);

            builder.AppendLine(
                "                    this.InvokeInstanceDevExtMethods(\"SetStatsGenerateTextData\", generateTextData);");
            builder.AppendLine(
                "                    statsString = generateTextData.GetText(bonusIncreaseColor, bonusDecreaseColor);");
            builder.AppendLine(
                "                    break;");

            builder.AppendLine("            }");

            builder.AppendLine();
            builder.AppendLine(
                "            // All stats text");
            builder.AppendLine(
                "            if (uiTextStats != null)");
            builder.AppendLine("            {");
            builder.AppendLine(
                "                uiTextStats.SetGameObjectActive(!string.IsNullOrEmpty(statsString));");
            builder.AppendLine(
                "                uiTextStats.text = statsString;");
            builder.AppendLine("            }");
            builder.AppendLine("        }");
        }

        private static void AppendUICharacterStatsInitializer(
            StringBuilder builder,
            CharacterStatFieldData[] fields,
            bool isRate)
        {
            builder.AppendLine(
                "                    generateTextData = new CharacterStatsTextGenerateData()");
            builder.AppendLine("                    {");
            builder.AppendLine(
                "                        data = Data,");

            builder
                .Append("                        isRate = ")
                .Append(isRate ? "true" : "false")
                .AppendLine(",");

            builder.AppendLine(
                "                        isBonus = isBonus,");
            builder.AppendLine(
                "                        numberFormatSimple = numberFormatSimple,");
            builder.AppendLine(
                "                        numberFormatRate = numberFormatRate,");

            foreach (CharacterStatFieldData field in fields)
            {
                builder
                    .Append("                        ")
                    .Append(field.StatsFormatFieldName)
                    .Append(" = ")
                    .Append(
                        isRate
                            ? field.RateFormatFieldName
                            : field.NormalFormatFieldName)
                    .AppendLine(",");
            }

            foreach (CharacterStatFieldData field in fields)
            {
                builder
                    .Append("                        ")
                    .Append(field.TextWrapperFieldName)
                    .Append(" = ")
                    .Append(field.TextWrapperFieldName)
                    .AppendLine(",");
            }

            builder.AppendLine("                    };");
        }

        private static void AppendUICharacterStatsDevExtensionComment(
            StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine("            // Dev Extension");
            builder.AppendLine("            // How to implement it?:");
            builder.AppendLine("            // /*");
            builder.AppendLine(
                "            //  * - Add `customStat1` to `CharacterStats` partial class file");
            builder.AppendLine(
                "            //  * - Add `customStat1StatsFormat` to `CharacterStatsTextGenerateData`");
            builder.AppendLine(
                "            //  * - Add `uiTextCustomStat1` to `CharacterStatsTextGenerateData`");
            builder.AppendLine(
                "            //  * - Add `formatKeyCustomStat1Stats` to `UICharacterStats` partial class file");
            builder.AppendLine(
                "            //  * - Add `formatKeyCustomStat1RateStats` to `UICharacterStats` partial class file");
            builder.AppendLine(
                "            //  * - Add `uiTextCustomStat1` to `UICharacterStats`");
            builder.AppendLine("            //  */");
            builder.AppendLine(
                "            // [DevExtMethods(\"SetStatsGenerateTextData\")]");
            builder.AppendLine(
                "            // public void SetStatsGenerateTextData_Ext(CharacterStatsTextGenerateData generateTextData)");
            builder.AppendLine("            // {");
            builder.AppendLine(
                "            //   generateTextData.customStat1StatsFormat = formatKeyCustomStat1Stats;");
            builder.AppendLine(
                "            //   generateTextData.uiTextCustomStat1 = uiTextCustomStat1;");
            builder.AppendLine("            // }");
            builder.AppendLine(
                "            // [DevExtMethods(\"SetRateStatsGenerateTextData\")]");
            builder.AppendLine(
                "            // public void SetRateStatsGenerateTextData_Ext(CharacterStatsTextGenerateData generateTextData)");
            builder.AppendLine("            // {");
            builder.AppendLine(
                "            //   generateTextData.customStat1StatsFormat = formatKeyCustomStat1RateStats;");
            builder.AppendLine(
                "            //   generateTextData.uiTextCustomStat1 = uiTextCustomStat1;");
            builder.AppendLine("            // }");
        }

        private static void AppendUICharacterStatsSetAllSimple(
            StringBuilder builder,
            CharacterStatFieldData[] fields)
        {
            builder.AppendLine();
            builder.AppendLine("#if UNITY_EDITOR");
            builder.AppendLine(
                "        [ContextMenu(\"Set All Formats To Be Simple\")]");
            builder.AppendLine(
                "        public void SetAllFormatsToBeSimple()");
            builder.AppendLine("        {");

            foreach (CharacterStatFieldData field in fields)
            {
                builder
                    .Append("            ")
                    .Append(field.NormalFormatFieldName)
                    .Append(" = new UILocaleKeySetting(UIFormatKeys.")
                    .Append(
                        field.IsRate
                            ? "UI_FORMAT_SIMPLE_PERCENTAGE"
                            : "UI_FORMAT_SIMPLE")
                    .AppendLine(");");
            }

            builder.AppendLine();

            foreach (CharacterStatFieldData field in fields)
            {
                builder
                    .Append("            ")
                    .Append(field.RateFormatFieldName)
                    .AppendLine(
                        " = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE_PERCENTAGE);");
            }

            builder.AppendLine(
                "            EditorUtility.SetDirty(this);");
            builder.AppendLine("        }");
            builder.AppendLine("#endif");
        }

        #endregion

        #region Naming

        private static string GetCharacterStatsFormatFieldName(
            string statName)
        {
            return statName + "StatsFormat";
        }

        private static string GetNormalUIFormatKey(
            string statName)
        {
            return LocaleKeyPrefix +
                           ToUpperSnakeCase(statName);
        }

        private static string GetRateUIFormatKey(
            string statName)
        {
            return LocaleKeyPrefix +
                           ToUpperSnakeCase(statName) +
                           "_RATE";
        }

        private static string ToPascalCase(
            string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (char.IsUpper(value[0]))
                return value;

            return char.ToUpperInvariant(value[0]) +
                   value.Substring(1);
        }

        private static string ToUpperSnakeCase(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string result = value
                .Replace('-', '_')
                .Replace(' ', '_');

            result = Regex.Replace(
                result,
                @"(?<=[a-z0-9])(?=[A-Z])",
                "_");

            result = Regex.Replace(
                result,
                @"(?<=[A-Z])(?=[A-Z][a-z])",
                "_");

            result = Regex.Replace(
                result,
                @"_+",
                "_");

            return result.ToUpperInvariant();
        }

        private static string ToDisplayName(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string result = Regex.Replace(
                value,
                @"(?<=[a-z0-9])(?=[A-Z])",
                " ");

            result = Regex.Replace(
                result,
                @"(?<=[A-Z])(?=[A-Z][a-z])",
                " ");

            return char.ToUpperInvariant(result[0]) +
                   result.Substring(1);
        }

        private static string EscapeString(
            string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }

        private static void AppendGeneratedHeader(
            StringBuilder builder)
        {
            builder.AppendLine("// <auto-generated>");
            builder.AppendLine(
                "// Generated by CodeGen_CharacterStatsUIClasses.");
            builder.AppendLine(
                "// Changes to this file will be overwritten.");
            builder.AppendLine("// </auto-generated>");
            builder.AppendLine();
        }

        #endregion

        private sealed class CharacterStatFieldData
        {
            public FieldInfo Field { get; }

            public CharacterStatTextGenAttribute Attribute { get; }

            public string FieldName => Field.Name;

            public string StatName { get; }

            public string FormatKey { get; }

            public bool IsRate => Attribute.IsRate;

            public string PascalStatName =>
                ToPascalCase(StatName);

            public string DisplayName =>
                ToDisplayName(StatName);

            public string StatsFormatFieldName =>
                GetCharacterStatsFormatFieldName(StatName);

            public string TextWrapperFieldName =>
                "uiText" + PascalStatName;

            public string NormalFormatFieldName =>
                "formatKey" + PascalStatName + "Stats";

            public string RateFormatFieldName =>
                "formatKey" + PascalStatName + "RateStats";

            public string NormalUIFormatKey =>
                GetNormalUIFormatKey(FormatKey);

            public string RateUIFormatKey =>
                GetRateUIFormatKey(FormatKey);

            public CharacterStatFieldData(
                FieldInfo field,
                CharacterStatTextGenAttribute attribute)
            {
                Field = field;
                Attribute = attribute;

                StatName = field.Name;

                FormatKey =
                    string.IsNullOrWhiteSpace(
                        attribute.FormatKey)
                        ? field.Name
                        : attribute.FormatKey;
            }
        }
    }
}

#endif