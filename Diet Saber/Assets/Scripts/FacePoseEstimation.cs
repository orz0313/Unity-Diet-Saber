using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;


public class FacePoseEstimation : MonoBehaviour
{
    float eyeclose=0.195f,eyeopen=0.205f,mouthclose=0.3f,mouthopen=0.8f;
    public SkinnedMeshRenderer eye,eyelid,eyebrow,mouth;
    Vector3 Head = new Vector3(0,1.35f,0);
    Animator animator;
    [SerializeField]float roll=0,pitch=0,yaw=0,min_ear=1f,mar=0f,mdst=0.25f;
    Transform neck;
    Quaternion neckQua;
    Quaternion rotate;
    Quaternion animationNeckRotate;
    Quaternion aniCurrentRotate;
    Thread receiveThread;
    TcpClient Client;
    TcpListener Listener;
    int port = 5066;
    [SerializeField]bool isfaceupdating;
    bool isforward = false;
    bool isbackward = false;

    void Start()
    {
        Application.targetFrameRate = 30;
        animator = GetComponent<Animator>();
        neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        neckQua = Quaternion.Euler(0,-90,-90);
        InitializeTCP();
        
    }
    public void SetisfaceupdatingTrue(){
        // isfaceupdating = true;
        StartCoroutine(Forward());
    }
    IEnumerator Forward(){
        aniCurrentRotate = neck.rotation;
        isforward = true;
        yield return new WaitForSeconds(2f);
        isforward = false;
        isfaceupdating = true;
    }

    public void SetisfaceupdatingFalse(){
        // isfaceupdating = false;
        StartCoroutine(Backward());
    }
    IEnumerator Backward(){
        aniCurrentRotate = rotate;
        isbackward = true;
        isfaceupdating = false;
        yield return new WaitForSeconds(2f);
        isbackward = false;
    }

    private void InitializeTCP(){
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }
    private void ReceiveData(){
        try{
            Listener = new TcpListener(IPAddress.Parse("127.0.0.1"),port);
            Listener.Start();
            Byte[] bytes = new Byte[1024];
            while(true){
                using(Client=Listener.AcceptTcpClient()){
                    using(NetworkStream stream = Client.GetStream()){
                        int length;
                        while((length = stream.Read(bytes,0,bytes.Length))!=0){
                            var incomedata = new byte[length];
                            Array.Copy(bytes,0,incomedata,0,length);
                            string ClientMessage = Encoding.ASCII.GetString(incomedata);
                            string[] res = ClientMessage.Split(' ');
                            roll = float.Parse(res[0])*0.4f+roll*0.6f;
                            pitch = float.Parse(res[1])*0.4f+pitch*0.6f;
                            yaw = float.Parse(res[2])*0.4f+yaw*0.6f;
                            min_ear = float.Parse(res[3])*0.4f;
                            mar = float.Parse(res[4])*0.4f+mar*0.6f;
                            mdst = float.Parse(res[5])*0.4f;
                        }
                    }
                }
            }
        }catch(Exception e){
            print (e.ToString());
        }
    }
    void Update(){
        rotate = Quaternion.Euler(-pitch,yaw+this.transform.eulerAngles.y,-roll) * neckQua;
        animationNeckRotate = neck.rotation;
    }
    void LateUpdate(){
        if(isforward){
            neck.rotation = Quaternion.Slerp(aniCurrentRotate,rotate,0.3f);
            aniCurrentRotate = neck.rotation;
        }
        if(isbackward){
            neck.rotation = Quaternion.Slerp(aniCurrentRotate,animationNeckRotate,0.3f);
            aniCurrentRotate = neck.rotation;
        }

        if(isfaceupdating){
            neck.rotation = rotate;

            min_ear = Mathf.Clamp(min_ear,eyeclose,eyeopen);
            float eyeratio = -100/(eyeopen-eyeclose)*(min_ear-eyeopen);

            mar = Mathf.Clamp(mar,mouthclose,mouthopen);
            float mouthratio = 100/(mouthopen-mouthclose)*(mar-mouthclose);

            if(mdst > 0.33f){
                animator.SetLayerWeight(1,1f);
                animator.CrossFade("smile1@unitychan",0.1f);
            }
            else{
                animator.SetLayerWeight(1,1f);
                animator.CrossFade("default@unitychan",0.1f);
            //     // eye.SetBlendShapeWeight(6,eyeratio);
            //     // eyelid.SetBlendShapeWeight(6,eyeratio);
               
                if(mouthratio>70){
            //         // eye.SetBlendShapeWeight(2,100);
            //         // eyebrow.SetBlendShapeWeight(2,100);
                    mouth.SetBlendShapeWeight(0,mouthratio);
                }
                else{
                    mouth.SetBlendShapeWeight(2,mouthratio);
                    mouth.SetBlendShapeWeight(1,80);
                }
            }
        }
    }
    public Quaternion GetRPY(){
        return rotate;
    }
}
