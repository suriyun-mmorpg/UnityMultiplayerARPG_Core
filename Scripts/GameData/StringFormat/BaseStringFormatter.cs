using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseStringFormatter : ScriptableObject
    {
        public abstract string PreprocessFormat(string format);

        public string Format<T1>(UILocaleKeySetting formatKey, T1 arg1)
        {
            return Format(LanguageManager.GetText(formatKey), arg1);
        }

        public string Format<T1>(string format, T1 arg1)
        {
            return ZString.Format(PreprocessFormat(format), arg1);
        }

        public string Format<T1, T2>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2);
        }

        public string Format<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2);
        }

        public string Format<T1, T2, T3>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3);
        }

        public string Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3);
        }

        public string Format<T1, T2, T3, T4>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4);
        }

        public string Format<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4);
        }

        public string Format<T1, T2, T3, T4, T5>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5);
        }

        public string Format<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5);
        }

        public string Format<T1, T2, T3, T4, T5, T6>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public string Format<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
        {
            return Format(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }

        public string Format<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
        {
            return ZString.Format(PreprocessFormat(format), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }

        public string FormatIndexesOnly<T1>(UILocaleKeySetting formatKey, T1 arg1)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1);
        }

        public string FormatIndexesOnly<T1>(string format, T1 arg1)
        {
            return ZString.Format(format, arg1);
        }

        public string FormatIndexesOnly<T1, T2>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2);
        }

        public string FormatIndexesOnly<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            return ZString.Format(format, arg1, arg2);
        }

        public string FormatIndexesOnly<T1, T2, T3>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3);
        }

        public string FormatIndexesOnly<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            return ZString.Format(format, arg1, arg2, arg3);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(UILocaleKeySetting formatKey, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
        {
            return FormatIndexesOnly(LanguageManager.GetText(formatKey), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }

        public string FormatIndexesOnly<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
        {
            return ZString.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }
    }
}