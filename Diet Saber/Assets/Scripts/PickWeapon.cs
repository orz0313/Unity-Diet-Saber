using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickWeapon : MonoBehaviour
{
    private ParticleSystem Shinny;
    
    void Awake() 
    {
        Shinny = GetComponent<ParticleSystem>();
    }
    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("ItemPick"))
        {
            var emission = Shinny.emission;
            emission.enabled = false;
            GameObject Clone = Instantiate(this.gameObject,other.gameObject.transform);
            Clone.transform.localPosition = Vector3.zero;
            Clone.transform.localRotation = Quaternion.identity;
            Clone.transform.localScale = new Vector3(8,2,8);
            Destroy(this.gameObject);
        }
    }
}
