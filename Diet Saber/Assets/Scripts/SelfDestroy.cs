using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    [SerializeField]int livingtime = 2;
    void Start()
    {
        StartCoroutine(SelfDelete(livingtime));
    }
    IEnumerator SelfDelete(int i)
    {
        yield return new WaitForSeconds(i);
        Destroy(this.gameObject);
    }
}
