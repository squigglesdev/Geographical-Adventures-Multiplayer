using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform mainCameraTransform;
    public Transform enemyTransforms;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    void Update()
    {
        enemyTransforms = GameObject.FindWithTag("Plane").transform;
        transform.position = enemyTransforms.position;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward, mainCameraTransform.rotation * Vector3.up);
    }
}
