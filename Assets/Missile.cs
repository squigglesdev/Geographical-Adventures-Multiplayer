using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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
            target = GameObject.FindWithTag("Plane").transform;
            var step = initialVelocity * Time.deltaTime;
            transform.LookAt(target);
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);
            fuelVolume --;
        }
        else
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "target")
            {
                Debug.Log("hit");
                PhotonNetwork.Destroy(other.gameObject);
                PhotonNetwork.Destroy(this.gameObject);
            }
    }
}
