using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform player;
    private void Awake() {
        PhotonNetwork.Instantiate(playerPrefab.name, player.position, player.rotation);
    }
}
