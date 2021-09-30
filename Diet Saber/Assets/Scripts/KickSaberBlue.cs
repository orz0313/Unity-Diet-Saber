using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class KickSaberBlue : MonoBehaviour
{
    
    [SerializeField] GameObject _tip = null;
    [SerializeField] GameObject _base = null;
    [SerializeField] float _forceAppliedToCut = 3f;
    [SerializeField] GameObject SliceParticleEffect;
    Vector3 _previousTipPosition;
    Vector3 _previousBasePosition;
    Vector3 _triggerEnterTipPosition;
    Vector3 _triggerEnterBasePosition;
    Vector3 _triggerExitTipPosition;
    // bool IsSccesstoSlice=false;
    [SerializeField] PlayerInfo playerInfo;

    void Start()
    {
        _previousTipPosition = _tip.transform.position;
        _previousBasePosition = _base.transform.position;
    }    

    void LateUpdate()
    {
        _previousTipPosition = _tip.transform.position;
        _previousBasePosition = _base.transform.position;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 16)
        {            
            _triggerEnterTipPosition = _tip.transform.position;
            _triggerEnterBasePosition = _base.transform.position;
        }
        else return;
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 16)
        {                
            _triggerExitTipPosition = _tip.transform.position;

            Vector3 side1 = _triggerExitTipPosition - _triggerEnterTipPosition;
            Vector3 side2 = _triggerExitTipPosition - _triggerEnterBasePosition;

            Vector3 normal = Vector3.Cross(side1, side2).normalized;

            Vector3 transformedNormal = ((Vector3)(other.gameObject.transform.localToWorldMatrix.transpose * normal)).normalized;

            Vector3 transformedStartingPoint = other.gameObject.transform.InverseTransformPoint(_triggerEnterTipPosition);

            Plane plane = new Plane();

            plane.SetNormalAndPosition(transformedNormal,transformedStartingPoint);

            var direction = Vector3.Dot(Vector3.up, transformedNormal);

           if (direction < 0)
            {
                plane = plane.flipped;
            }
            GameObject[] slices = Slicer.Slice(plane, other.gameObject);
                
            StartCoroutine(Slicer.DestroySlicedCube(slices[0]));
            StartCoroutine(Slicer.DestroySlicedCube(slices[1]));

            Instantiate(SliceParticleEffect,other.gameObject.transform.position,other.gameObject.transform.rotation);
            
            Destroy(other.gameObject);
            playerInfo.PlayerHPAdd(1);

            Rigidbody rigidbody = slices[1].GetComponent<Rigidbody>();
            Vector3 newNormal = transformedNormal + Vector3.up * _forceAppliedToCut;
            rigidbody.AddForce(newNormal, ForceMode.Impulse);
        }
    }

    // void OnTriggerEnter(Collider other) 
    // {
    //     if(other.gameObject.layer == 16)
    //     {
    //         playerInfo.PlayerHPAdd(1);
    //         Destroy(other.gameObject);
    //     }
    // }
}
