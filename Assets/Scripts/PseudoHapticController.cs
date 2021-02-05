using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PseudoHapticController : Controller
{
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;
    protected Vector3 deltaPosition;
    protected Quaternion deltaRotation;
    
    protected override void UpdateVirtual()
    {
        // Find amount moved since last frame
        deltaPosition = RealPosition - _lastPosition;
        deltaRotation = RealRotation * Quaternion.Inverse(_lastRotation);
        _lastPosition = RealPosition;
        _lastRotation = RealRotation;

        // Adjust amount moved since last frame by ControlDisplayRatio
        VirtualPosition += deltaPosition * ControlDisplayRatio;
        VirtualRotation = Quaternion.Slerp(Quaternion.identity, deltaRotation, ControlDisplayRatio) * VirtualRotation;
        // Apply the rotation to the grab position offset
        _grabPositionOffset = Quaternion.Slerp(Quaternion.identity, deltaRotation, ControlDisplayRatio) * _grabPositionOffset;
        VirtualRotation.Normalize();
    }

    protected override void Release()
    {
        if (_holding is null) return;
        _holding.velocity = deltaPosition / Time.deltaTime;
        // _holding.angularVelocity = deltaRotation.eulerAngles / Time.deltaTime;
        base.Release();
    }
}
