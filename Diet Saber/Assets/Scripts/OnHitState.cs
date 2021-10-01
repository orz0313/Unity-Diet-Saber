using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHitState : StateMachineBehaviour
{
    ThirdPersonController ThirdPersonController;
    VNectModel VNectModel;
    // FacePoseEstimation FacePoseEstimation;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ThirdPersonController = animator.gameObject.GetComponent<ThirdPersonController>();
        // FacePoseEstimation = animator.gameObject.GetComponent<FacePoseEstimation>();
        VNectModel = animator.gameObject.GetComponent<VNectModel>();
        
        Physics.IgnoreLayerCollision(6,9,true);
        ThirdPersonController.enabled = false;
        VNectModel.SetState(VNectModel.GetModelState(0));
        // FacePoseEstimation.SetisfaceupdatingFalse();        
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Physics.IgnoreLayerCollision(6,9,false);
        // ThirdPersonController.enabled = true;
        VNectModel.SetState(VNectModel.PreviousState);
        // FacePoseEstimation.SetisfaceupdatingTrue();       
    }
}
