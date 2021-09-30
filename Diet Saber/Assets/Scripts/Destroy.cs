using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    [SerializeField]PlayerInfo playerInfo;
    private void OnTriggerEnter(Collider other)
    {
        playerInfo.PlayerHPtakeaway(1);
        Destroy(other.gameObject);
    }
}
