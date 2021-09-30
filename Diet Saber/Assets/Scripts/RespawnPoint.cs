using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    PlayerInfo PlayerInfo;

    void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")){
            PlayerInfo = other.GetComponent<PlayerInfo>();
            PlayerInfo.NewSpawnPpint(this.gameObject.transform.position);
            Destroy(this.gameObject);
        }
    }
}
