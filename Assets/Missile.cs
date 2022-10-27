using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Missile : MonoBehaviourPun
{
    [Header("Missile Settings")]
    public int initialVelocity;
    public int fuelVolume;
    public int damage;

    [Header("References")]
    public AudioSource audioSource;

    //private stuff
    Transform target;

    void Awake()
    {
        audioSource = GameObject.FindWithTag("target").GetComponent<AudioSource>();
        GameObject parent = GameObject.Find("Game");
        gameObject.transform.parent = parent.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(audioSource.pitch < 1.5f)
        {
            audioSource.pitch += 0.05f;
        }
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
            audioSource.pitch = 1f;
            Destroy(gameObject);
        }
    }
    void OnCollisionEnter(Collision other) {
        MissileTarget missileTarget = other.gameObject.GetComponent<MissileTarget>();
        // Only attempts to inflict damage if the other game object has
        // the 'Target' component
        if(missileTarget != null) {
            missileTarget.Hit(damage);
            audioSource.pitch = 1f;
            Destroy(gameObject); // Deletes the round
        }
    }
}
