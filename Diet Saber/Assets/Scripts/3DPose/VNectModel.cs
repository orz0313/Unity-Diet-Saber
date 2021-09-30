using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Position index of joint points
/// </summary>
public enum PositionIndex : int
{

    rShldrBend =0,
    rForearmBend,
    rHand,
    rThumb2,
    rMid1,

    lShldrBend,
    lForearmBend,
    lHand,
    lThumb2,
    lMid1,

    lEar,
    lEye,
    rEar,
    rEye,
    Nose,

    rThighBend,
    rShin,
    rFoot,
    rToe,

    lThighBend,
    lShin,
    lFoot,
    lToe,


    //Calculated coordinates
    abdomenUpper,
    hip,
    head,
    neck,
    spine,

    Count,
    None,
}
public static partial class EnumExtend
{
    public static int Int(this PositionIndex i)
    {
        return (int)i;
    }
}
public class VNectModel : MonoBehaviour
{
    public class JointPoint
    {
        public Vector2 Pos2D = new Vector2();
        public float score2D;

        public Vector3 Pos3D = new Vector3();
        public Vector3 Now3D = new Vector3();
        public Vector3[] PrevPos3D = new Vector3[6];
        public float score3D;

        // Bones
        public Transform Transform = null;
        public Quaternion InitRotation;
        public Quaternion Inverse;
        public Quaternion InverseRotation;

        public JointPoint Child = null;
        public JointPoint Parent = null;

        // For Kalman filter
        public Vector3 P = new Vector3();
        public Vector3 X = new Vector3();
        public Vector3 K = new Vector3();
    }
    public class VirtualJointPoint
    {
        public Transform Transform = null;
        public Quaternion Rotation;
        public Vector3 Position;

    }
    public enum ModelState
    {
        FullAnimation,
        UpperHalfPoseUpdating,
        FullPoseUpdating
    }
    public class Skeleton
    {
        public GameObject LineObject;
        public LineRenderer Line;

        public JointPoint start = null;
        public JointPoint end = null;
    }
    private List<Skeleton> Skeletons = new List<Skeleton>();
    public Material SkeletonMaterial;
    public bool ShowSkeleton;
    private bool useSkeleton;
    public float SkeletonX;
    public float SkeletonY;
    public float SkeletonZ;
    public float SkeletonScale;
    // Joint position and bone
    private JointPoint[] jointPoints;
    public JointPoint[] JointPoints { get { return jointPoints; } }
    private VirtualJointPoint[] VirtualJointPoints;
    private Vector3 initPosition; // Initial center position
    private Quaternion InitGazeRotation;
    private Quaternion gazeInverse;
    [SerializeField]TextMeshProUGUI TextMeshProUGUI;
    // UnityChan
    public GameObject ModelObject;
    public GameObject Nose;
    private Animator anim;
    // Move in z direction
    private float centerTall = 224 * 0.75f;
    private float tall = 224 * 0.75f;
    private float prevTall = 224 * 0.75f;
    private float dz;
    private float currentTime;
    private float Slerpinterpolated;
    public float ZScale = 0.8f;
    Vector3 forward = new Vector3(0,0,0);
    [SerializeField]bool IsFullbodyPoseUpdating = false;
    [SerializeField]bool IsUpperHalfPoseUpdating = false;
    [SerializeField]bool IsAnimationUpdating = true;
    [SerializeField]bool IsSlerptoFullbodyPoseUpdate = false;
    [SerializeField]bool IsSlerptoUpperHalfPoseUpdate = false;
    [SerializeField]bool IsSlerptoFullAnimation = false;
    [SerializeField]Vector3 VirtualGameobjectPosition;
    [SerializeField]Vector3 VirtualGameobjectRotation;
    [SerializeField]GameObject Hip;
    Vector3 OffsetPosition;
    Vector3 OffsetRotation;
    ModelState CurrentState = (ModelState)0;
    public ModelState PreviousState = (ModelState)0;
    private void Awake()
    {
        anim = ModelObject.GetComponent<Animator>();
        TextMeshProUGUI.text = "Animation Updating Mode";
        VirtualJointPoints = new VirtualJointPoint[PositionIndex.Count.Int()];
        OffsetPosition = transform.position;
        OffsetRotation = transform.eulerAngles;

        for(int i = 0;i < PositionIndex.Count.Int();i++)
        {
            VirtualJointPoints[i] = new VirtualJointPoint();
        }

    }
    private void Update()
    {
        if (jointPoints != null)
        {
            PoseCalculate();
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SetState((ModelState)0);
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
             SetState((ModelState)1);
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            SetState((ModelState)2);
        }
    }
    private void LateUpdate()
    {
        if(IsSlerptoFullAnimation)
        {
            SlerptoFullAnimation();
        }

        if(IsSlerptoFullbodyPoseUpdate)
        {
            SlerptoFullbodyPoseUpdate();
        }

        if(IsFullbodyPoseUpdating)
        {
            FullbodyPoseUpdate();
        }
        if(IsUpperHalfPoseUpdating)
        {
            UpperHalfPoseUpdating();
        }
        if(IsSlerptoUpperHalfPoseUpdate)
        {
            SlerptoUpperHalfPoseUpdate();
        }
    }
    /// <summary>
    /// Initialize joint points
    /// </summary>
    /// <returns></returns>
    public JointPoint[] Init()
    {
        jointPoints = new JointPoint[PositionIndex.Count.Int()];
        for (var i = 0; i < PositionIndex.Count.Int(); i++) jointPoints[i] = new JointPoint();

        // anim = ModelObject.GetComponent<Animator>();

        // etc
        jointPoints[PositionIndex.abdomenUpper.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Spine);
        jointPoints[PositionIndex.hip.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Hips);
        jointPoints[PositionIndex.head.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.neck.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Neck);
        jointPoints[PositionIndex.spine.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Spine);

        // Right Arm
        jointPoints[PositionIndex.rShldrBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        jointPoints[PositionIndex.rForearmBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        jointPoints[PositionIndex.rHand.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightHand);
        jointPoints[PositionIndex.rThumb2.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        jointPoints[PositionIndex.rMid1.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
        // Left Arm
        jointPoints[PositionIndex.lShldrBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        jointPoints[PositionIndex.lForearmBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        jointPoints[PositionIndex.lHand.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        jointPoints[PositionIndex.lThumb2.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        jointPoints[PositionIndex.lMid1.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);

        // Face
        jointPoints[PositionIndex.lEar.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.lEye.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftEye);
        jointPoints[PositionIndex.rEar.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.rEye.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightEye);
        jointPoints[PositionIndex.Nose.Int()].Transform = Nose.transform;

        // Right Leg
        jointPoints[PositionIndex.rThighBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        jointPoints[PositionIndex.rShin.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        jointPoints[PositionIndex.rFoot.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        jointPoints[PositionIndex.rToe.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightToes);

        // Left Leg
        jointPoints[PositionIndex.lThighBend.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        jointPoints[PositionIndex.lShin.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        jointPoints[PositionIndex.lFoot.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        jointPoints[PositionIndex.lToe.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftToes);


        // Child Settings
        // Right Arm
        jointPoints[PositionIndex.rShldrBend.Int()].Child = jointPoints[PositionIndex.rForearmBend.Int()];
        jointPoints[PositionIndex.rForearmBend.Int()].Child = jointPoints[PositionIndex.rHand.Int()];
        jointPoints[PositionIndex.rForearmBend.Int()].Parent = jointPoints[PositionIndex.rShldrBend.Int()];

        // Left Arm
        jointPoints[PositionIndex.lShldrBend.Int()].Child = jointPoints[PositionIndex.lForearmBend.Int()];
        jointPoints[PositionIndex.lForearmBend.Int()].Child = jointPoints[PositionIndex.lHand.Int()];
        jointPoints[PositionIndex.lForearmBend.Int()].Parent = jointPoints[PositionIndex.lShldrBend.Int()];

        // Fase

        // Right Leg
        jointPoints[PositionIndex.rThighBend.Int()].Child = jointPoints[PositionIndex.rShin.Int()];
        jointPoints[PositionIndex.rShin.Int()].Child = jointPoints[PositionIndex.rFoot.Int()];
        jointPoints[PositionIndex.rFoot.Int()].Child = jointPoints[PositionIndex.rToe.Int()];
        jointPoints[PositionIndex.rFoot.Int()].Parent = jointPoints[PositionIndex.rShin.Int()];

        // Left Leg
        jointPoints[PositionIndex.lThighBend.Int()].Child = jointPoints[PositionIndex.lShin.Int()];
        jointPoints[PositionIndex.lShin.Int()].Child = jointPoints[PositionIndex.lFoot.Int()];
        jointPoints[PositionIndex.lFoot.Int()].Child = jointPoints[PositionIndex.lToe.Int()];
        jointPoints[PositionIndex.lFoot.Int()].Parent = jointPoints[PositionIndex.lShin.Int()];

        // etc
        jointPoints[PositionIndex.spine.Int()].Child = jointPoints[PositionIndex.neck.Int()];
        jointPoints[PositionIndex.neck.Int()].Child = jointPoints[PositionIndex.head.Int()];
        //jointPoints[PositionIndex.head.Int()].Child = jointPoints[PositionIndex.Nose.Int()];

        useSkeleton = ShowSkeleton;
        if (useSkeleton)
        {
            // Line Child Settings
            // Right Arm
            AddSkeleton(PositionIndex.rShldrBend, PositionIndex.rForearmBend);
            AddSkeleton(PositionIndex.rForearmBend, PositionIndex.rHand);
            AddSkeleton(PositionIndex.rHand, PositionIndex.rThumb2);
            AddSkeleton(PositionIndex.rHand, PositionIndex.rMid1);

            // Left Arm
            AddSkeleton(PositionIndex.lShldrBend, PositionIndex.lForearmBend);
            AddSkeleton(PositionIndex.lForearmBend, PositionIndex.lHand);
            AddSkeleton(PositionIndex.lHand, PositionIndex.lThumb2);
            AddSkeleton(PositionIndex.lHand, PositionIndex.lMid1);

            // Fase
            AddSkeleton(PositionIndex.lEar, PositionIndex.Nose);
            AddSkeleton(PositionIndex.rEar, PositionIndex.Nose);

            // Right Leg
            AddSkeleton(PositionIndex.rThighBend, PositionIndex.rShin);
            AddSkeleton(PositionIndex.rShin, PositionIndex.rFoot);
            AddSkeleton(PositionIndex.rFoot, PositionIndex.rToe);

            // Left Leg
            AddSkeleton(PositionIndex.lThighBend, PositionIndex.lShin);
            AddSkeleton(PositionIndex.lShin, PositionIndex.lFoot);
            AddSkeleton(PositionIndex.lFoot, PositionIndex.lToe);

            // etc
            AddSkeleton(PositionIndex.spine, PositionIndex.neck);
            AddSkeleton(PositionIndex.neck, PositionIndex.head);
            AddSkeleton(PositionIndex.head, PositionIndex.Nose);
            AddSkeleton(PositionIndex.neck, PositionIndex.rShldrBend);
            AddSkeleton(PositionIndex.neck, PositionIndex.lShldrBend);
            AddSkeleton(PositionIndex.rThighBend, PositionIndex.rShldrBend);
            AddSkeleton(PositionIndex.lThighBend, PositionIndex.lShldrBend);
            AddSkeleton(PositionIndex.rShldrBend, PositionIndex.abdomenUpper);
            AddSkeleton(PositionIndex.lShldrBend, PositionIndex.abdomenUpper);
            AddSkeleton(PositionIndex.rThighBend, PositionIndex.abdomenUpper);
            AddSkeleton(PositionIndex.lThighBend, PositionIndex.abdomenUpper);
            AddSkeleton(PositionIndex.lThighBend, PositionIndex.rThighBend);
        }

        // Set Inverse
        var forward = TriangleNormal(jointPoints[PositionIndex.hip.Int()].Transform.position, jointPoints[PositionIndex.lThighBend.Int()].Transform.position, jointPoints[PositionIndex.rThighBend.Int()].Transform.position);
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Transform != null)
            {
                jointPoint.InitRotation = jointPoint.Transform.rotation;
            }

            if (jointPoint.Child != null)
            {
                jointPoint.Inverse = GetInverse(jointPoint, jointPoint.Child, forward);
                jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
            }
        }
        var hip = jointPoints[PositionIndex.hip.Int()];
        initPosition = jointPoints[PositionIndex.hip.Int()].Transform.position;
        hip.Inverse = Quaternion.Inverse(Quaternion.LookRotation(forward));
        hip.InverseRotation = hip.Inverse * hip.InitRotation;

        // For Head Rotation
        var head = jointPoints[PositionIndex.head.Int()];
        head.InitRotation = jointPoints[PositionIndex.head.Int()].Transform.rotation;
        var gaze = jointPoints[PositionIndex.Nose.Int()].Transform.position - jointPoints[PositionIndex.head.Int()].Transform.position;
        head.Inverse = Quaternion.Inverse(Quaternion.LookRotation(gaze));
        head.InverseRotation = head.Inverse * head.InitRotation;
        
        var lHand = jointPoints[PositionIndex.lHand.Int()];
        var lf = TriangleNormal(lHand.Pos3D, jointPoints[PositionIndex.lMid1.Int()].Pos3D, jointPoints[PositionIndex.lThumb2.Int()].Pos3D);
        lHand.InitRotation = lHand.Transform.rotation;
        lHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.lThumb2.Int()].Transform.position - jointPoints[PositionIndex.lMid1.Int()].Transform.position, lf));
        lHand.InverseRotation = lHand.Inverse * lHand.InitRotation;

        var rHand = jointPoints[PositionIndex.rHand.Int()];
        var rf = TriangleNormal(rHand.Pos3D, jointPoints[PositionIndex.rThumb2.Int()].Pos3D, jointPoints[PositionIndex.rMid1.Int()].Pos3D);
        rHand.InitRotation = jointPoints[PositionIndex.rHand.Int()].Transform.rotation;
        rHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.rThumb2.Int()].Transform.position - jointPoints[PositionIndex.rMid1.Int()].Transform.position, rf));
        rHand.InverseRotation = rHand.Inverse * rHand.InitRotation;

        jointPoints[PositionIndex.hip.Int()].score3D = 1f;
        jointPoints[PositionIndex.neck.Int()].score3D = 1f;
        jointPoints[PositionIndex.Nose.Int()].score3D = 1f;
        jointPoints[PositionIndex.head.Int()].score3D = 1f;
        jointPoints[PositionIndex.spine.Int()].score3D = 1f;


        return JointPoints;
    }
    public void PoseCalculate()
    {
        // caliculate movement range of z-coordinate from height
        var t1 = Vector3.Distance(jointPoints[PositionIndex.head.Int()].Pos3D, jointPoints[PositionIndex.neck.Int()].Pos3D);
        var t2 = Vector3.Distance(jointPoints[PositionIndex.neck.Int()].Pos3D, jointPoints[PositionIndex.spine.Int()].Pos3D);
        var pm = (jointPoints[PositionIndex.rThighBend.Int()].Pos3D + jointPoints[PositionIndex.lThighBend.Int()].Pos3D) / 2f;
        var t3 = Vector3.Distance(jointPoints[PositionIndex.spine.Int()].Pos3D, pm);
        var t4r = Vector3.Distance(jointPoints[PositionIndex.rThighBend.Int()].Pos3D, jointPoints[PositionIndex.rShin.Int()].Pos3D);
        var t4l = Vector3.Distance(jointPoints[PositionIndex.lThighBend.Int()].Pos3D, jointPoints[PositionIndex.lShin.Int()].Pos3D);
        var t4 = (t4r + t4l) / 2f;
        var t5r = Vector3.Distance(jointPoints[PositionIndex.rShin.Int()].Pos3D, jointPoints[PositionIndex.rFoot.Int()].Pos3D);
        var t5l = Vector3.Distance(jointPoints[PositionIndex.lShin.Int()].Pos3D, jointPoints[PositionIndex.lFoot.Int()].Pos3D);
        var t5 = (t5r + t5l) / 2f;
        var t = t1 + t2 + t3 + t4 + t5;


        // Low pass filter in z direction
        tall = t * 0.7f + prevTall * 0.3f;
        prevTall = tall;

        if (tall == 0)
        {
            tall = centerTall;
        }
        dz = (centerTall - tall) / centerTall * ZScale;
        forward = TriangleNormal(jointPoints[PositionIndex.hip.Int()].Pos3D, jointPoints[PositionIndex.lThighBend.Int()].Pos3D, jointPoints[PositionIndex.rThighBend.Int()].Pos3D);
    }
    void FullbodyPoseUpdate()
    {
        // movement and rotatation of center
        forward = TriangleNormal(jointPoints[PositionIndex.hip.Int()].Pos3D, jointPoints[PositionIndex.lThighBend.Int()].Pos3D, jointPoints[PositionIndex.rThighBend.Int()].Pos3D);
        
        // Gameobject transform
        transform.position = jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz) + VirtualGameobjectPosition + new Vector3(0f,-0.87f,0f) - OffsetPosition;
        transform.eulerAngles = new Vector3(0f,-90f+(Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation).eulerAngles.y,0f) + VirtualGameobjectRotation - OffsetRotation;

        jointPoints[PositionIndex.hip.Int()].Transform.position = jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz);
        jointPoints[PositionIndex.hip.Int()].Transform.rotation = Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation;
        
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Parent != null)
            {
                var fv = jointPoint.Parent.Pos3D - jointPoint.Pos3D;
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, fv) * jointPoint.InverseRotation;
            }
            else if (jointPoint.Child != null)
            {
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, forward) * jointPoint.InverseRotation;
            }
        }

        // Head Rotation
        // var gaze = jointPoints[PositionIndex.Nose.Int()].Pos3D - jointPoints[PositionIndex.head.Int()].Pos3D;
        // var f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].Pos3D, jointPoints[PositionIndex.rEar.Int()].Pos3D, jointPoints[PositionIndex.lEar.Int()].Pos3D);
        // var head = jointPoints[PositionIndex.head.Int()];
        // head.Transform.rotation = Quaternion.LookRotation(gaze, f) * head.InverseRotation;
        
        // // Wrist rotation (Test code)
        // var lHand = jointPoints[PositionIndex.lHand.Int()];
        // var lf = TriangleNormal(lHand.Pos3D, jointPoints[PositionIndex.lMid1.Int()].Pos3D, jointPoints[PositionIndex.lThumb2.Int()].Pos3D);
        // lHand.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.lThumb2.Int()].Pos3D - jointPoints[PositionIndex.lMid1.Int()].Pos3D, lf) * lHand.InverseRotation;

        // var rHand = jointPoints[PositionIndex.rHand.Int()];
        // var rf = TriangleNormal(rHand.Pos3D, jointPoints[PositionIndex.rThumb2.Int()].Pos3D, jointPoints[PositionIndex.rMid1.Int()].Pos3D);
        // rHand.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.rThumb2.Int()].Pos3D - jointPoints[PositionIndex.rMid1.Int()].Pos3D, rf) * rHand.InverseRotation;

        
        foreach (var sk in Skeletons)
        {
            var s = sk.start;
            var e = sk.end;

            sk.Line.SetPosition(0, new Vector3(s.Pos3D.x * SkeletonScale + SkeletonX, s.Pos3D.y * SkeletonScale + SkeletonY, s.Pos3D.z * SkeletonScale + SkeletonZ));
            sk.Line.SetPosition(1, new Vector3(e.Pos3D.x * SkeletonScale + SkeletonX, e.Pos3D.y * SkeletonScale + SkeletonY, e.Pos3D.z * SkeletonScale + SkeletonZ));
        }

        // Transform for Gameworld
        Hip.transform.position = Hip.transform.position + VirtualGameobjectPosition - OffsetPosition;
        Hip.transform.eulerAngles = Hip.transform.eulerAngles + VirtualGameobjectRotation + new Vector3(0,180,0) - OffsetRotation;
    }
    void UpperHalfPoseUpdating()
    {
        // movement and rotatation of center
        forward = TriangleNormal(jointPoints[PositionIndex.hip.Int()].Pos3D, jointPoints[PositionIndex.lThighBend.Int()].Pos3D, jointPoints[PositionIndex.rThighBend.Int()].Pos3D);

        VirtualGameobjectPosition = transform.position;
        VirtualGameobjectRotation = transform.eulerAngles;

        jointPoints[PositionIndex.hip.Int()].Transform.position = jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz);
        jointPoints[PositionIndex.hip.Int()].Transform.rotation = Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation;
        
        for (int i=0 ; i<15 ; i++)
        {
            if (jointPoints[i].Parent != null)
            {
                var fv = jointPoints[i].Parent.Pos3D - jointPoints[i].Pos3D;
                jointPoints[i].Transform.rotation = Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, fv) * jointPoints[i].InverseRotation;
            }
            else if (jointPoints[i].Child != null)
            {
                jointPoints[i].Transform.rotation = Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, forward) * jointPoints[i].InverseRotation;
            }
        }
        for (int i=23 ; i<28 ; i++)
        {
            if (jointPoints[i].Parent != null)
            {
                var fv = jointPoints[i].Parent.Pos3D - jointPoints[i].Pos3D;
                jointPoints[i].Transform.rotation = Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, fv) * jointPoints[i].InverseRotation;
            }
            else if (jointPoints[i].Child != null)
            {
                jointPoints[i].Transform.rotation = Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, forward) * jointPoints[i].InverseRotation;
            }
        }

        // Head Rotation
        // var gaze = jointPoints[PositionIndex.Nose.Int()].Pos3D - jointPoints[PositionIndex.head.Int()].Pos3D;
        // var f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].Pos3D, jointPoints[PositionIndex.rEar.Int()].Pos3D, jointPoints[PositionIndex.lEar.Int()].Pos3D);
        // var head = jointPoints[PositionIndex.head.Int()];
        // head.Transform.rotation = Quaternion.LookRotation(gaze, f) * head.InverseRotation;
        
        // // Wrist rotation (Test code)
        // var lHand = jointPoints[PositionIndex.lHand.Int()];
        // var lf = TriangleNormal(lHand.Pos3D, jointPoints[PositionIndex.lMid1.Int()].Pos3D, jointPoints[PositionIndex.lThumb2.Int()].Pos3D);
        // lHand.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.lThumb2.Int()].Pos3D - jointPoints[PositionIndex.lMid1.Int()].Pos3D, lf) * lHand.InverseRotation;

        // var rHand = jointPoints[PositionIndex.rHand.Int()];
        // var rf = TriangleNormal(rHand.Pos3D, jointPoints[PositionIndex.rThumb2.Int()].Pos3D, jointPoints[PositionIndex.rMid1.Int()].Pos3D);
        // rHand.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.rThumb2.Int()].Pos3D - jointPoints[PositionIndex.rMid1.Int()].Pos3D, rf) * rHand.InverseRotation;
        
        Hip.transform.position = Hip.transform.position + VirtualGameobjectPosition - OffsetPosition;
        Hip.transform.eulerAngles = Hip.transform.eulerAngles + VirtualGameobjectRotation + new Vector3(0,180,0) - OffsetRotation;
    }
    void SlerptoFullbodyPoseUpdate()
    {
        forward = TriangleNormal(jointPoints[PositionIndex.hip.Int()].Pos3D, jointPoints[PositionIndex.lThighBend.Int()].Pos3D, jointPoints[PositionIndex.rThighBend.Int()].Pos3D);
        
        Slerpinterpolated = 0.1f+(Time.time - currentTime)/2.5f;

        transform.position = jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz) + VirtualGameobjectPosition +new Vector3(0f,-0.87f,0f) - OffsetPosition;
        transform.eulerAngles = new Vector3(0f,-90f+(Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation).eulerAngles.y,0f) + VirtualGameobjectRotation - OffsetRotation;

        jointPoints[PositionIndex.hip.Int()].Transform.position = Vector3.Lerp(VirtualJointPoints[PositionIndex.hip.Int()].Position,jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz),Slerpinterpolated);
        VirtualJointPoints[PositionIndex.hip.Int()].Position = jointPoints[PositionIndex.hip.Int()].Transform.position;
               
        jointPoints[PositionIndex.hip.Int()].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.hip.Int()].Rotation,Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation,Slerpinterpolated);
        VirtualJointPoints[PositionIndex.hip.Int()].Rotation = jointPoints[PositionIndex.hip.Int()].Transform.rotation;

        for (int i=0 ; i<PositionIndex.Count.Int() ; i++)
        {
            if (jointPoints[i].Parent != null)
            {
                var fv = jointPoints[i].Parent.Pos3D - jointPoints[i].Pos3D;
                jointPoints[i].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[i].Rotation,Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, fv) * jointPoints[i].InverseRotation,Slerpinterpolated);
                VirtualJointPoints[i].Rotation = jointPoints[i].Transform.rotation;
            }
            else if (jointPoints[i].Child != null)
            {
                jointPoints[i].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[i].Rotation,Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, forward) * jointPoints[i].InverseRotation,Slerpinterpolated);
                VirtualJointPoints[i].Rotation = jointPoints[i].Transform.rotation;
            }
        }
        // Head Rotation
        // var gaze = jointPoints[PositionIndex.Nose.Int()].Pos3D - jointPoints[PositionIndex.head.Int()].Pos3D;
        // var f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].Pos3D, jointPoints[PositionIndex.rEar.Int()].Pos3D, jointPoints[PositionIndex.lEar.Int()].Pos3D);
        // var head = jointPoints[PositionIndex.head.Int()];
        // head.Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.head.Int()].Rotation,Quaternion.LookRotation(gaze, f) * head.InverseRotation,Slerpinterpolated);
        // VirtualJointPoints[PositionIndex.head.Int()].Rotation = head.Transform.rotation;

        // // // // Wrist rotation (Test code)
        // var lHand = jointPoints[PositionIndex.lHand.Int()];
        // var lf = TriangleNormal(lHand.Pos3D, jointPoints[PositionIndex.lMid1.Int()].Pos3D, jointPoints[PositionIndex.lThumb2.Int()].Pos3D);
        // lHand.Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.lHand.Int()].Rotation,Quaternion.LookRotation(jointPoints[PositionIndex.lThumb2.Int()].Pos3D - jointPoints[PositionIndex.lMid1.Int()].Pos3D, lf) * lHand.InverseRotation,Slerpinterpolated);
        // VirtualJointPoints[PositionIndex.lHand.Int()].Rotation = lHand.Transform.rotation;

        // var rHand = jointPoints[PositionIndex.rHand.Int()];
        // var rf = TriangleNormal(rHand.Pos3D, jointPoints[PositionIndex.rThumb2.Int()].Pos3D, jointPoints[PositionIndex.rMid1.Int()].Pos3D);
        // rHand.Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.rHand.Int()].Rotation,Quaternion.LookRotation(jointPoints[PositionIndex.rThumb2.Int()].Pos3D - jointPoints[PositionIndex.rMid1.Int()].Pos3D, rf) * rHand.InverseRotation,Slerpinterpolated);            
        // VirtualJointPoints[PositionIndex.rHand.Int()].Rotation = rHand.Transform.rotation;

        // Transform for Gameworld
        Hip.transform.position = Hip.transform.position + VirtualGameobjectPosition - OffsetPosition;
        Hip.transform.eulerAngles = Hip.transform.eulerAngles + VirtualGameobjectRotation + new Vector3(0,180,0) - OffsetRotation;
    }
    void SlerptoUpperHalfPoseUpdate()
    {
        forward = TriangleNormal(jointPoints[PositionIndex.hip.Int()].Pos3D, jointPoints[PositionIndex.lThighBend.Int()].Pos3D, jointPoints[PositionIndex.rThighBend.Int()].Pos3D);
        
        Slerpinterpolated = 0.1f+(Time.time - currentTime)/2.5f;
        
        VirtualGameobjectPosition = transform.position;
        VirtualGameobjectRotation = transform.eulerAngles;

        jointPoints[PositionIndex.hip.Int()].Transform.position = Vector3.Lerp(VirtualJointPoints[PositionIndex.hip.Int()].Position,jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz),Slerpinterpolated);
        VirtualJointPoints[PositionIndex.hip.Int()].Position = jointPoints[PositionIndex.hip.Int()].Transform.position;
        
        jointPoints[PositionIndex.hip.Int()].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.hip.Int()].Rotation,Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation,Slerpinterpolated);
        VirtualJointPoints[PositionIndex.hip.Int()].Rotation = jointPoints[PositionIndex.hip.Int()].Transform.rotation;

        for (int i=0 ; i<15 ; i++)
        {
            if (jointPoints[i].Parent != null)
            {
                var fv = jointPoints[i].Parent.Pos3D - jointPoints[i].Pos3D;
                jointPoints[i].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[i].Rotation,Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, fv) * jointPoints[i].InverseRotation,Slerpinterpolated);
                VirtualJointPoints[i].Rotation = jointPoints[i].Transform.rotation;
            }
            else if (jointPoints[i].Child != null)
            {
                jointPoints[i].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[i].Rotation,Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, forward) * jointPoints[i].InverseRotation,Slerpinterpolated);
                VirtualJointPoints[i].Rotation = jointPoints[i].Transform.rotation;
            }
        }
        for (int i=23 ; i<28 ; i++)
        {
            if (jointPoints[i].Parent != null)
            {
                var fv = jointPoints[i].Parent.Pos3D - jointPoints[i].Pos3D;
                jointPoints[i].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[i].Rotation,Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, fv) * jointPoints[i].InverseRotation,Slerpinterpolated);
                VirtualJointPoints[i].Rotation = jointPoints[i].Transform.rotation;
            }
            else if (jointPoints[i].Child != null)
            {
                jointPoints[i].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[i].Rotation,Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, forward) * jointPoints[i].InverseRotation,Slerpinterpolated);
                VirtualJointPoints[i].Rotation = jointPoints[i].Transform.rotation;
            }
        }

        for (int i=15 ; i<23 ;i++)
        {
            if (jointPoints[i].Parent != null)
            {
                var fv = jointPoints[i].Parent.Pos3D - jointPoints[i].Pos3D;
                jointPoints[i].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[i].Rotation,jointPoints[i].Transform.rotation,Slerpinterpolated);
                VirtualJointPoints[i].Rotation = jointPoints[i].Transform.rotation;
            }
            else if (jointPoints[i].Child != null)
            {
                jointPoints[i].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[i].Rotation,jointPoints[i].Transform.rotation,Slerpinterpolated);
                VirtualJointPoints[i].Rotation = jointPoints[i].Transform.rotation;
            }        
        }
        // Head Rotation
        // var gaze = jointPoints[PositionIndex.Nose.Int()].Pos3D - jointPoints[PositionIndex.head.Int()].Pos3D;
        // var f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].Pos3D, jointPoints[PositionIndex.rEar.Int()].Pos3D, jointPoints[PositionIndex.lEar.Int()].Pos3D);
        // var head = jointPoints[PositionIndex.head.Int()];
        // head.Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.head.Int()].Rotation,Quaternion.LookRotation(gaze, f) * head.InverseRotation,Slerpinterpolated);
        // VirtualJointPoints[PositionIndex.head.Int()].Rotation = head.Transform.rotation;

        // // Wrist rotation (Test code)
        // var lHand = jointPoints[PositionIndex.lHand.Int()];
        // var lf = TriangleNormal(lHand.Pos3D, jointPoints[PositionIndex.lMid1.Int()].Pos3D, jointPoints[PositionIndex.lThumb2.Int()].Pos3D);
        // lHand.Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.lHand.Int()].Rotation,Quaternion.LookRotation(jointPoints[PositionIndex.lThumb2.Int()].Pos3D - jointPoints[PositionIndex.lMid1.Int()].Pos3D, lf) * lHand.InverseRotation,Slerpinterpolated);
        // VirtualJointPoints[PositionIndex.lHand.Int()].Rotation = lHand.Transform.rotation;

        // var rHand = jointPoints[PositionIndex.rHand.Int()];
        // var rf = TriangleNormal(rHand.Pos3D, jointPoints[PositionIndex.rThumb2.Int()].Pos3D, jointPoints[PositionIndex.rMid1.Int()].Pos3D);
        // rHand.Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.rHand.Int()].Rotation,Quaternion.LookRotation(jointPoints[PositionIndex.rThumb2.Int()].Pos3D - jointPoints[PositionIndex.rMid1.Int()].Pos3D, rf) * rHand.InverseRotation,Slerpinterpolated);
        // VirtualJointPoints[PositionIndex.rHand.Int()].Rotation = rHand.Transform.rotation;

        Hip.transform.position = Hip.transform.position + VirtualGameobjectPosition - OffsetPosition;
        Hip.transform.eulerAngles = Hip.transform.eulerAngles + VirtualGameobjectRotation + new Vector3(0,180,0) - OffsetRotation;
    }
    void SlerptoFullAnimation()
    {
        forward = TriangleNormal(jointPoints[PositionIndex.hip.Int()].Pos3D, jointPoints[PositionIndex.lThighBend.Int()].Pos3D, jointPoints[PositionIndex.rThighBend.Int()].Pos3D);
        Slerpinterpolated = 0.1f+(Time.time - currentTime)/2.5f;
        
        VirtualGameobjectPosition = transform.position;
        VirtualGameobjectRotation = transform.eulerAngles;        

        jointPoints[PositionIndex.hip.Int()].Transform.position = Vector3.Lerp(VirtualJointPoints[PositionIndex.hip.Int()].Position ,jointPoints[PositionIndex.hip.Int()].Transform.position - VirtualGameobjectPosition+OffsetPosition,Slerpinterpolated);
        VirtualJointPoints[PositionIndex.hip.Int()].Position = jointPoints[PositionIndex.hip.Int()].Transform.position;
        jointPoints[PositionIndex.hip.Int()].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.hip.Int()].Rotation,RotateEulerYaxis(jointPoints[PositionIndex.hip.Int()].Transform.rotation,VirtualGameobjectRotation.y),Slerpinterpolated);
        VirtualJointPoints[PositionIndex.hip.Int()].Rotation = jointPoints[PositionIndex.hip.Int()].Transform.rotation;


        for (int i=0 ; i<PositionIndex.Count.Int() ; i++)
        {
            if (jointPoints[i].Parent != null)
            {
                jointPoints[i].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[i].Rotation,jointPoints[i].Transform.rotation,Slerpinterpolated);
                VirtualJointPoints[i].Rotation = jointPoints[i].Transform.rotation;
            }
            else if (jointPoints[i].Child != null)
            {
                jointPoints[i].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[i].Rotation,jointPoints[i].Transform.rotation,Slerpinterpolated);
                VirtualJointPoints[i].Rotation = jointPoints[i].Transform.rotation;
            }
        }
        // Head Rotation
        jointPoints[PositionIndex.head.Int()].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.head.Int()].Rotation,jointPoints[PositionIndex.head.Int()].Transform.rotation,Slerpinterpolated);
        VirtualJointPoints[PositionIndex.head.Int()].Rotation = jointPoints[PositionIndex.head.Int()].Transform.rotation;

        // // Wrist rotation (Test code)
        jointPoints[PositionIndex.lHand.Int()].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.lHand.Int()].Rotation,jointPoints[PositionIndex.lHand.Int()].Transform.rotation,Slerpinterpolated);
        VirtualJointPoints[PositionIndex.lHand.Int()].Rotation = jointPoints[PositionIndex.lHand.Int()].Transform.rotation;

        jointPoints[PositionIndex.rHand.Int()].Transform.rotation = Quaternion.Slerp(VirtualJointPoints[PositionIndex.rHand.Int()].Rotation,jointPoints[PositionIndex.rHand.Int()].Transform.rotation,Slerpinterpolated);            
        VirtualJointPoints[PositionIndex.rHand.Int()].Rotation = jointPoints[PositionIndex.rHand.Int()].Transform.rotation;

        Hip.transform.position = Hip.transform.position + VirtualGameobjectPosition - OffsetPosition;
        Hip.transform.eulerAngles = Hip.transform.eulerAngles + VirtualGameobjectRotation + new Vector3(0,180,0) - OffsetRotation;
    }
    void SetUpperHalfPoseUpdate()
    {
        if(IsAnimationUpdating | IsFullbodyPoseUpdating)
        {
            StartCoroutine(ToUpperHalfPoseUpdate());
        }
    }
    void SetFullbodyPoseUpdate()
    {
        if(IsAnimationUpdating | IsUpperHalfPoseUpdating)
        {
            StartCoroutine(ToFullbodyPoseUpdate());
        }
    }
    void SetFullAnimation()
    {
        if(IsUpperHalfPoseUpdating | IsFullbodyPoseUpdating)
        {
            StartCoroutine(ToFullAnimation());
        }
    }
    void VirtualJointPointsSyntoAnimation()
    {        
        VirtualJointPoints[PositionIndex.hip.Int()].Position = jointPoints[PositionIndex.hip.Int()].Transform.position - VirtualGameobjectPosition + OffsetPosition;
        VirtualJointPoints[PositionIndex.hip.Int()].Rotation = RotateEulerYaxis(jointPoints[PositionIndex.hip.Int()].Transform.rotation,VirtualGameobjectRotation.y);
        
        for (int i=0;i<PositionIndex.Count.Int();i++)
        {
            if (jointPoints[i].Parent != null)
            {
                VirtualJointPoints[i].Rotation = RotateEulerYaxis(jointPoints[i].Transform.rotation,VirtualGameobjectRotation.y);
            }
            else if (jointPoints[i].Child != null)
            {
                VirtualJointPoints[i].Rotation = RotateEulerYaxis(jointPoints[i].Transform.rotation,VirtualGameobjectRotation.y);
            }
        }
        VirtualJointPoints[PositionIndex.head.Int()].Rotation = RotateEulerYaxis(jointPoints[PositionIndex.head.Int()].Transform.rotation,VirtualGameobjectRotation.y);
        VirtualJointPoints[PositionIndex.lHand.Int()].Rotation = RotateEulerYaxis(jointPoints[PositionIndex.lHand.Int()].Transform.rotation,VirtualGameobjectRotation.y);
        VirtualJointPoints[PositionIndex.rHand.Int()].Rotation = RotateEulerYaxis(jointPoints[PositionIndex.rHand.Int()].Transform.rotation,VirtualGameobjectRotation.y);

    }
    void VirtualJointPointsSyntoFullbodyPose()
    {
        VirtualJointPoints[PositionIndex.hip.Int()].Position = jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz);
        VirtualJointPoints[PositionIndex.hip.Int()].Rotation = Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation;
        
        for (int i=0;i<PositionIndex.Count.Int();i++)
        {
            if (jointPoints[i].Parent != null)
            {
                var fv = jointPoints[i].Parent.Pos3D - jointPoints[i].Pos3D;
                VirtualJointPoints[i].Rotation = Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, fv) * jointPoints[i].InverseRotation;
            }
            else if (jointPoints[i].Child != null)
            {
                VirtualJointPoints[i].Rotation = Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, forward) * jointPoints[i].InverseRotation;
            }
        }

        // Head Rotation
        var gaze = jointPoints[PositionIndex.Nose.Int()].Pos3D - jointPoints[PositionIndex.head.Int()].Pos3D;
        var f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].Pos3D, jointPoints[PositionIndex.rEar.Int()].Pos3D, jointPoints[PositionIndex.lEar.Int()].Pos3D);
        var head = jointPoints[PositionIndex.head.Int()];
        VirtualJointPoints[PositionIndex.head.Int()].Rotation = Quaternion.LookRotation(gaze, f) * head.InverseRotation;
        
        // Wrist rotation (Test code)
        var lHand = jointPoints[PositionIndex.lHand.Int()];
        var lf = TriangleNormal(lHand.Pos3D, jointPoints[PositionIndex.lMid1.Int()].Pos3D, jointPoints[PositionIndex.lThumb2.Int()].Pos3D);
        VirtualJointPoints[PositionIndex.lHand.Int()].Rotation = Quaternion.LookRotation(jointPoints[PositionIndex.lThumb2.Int()].Pos3D - jointPoints[PositionIndex.lMid1.Int()].Pos3D, lf) * lHand.InverseRotation;

        var rHand = jointPoints[PositionIndex.rHand.Int()];
        var rf = TriangleNormal(rHand.Pos3D, jointPoints[PositionIndex.rThumb2.Int()].Pos3D, jointPoints[PositionIndex.rMid1.Int()].Pos3D);
        VirtualJointPoints[PositionIndex.rHand.Int()].Rotation = Quaternion.LookRotation(jointPoints[PositionIndex.rThumb2.Int()].Pos3D - jointPoints[PositionIndex.rMid1.Int()].Pos3D, rf) * rHand.InverseRotation;    
    }
    void VirtualJointPointsSyntoUpperHalfPose()
    {
        VirtualJointPoints[PositionIndex.hip.Int()].Position = jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz);
        VirtualJointPoints[PositionIndex.hip.Int()].Rotation = Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation;

        for (int i=0 ; i<15 ; i++)
        {
            if (jointPoints[i].Parent != null)
            {
                var fv = jointPoints[i].Parent.Pos3D - jointPoints[i].Pos3D;
                jointPoints[i].Transform.rotation = Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, fv) * jointPoints[i].InverseRotation;
            }
            else if (jointPoints[i].Child != null)
            {
                jointPoints[i].Transform.rotation = Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, forward) * jointPoints[i].InverseRotation;
            }
        }
        for (int i=23 ; i<28 ; i++)
        {
            if (jointPoints[i].Parent != null)
            {
                var fv = jointPoints[i].Parent.Pos3D - jointPoints[i].Pos3D;
                jointPoints[i].Transform.rotation = Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, fv) * jointPoints[i].InverseRotation;
            }
            else if (jointPoints[i].Child != null)
            {
                jointPoints[i].Transform.rotation = Quaternion.LookRotation(jointPoints[i].Pos3D - jointPoints[i].Child.Pos3D, forward) * jointPoints[i].InverseRotation;
            }
        }
        for (int i=15;i<23;i++)
        {
            if (jointPoints[i].Parent != null)
            {
                VirtualJointPoints[i].Rotation = RotateEulerYaxis(jointPoints[i].Transform.rotation,VirtualGameobjectRotation.y);
            }
            else if (jointPoints[i].Child != null)
            {
                VirtualJointPoints[i].Rotation = RotateEulerYaxis(jointPoints[i].Transform.rotation,VirtualGameobjectRotation.y);
            }
        }

        // Head Rotation
        var gaze = jointPoints[PositionIndex.Nose.Int()].Pos3D - jointPoints[PositionIndex.head.Int()].Pos3D;
        var f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].Pos3D, jointPoints[PositionIndex.rEar.Int()].Pos3D, jointPoints[PositionIndex.lEar.Int()].Pos3D);
        var head = jointPoints[PositionIndex.head.Int()];
        VirtualJointPoints[PositionIndex.head.Int()].Rotation = Quaternion.LookRotation(gaze, f) * head.InverseRotation;
        
        // Wrist rotation (Test code)
        var lHand = jointPoints[PositionIndex.lHand.Int()];
        var lf = TriangleNormal(lHand.Pos3D, jointPoints[PositionIndex.lMid1.Int()].Pos3D, jointPoints[PositionIndex.lThumb2.Int()].Pos3D);
        VirtualJointPoints[PositionIndex.lHand.Int()].Rotation = Quaternion.LookRotation(jointPoints[PositionIndex.lThumb2.Int()].Pos3D - jointPoints[PositionIndex.lMid1.Int()].Pos3D, lf) * lHand.InverseRotation;

        var rHand = jointPoints[PositionIndex.rHand.Int()];
        var rf = TriangleNormal(rHand.Pos3D, jointPoints[PositionIndex.rThumb2.Int()].Pos3D, jointPoints[PositionIndex.rMid1.Int()].Pos3D);
        VirtualJointPoints[PositionIndex.rHand.Int()].Rotation = Quaternion.LookRotation(jointPoints[PositionIndex.rThumb2.Int()].Pos3D - jointPoints[PositionIndex.rMid1.Int()].Pos3D, rf) * rHand.InverseRotation;    
    }
    IEnumerator ToUpperHalfPoseUpdate()
    {
        currentTime = Time.time;
        TextMeshProUGUI.text = "Slerping to UpperbodyPose Updating Mode";
        PreviousState = CurrentState;
        IsAnimationUpdating = false;
        IsUpperHalfPoseUpdating = false;
        IsFullbodyPoseUpdating = false;
        IsSlerptoFullAnimation = false;
        IsSlerptoFullbodyPoseUpdate = false;

        VirtualGameobjectPosition = transform.position;
        VirtualGameobjectRotation = transform.eulerAngles;
        OffsetPosition = jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz) + new Vector3(0f,-0.87f,0f);
        OffsetRotation = new Vector3(0f,-90f+(Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation).eulerAngles.y,0f);

        SetVirtualJointPoints(CurrentState);

        IsSlerptoUpperHalfPoseUpdate = true;
        yield return new WaitForSeconds(2f);
        IsSlerptoUpperHalfPoseUpdate = false;
        CurrentState = (ModelState)1;
        IsUpperHalfPoseUpdating = true;
        TextMeshProUGUI.text = "UpperbodyPose Updating Mode";
    }
    IEnumerator ToFullbodyPoseUpdate()
    {
        currentTime = Time.time;
        TextMeshProUGUI.text = "Slerping to Fullbody Updating Mode";
        PreviousState = CurrentState;
        IsAnimationUpdating = false;
        IsFullbodyPoseUpdating = false;
        IsSlerptoUpperHalfPoseUpdate = false;
        IsUpperHalfPoseUpdating = false;
        IsSlerptoFullAnimation = false;
        
        VirtualGameobjectPosition = transform.position;
        VirtualGameobjectRotation = transform.eulerAngles;
        OffsetPosition = jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz) + new Vector3(0f,-0.87f,0f);
        OffsetRotation = new Vector3(0f,-90f+(Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation).eulerAngles.y,0f);

        SetVirtualJointPoints(CurrentState);

        IsSlerptoFullbodyPoseUpdate = true;
        yield return new WaitForSeconds(2f);
        IsSlerptoFullbodyPoseUpdate = false;
        CurrentState = (ModelState)2;
        IsFullbodyPoseUpdating = true;
        TextMeshProUGUI.text = "FullbodyPose Updating Mode";
    }
    IEnumerator ToFullAnimation()
    {
        currentTime = Time.time;
        TextMeshProUGUI.text = "Slerping to Animation Updating Mode";
        PreviousState = CurrentState;
        VirtualGameobjectPosition = transform.position;
        VirtualGameobjectRotation = transform.eulerAngles;
        OffsetPosition = jointPoints[PositionIndex.hip.Int()].Pos3D * 0.005f + new Vector3(initPosition.x, initPosition.y, initPosition.z + dz) + new Vector3(0f,-0.87f,0f);
        OffsetRotation = new Vector3(0f,-90f+(Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hip.Int()].InverseRotation).eulerAngles.y,0f);
        
        IsUpperHalfPoseUpdating = false;
        IsFullbodyPoseUpdating = false;
        IsSlerptoFullbodyPoseUpdate = false;
        IsSlerptoUpperHalfPoseUpdate = false;
        IsAnimationUpdating = false;
        
        SetVirtualJointPoints(CurrentState);
        
        IsSlerptoFullAnimation = true;
        yield return new WaitForSeconds(2f);
        IsSlerptoFullAnimation =false;
        CurrentState = (ModelState)0;
        IsAnimationUpdating = true;
        TextMeshProUGUI.text = "Animation Updating Mode";
    }
    public void SetState(ModelState M)
    {
        switch(M)
            {
                case ModelState.FullAnimation:
                    SetFullAnimation();
                    break;
                case ModelState.UpperHalfPoseUpdating:
                    SetUpperHalfPoseUpdate();
                    break;
                case ModelState.FullPoseUpdating:
                    SetFullbodyPoseUpdate();
                    break;
                default:
                    break;
            }
    }
    public ModelState GetModelState(int I)
    {
        switch(I)
        {
            case 0:
                return (ModelState)0;
            case 1:
                return (ModelState)1;
            case 2:
                return (ModelState)2;
            default:
                return (ModelState)0;
        }
    }
    void SetVirtualJointPoints(ModelState M)
    {
        switch(M)
        {
            case ModelState.FullAnimation:
                VirtualJointPointsSyntoAnimation();
                break;
            case ModelState.UpperHalfPoseUpdating:
                VirtualJointPointsSyntoUpperHalfPose();
                break;
            case ModelState.FullPoseUpdating:
                VirtualJointPointsSyntoFullbodyPose();
                break;
            default:
                break;        
        }
    }
    Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }
    private Quaternion RotateEulerYaxis(Quaternion Q,float Y)
    {
        return Quaternion.Euler(Q.eulerAngles.x,Q.eulerAngles.y-Y+180f,Q.eulerAngles.z);
    }
    private Quaternion GetInverse(JointPoint p1, JointPoint p2, Vector3 forward)
    {
        return Quaternion.Inverse(Quaternion.LookRotation(p1.Transform.position - p2.Transform.position, forward));
    }
    /// <summary>
    /// Add skelton from joint points
    /// </summary>
    /// <param name="s">position index</param>
    /// <param name="e">position index</param>
    private void AddSkeleton(PositionIndex s, PositionIndex e)
    {
        var sk = new Skeleton()
        {
            LineObject = new GameObject("Line"),
            start = jointPoints[s.Int()],
            end = jointPoints[e.Int()],
        };

        sk.Line = sk.LineObject.AddComponent<LineRenderer>();
        sk.Line.startWidth = 0.04f;
        sk.Line.endWidth = 0.01f;
        
        // define the number of vertex
        sk.Line.positionCount = 2;
        sk.Line.material = SkeletonMaterial;

        Skeletons.Add(sk);
    }
}
