using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : StateMachineBehaviour
{
    ThirdPersonController ThirdPersonController;
    VNectModel VNectModel;
    // FacePoseEstimation FacePoseEstimation;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ThirdPersonController = animator.gameObject.GetComponent<ThirdPersonController>();
        VNectModel = animator.gameObject.GetComponent<VNectModel>();
        // FacePoseEstimation = animator.gameObject.GetComponent<FacePoseEstimation>();

        Physics.IgnoreLayerCollision(6,9,true);
        ThirdPersonController.enabled = false;
        VNectModel.SetState(VNectModel.GetModelState(0));
        // FacePoseEstimation.SetisfaceupdatingFalse();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Death",false);
        animator.SetBool("IsJumping",false);
    }
}
