using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectLayerManager : MonoBehaviour
{
    public int order;
    public string layerName;
    void Start()
    {
        var renderer = gameObject.GetComponent<Renderer>();
        renderer.sortingLayerName = layerName;
        renderer.sortingOrder = order;
    }

    void Update()
    {
        
    }
}
