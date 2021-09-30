using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnKickCubes : MonoBehaviour
{
    public GameObject RedCube;
    public GameObject BlueCube;
    float Currenttime;
    [SerializeField][Range(0.1f,2f)]float SpawnTime;
    [SerializeField][Range(0.1f,5f)]float SpawnCubeSpeed;
    [SerializeField]bool IsActive=false;

    void Update()
    {
        if(IsActive)
        {
            if(Time.time - Currenttime > SpawnTime)
            {
                Currenttime = Time.time;
                if(Random.Range(0,2)>0)
                {
                    SpawnBlueCube();
                }
                else 
                {
                    SpawnRedCube();
                }
            }
        }
    }
    void SpawnBlueCube()
    {
        GameObject clone = Instantiate(BlueCube);
        clone.transform.SetParent(this.transform);
        clone.transform.localPosition = new Vector3(Random.Range(-0.5f,0.5f),0f,0f);
        clone.transform.rotation = Quaternion.Euler(0f,180f,Random.Range(-1,2)*45);
        clone.GetComponent<Move>().AssignCubeSpeed(SpawnCubeSpeed);
    }
    void SpawnRedCube()
    {
        GameObject clone = Instantiate(RedCube);
        clone.transform.SetParent(this.transform);
        clone.transform.localPosition = new Vector3(Random.Range(-0.5f,0.5f),0f,0f);
        clone.transform.rotation = Quaternion.Euler(0f,180f,Random.Range(-1,2)*45);
        clone.GetComponent<Move>().AssignCubeSpeed(SpawnCubeSpeed);
    }    
    public void SetIsActive(bool b ) 
    {
        IsActive = b;
    }
}
