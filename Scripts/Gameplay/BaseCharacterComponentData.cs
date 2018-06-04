using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacterComponentData : MonoBehaviour
{
    private BaseCharacterEntity cacheCharacterEntity;
    public BaseCharacterEntity CacheCharacterEntity
    {
        get
        {
            if (cacheCharacterEntity == null)
                cacheCharacterEntity = GetComponent<BaseCharacterEntity>();
            return cacheCharacterEntity;
        }
    }
}
