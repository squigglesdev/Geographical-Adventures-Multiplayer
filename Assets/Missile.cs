using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("References")]
    public int initialVelocity;
    public int fuelVolume;

    //private stuff
    Transform target;

    void Awake()
    {
        GameObject parent = GameObject.Find("Game");
        gameObject.transform.parent = parent.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (fuelVolume > 0)
        {
            target = GameObject.FindWithTag("target").transform;
            var step = initialVelocity * Time.deltaTime;
            transform.LookAt(target);
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);
            fuelVolume --;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "target")
            {
                Debug.Log("hit");
                Destroy(collision.gameObject);
                Destroy(this.gameObject);
            }
    }
}
