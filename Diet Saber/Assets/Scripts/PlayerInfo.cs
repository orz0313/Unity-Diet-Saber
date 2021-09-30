using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{   
    Rigidbody rb;
    Animator ani;
    ThirdPersonController ThirdPersonController;
    // FacePoseEstimation FacePoseEstimation;
    int PlayerHealthPoint;
    int PlayerInitialHP = 100;
    Vector3 PlayerRespawnPoint;
    public int HP{get{return PlayerHealthPoint;}}
    void Awake()
    {
        PlayerRespawnPoint = transform.position;
        rb = GetComponent<Rigidbody>();
        ani = GetComponent<Animator>();
        ThirdPersonController = GetComponent<ThirdPersonController>();
        // FacePoseEstimation = GetComponent<FacePoseEstimation>();
        PlayerHealthPoint = PlayerInitialHP;
    }
    // void Update(){
    //     if(Input.GetKeyDown(KeyCode.Tab)){
    //         HitPlayer(PlayerRespawnPoint,60);
    //         rb.AddForce(new Vector3(400f,200f,400f),ForceMode.Acceleration);
    //     }       
    // }
    public void NewSpawnPpint(Vector3 V3){
        PlayerRespawnPoint = V3;
    }
    public Vector3 GetRespawnPoint(){
        return PlayerRespawnPoint;
    }
    public void PlayerWinCheck(){
        PlayerWin();
    }
    void PlayerWin(){
        ani.SetTrigger("Win");
        ani.SetBool("IsJumping",false);
        PlayerHealthPoint = PlayerInitialHP;
    }
    public void PlayerHPtakeaway(int i)
    {
        PlayerHealthPoint -= i;
        if(PlayerHealthPoint<=0)
        {
            PlayerRespawn();
        }
    }
    public void PlayerHPAdd(int i)
    {
        if(PlayerHealthPoint<100)
        {
        PlayerHealthPoint += i;
        }
        else return;
    }
    public void HitPlayer(Vector3 v3,int i){
        PlayerHealthPoint -= i;
        if(PlayerHealthPoint<=0)
        {
            PlayerRespawn();
        }
        else
        {
            PlayerOnHit(v3);
        }
    }
    void PlayerRespawn(){
        ani.SetTrigger("Death Trigger");
        ani.SetBool("Death",true);
        ani.SetBool("IsJumping",false);   
        PlayerHealthPoint = PlayerInitialHP;
    }
    void PlayerOnHit(Vector3 v3){
        ani.SetTrigger("OnHit");
        rb.AddForce(new Vector3((transform.position.x-v3.x)*300f,200f,(transform.position.z-v3.z)*300f)
        ,ForceMode.Acceleration);        
    }
}
