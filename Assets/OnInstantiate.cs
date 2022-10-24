using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnInstantiate : MonoBehaviour
{
    
    void Awake()
    {
        GameObject parent = GameObject.Find("Game");
        gameObject.transform.parent = parent.transform;
    }
}
