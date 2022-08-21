using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public enum CollectibleType
{
    Candy,
    Candle
}

public class Collectible : MonoBehaviour
{
    public static Dictionary<CollectibleType, int> CollectibleCounts;

    public CollectibleType collectibleType;

    void Awake()
    {
        if(CollectibleCounts == null)
            CollectibleCounts = new Dictionary<CollectibleType, int>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == StarterAssetsInputs.currentPlayerObject)
        {
            IncrementCollectibleCount(collectibleType);
            Destroy(gameObject);
        }
    }

    public static void IncrementCollectibleCount(CollectibleType type)
    {
        if(!CollectibleCounts.ContainsKey(type))
            CollectibleCounts.Add(type, 0);

        CollectibleCounts[type]++;

        UIManager.instance.IncrementCollectibleCount(type, CollectibleCounts[type]);
    }
}
