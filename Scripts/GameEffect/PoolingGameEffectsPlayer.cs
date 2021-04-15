using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PoolingGameEffectsPlayer : MonoBehaviour, IPoolDescriptorCollection
    {
        public GameEffectPoolContainer[] poolingGameEffects;

        public IEnumerable<IPoolDescriptor> PoolDescriptors
        {
            get
            {
                List<IPoolDescriptor> effects = new List<IPoolDescriptor>();
                if (poolingGameEffects != null && poolingGameEffects.Length > 0)
                {
                    foreach (GameEffectPoolContainer container in poolingGameEffects)
                    {
                        effects.Add(container.prefab);
                    }
                }
                return effects;
            }
        }

        public void PlayRandomEffect()
        {
            if (poolingGameEffects != null && poolingGameEffects.Length > 0)
                poolingGameEffects[Random.Range(0, poolingGameEffects.Length)].GetInstance();
        }
    }
}
