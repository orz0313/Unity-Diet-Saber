using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class ThirdPersonController : MonoBehaviour {
    [SerializeField]Transform cam;//maincamera to be reference & cinemachine free look camera
    [SerializeField]CinemachineFreeLook FreeLookCamera;
    Animator ani;
    Rigidbody rb;
    float velocity = 2f;
    Vector3 JumpHeight = new Vector3(0f,4f,0f);
    Vector3 inputdirection;
    Vector3 movedirction;
    bool ontheGround;
    bool running = false;
    bool jumping = false;
    float LastJumpingTime = 0f;
    float inputangle;
    float targetangle;
    float horizontalinput;
    float verticalinput;
    void Awake() {
        rb = GetComponent<Rigidbody>();
        ani = GetComponent<Animator>();
    }

    void Update() {
        if(Input.GetKey(KeyCode.Q))
        {
            FreeLookCamera.m_XAxis.Value -= 120f*Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.E))
        {
            FreeLookCamera.m_XAxis.Value += 120f*Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.T))
        {
            FreeLookCamera.m_YAxis.Value += 0.2f*Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.G))
        {
            FreeLookCamera.m_YAxis.Value -= 0.2f*Time.deltaTime;
        }
        
        horizontalinput = Input.GetAxisRaw("Horizontal");
        verticalinput = Input.GetAxisRaw("Vertical");
        inputdirection = new Vector3(horizontalinput,0f,verticalinput).normalized;
        ani.SetFloat("Velocity",inputdirection.magnitude);
        inputangle = Mathf.Atan2(inputdirection.x,inputdirection.z) * Mathf.Rad2Deg ; 
        targetangle = inputangle + cam.eulerAngles.y;
        movedirction = Quaternion.Euler(0f,targetangle,0f) * Vector3.forward ;
        ontheGround = OntheGround();
        running = Input.GetKey(KeyCode.LeftShift);

        if(Input.GetKeyDown(KeyCode.B)&&ontheGround&&Time.time>=LastJumpingTime+0.2f){
            jumping = true;
            LastJumpingTime = Time.time;
        }

        if(inputdirection.magnitude >=0.1f){
            if(running){
                transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.Euler(0,targetangle,0),0.2f);
                transform.position += movedirction*velocity*Time.deltaTime*3f;
                ani.SetBool("IsRunning",true);
            }
            if(!running){
                transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.Euler(0,targetangle,0),0.1f);
                transform.position += movedirction*velocity*Time.deltaTime; 
                ani.SetBool("IsRunning",false);
            }
        }
        if (inputdirection.magnitude <0.1f){
            ani.SetBool("IsRunning",false);
        }
    }
    bool OntheGround(){
        return Physics.Raycast(transform.position+Vector3.up, -Vector3.up, 1.1f);
    }
    void FixedUpdate(){
        
        if(jumping){
            rb.velocity = JumpHeight;
            ani.SetBool("IsJumping",true);
            jumping = false;
        }
        if(Time.time>=LastJumpingTime+0.5f&&ontheGround){
            ani.SetBool("IsJumping",false);
        }
        
    }
}
