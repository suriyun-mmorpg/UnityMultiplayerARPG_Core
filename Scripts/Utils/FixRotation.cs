using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixRotation : MonoBehaviour
{
    public Vector3 eulerAngles;
    private void LateUpdate()
    {
        transform.eulerAngles = eulerAngles;
    }
}
