using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    float Speed = 1;
    void Update()
    {        
        transform.position += new Vector3(0,0,-Speed) * Time.deltaTime;
    }

    public void AssignCubeSpeed(float f)
    {
        Speed = f;
    }
}
