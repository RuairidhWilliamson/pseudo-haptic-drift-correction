using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowRealignContinuous : PseudoHapticController
{
    
    private Vector3 _velocity;
    [SerializeField] private float smoothTime = 0.1f;

    protected override void UpdateVirtual()
    {
        base.UpdateVirtual();
        // VirtualPosition = Vector3.SmoothDamp(VirtualPosition, RealPosition, ref _velocity, smoothTime);
        VirtualPosition = Vector3.Lerp(VirtualPosition, RealPosition, smoothTime * Time.deltaTime);
        VirtualRotation = Quaternion.Slerp(VirtualRotation, RealRotation, smoothTime * Time.deltaTime);
    }
}
