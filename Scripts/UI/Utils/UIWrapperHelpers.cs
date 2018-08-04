using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWrapperHelpers
{
    public static TextWrapper SetWrapperToText(Text oldComp, TextWrapper newComp)
    {
        if (oldComp != null && newComp == null)
        {
            newComp = oldComp.gameObject.GetOrAddComponent<TextWrapper>();
            newComp.unityText = oldComp;
        }
        return newComp;
    }

    public static InputFieldWrapper SetWrapperToInputField(InputField oldComp, InputFieldWrapper newComp)
    {
        if (oldComp != null && newComp == null)
        {
            newComp = oldComp.gameObject.GetOrAddComponent<InputFieldWrapper>();
            newComp.unityInputField = oldComp;
        }
        return newComp;
    }
}
