using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MissileSystem : MonoBehaviour
{
    [Header("References")]
    public int initialMissileCount;
    public GameObject missilePrefab;
    public Transform missileShootPoint;
    int missileCount;

    void Start()
    {
        missileCount = initialMissileCount;
    }

    // Update is called once per frame
    public void ShootMissile()
    {
        if (missileCount > 0)
        {
            PhotonNetwork.Instantiate(missilePrefab.name, missileShootPoint.position, missileShootPoint.rotation);
            missileCount --;
        }
    }
}
