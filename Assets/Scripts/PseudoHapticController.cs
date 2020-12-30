using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PseudoHapticController : Controller
{
    
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;
    
    protected override void UpdateVirtual()
    {
        // Find amount moved since last frame
        Vector3 deltaPosition = RealPosition - _lastPosition;
        Quaternion deltaRotation = RealRotation * Quaternion.Inverse(_lastRotation);
        _lastPosition = RealPosition;
        _lastRotation = RealRotation;

        // Adjust amount moved since last frame by ControlDisplayRatio
        VirtualPosition += deltaPosition * ControlDisplayRatio;
        VirtualRotation = Quaternion.Slerp(Quaternion.identity, deltaRotation, ControlDisplayRatio) * VirtualRotation;
        // Apply the rotation to the grab position offset
        _grabPositionOffset = Quaternion.Slerp(Quaternion.identity, deltaRotation, ControlDisplayRatio) * _grabPositionOffset;
        VirtualRotation.Normalize();
    }
}
