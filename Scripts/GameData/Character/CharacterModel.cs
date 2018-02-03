using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterModel : MonoBehaviour
{
    [Header("Collider")]
    public Vector3 center;
    public float radius = 0.5f;
    public float height = 2f;
    [Header("Equipment Containers")]
    public Transform rightHandContainer;
    public Transform leftHandContainer;
    public CharacterModelContainer[] equipmentContainers;

    private Animator tempAnimator;
    public Animator TempAnimator
    {
        get
        {
            if (tempAnimator == null)
                tempAnimator = GetComponent<Animator>();
            return tempAnimator;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        var topCorner = (Vector3.up * height * 0.5f) - (Vector3.up * radius);
        var bottomCorner = -(Vector3.up * height * 0.5f) + (Vector3.up * radius);
        Gizmos.DrawWireSphere(center + topCorner, radius);
        Gizmos.DrawWireSphere(center + bottomCorner, radius);
        Gizmos.DrawLine(center + topCorner + Vector3.left * radius, center + bottomCorner + Vector3.left * radius);
        Gizmos.DrawLine(center + topCorner + Vector3.right * radius, center + bottomCorner + Vector3.right * radius);
        Gizmos.DrawLine(center + topCorner + Vector3.forward * radius, center + bottomCorner + Vector3.forward * radius);
        Gizmos.DrawLine(center + topCorner + Vector3.back * radius, center + bottomCorner + Vector3.back * radius);
    }
}

[System.Serializable]
public struct CharacterModelContainer
{
    public string equipPosition;
    public Transform container;
}
