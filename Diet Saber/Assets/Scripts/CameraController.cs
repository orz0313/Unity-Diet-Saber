using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
public class CameraController : MonoBehaviour
{
    GameObject Player;
    ThirdPersonController ThirdPersonController;
    // VNectModel VNectModel;
    // [SerializeField] MaskableGraphic FaceRawImage;
    [SerializeField] CinemachineVirtualCameraBase FixedCamera;
    [SerializeField] CinemachineVirtualCameraBase FaceCamera;
    [SerializeField] CinemachineVirtualCameraBase ThirdPCamera;

    List<CinemachineVirtualCameraBase> CameraList = new List<CinemachineVirtualCameraBase>();
    int currentpriority;

    void Awake()
    {
        
        Player = GameObject.FindGameObjectWithTag("Player");
        ThirdPersonController = Player.GetComponent<ThirdPersonController>();
        // VNectModel = Player.GetComponent<VNectModel>();
        CameraList.Add(FixedCamera);//index 0
        CameraList.Add(FaceCamera);//index 1
        CameraList.Add(ThirdPCamera);//index 2

        foreach(CinemachineVirtualCameraBase i in CameraList)
        {
            if (i.Priority>currentpriority){
                currentpriority = i.Priority;
            }
        }
        
    }   
     void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            CameraList[0].enabled = true;
            CameraList[0].Priority = currentpriority + 1;
            currentpriority = CameraList[0].Priority;
            CameraList[1].enabled = false;
            CameraList[2].enabled = false;
            ThirdPersonController.enabled = false;
            // FaceRawImage.enabled = false;
        }
        if(Input.GetKeyDown(KeyCode.F2))
        {
            CameraList[1].enabled = true;
            CameraList[1].Priority = currentpriority + 1;
            currentpriority = CameraList[1].Priority;
            CameraList[0].enabled = false;
            CameraList[2].enabled = false;
            ThirdPersonController.enabled = false;
            // FaceRawImage.enabled = false;
        }
        if(Input.GetKeyDown(KeyCode.F3))
        {
            CameraList[2].enabled = true;
            CameraList[2].Priority = currentpriority + 1;
            currentpriority = CameraList[2].Priority;
            CameraList[0].enabled = false;
            CameraList[1].enabled = false;
            // FaceRawImage.enabled = true;
            if(ThirdPersonController.enabled == false)
            {
                ThirdPersonController.enabled = true;
            }
            else
            {
                return;
            }
        }
    }
}
