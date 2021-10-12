using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseState : StateMachineBehaviour
{
    
    ThirdPersonController ThirdPersonController;
    VNectModel VNectModel;
    // FacePoseEstimation FacePoseEstimation;
    PlayerInfo PlayerInfoInstance;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ThirdPersonController = animator.gameObject.GetComponent<ThirdPersonController>();
        VNectModel = animator.gameObject.GetComponent<VNectModel>();
        // FacePoseEstimation = animator.gameObject.GetComponent<FacePoseEstimation>();
        PlayerInfoInstance = animator.gameObject.GetComponent<PlayerInfo>();

        animator.gameObject.transform.position = PlayerInfoInstance.GetRespawnPoint();
        animator.gameObject.transform.rotation = Quaternion.identity;     
           
    }


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Physics.IgnoreLayerCollision(6,9,false);
        ThirdPersonController.enabled = true;
        VNectModel.SetState(VNectModel.PreviousState);
        // FacePoseEstimation.SetisfaceupdatingTrue();       
    }
}
