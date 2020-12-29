using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Serialization;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;

public class Controller : MonoBehaviour
{
    
    [SerializeField] private XRNode controllerNode = XRNode.RightHand;
    private InputDevice _device;
    
    [SerializeField] private Transform visualPrefab;
    [SerializeField] private float grabRadius = 0.05f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private XRRig rig;
    [SerializeField] private bool recenterButton;

    protected Vector3 RealPosition;
    protected Quaternion RealRotation;
    protected Vector3 VirtualPosition;
    protected Quaternion VirtualRotation;
    protected float ControlDisplayRatio = 1f;

    private bool _grabDown;
    private bool _recenterDown;

    private Transform _visual;
    private readonly Collider[] _colliders = new Collider[5];
    private Rigidbody _holding;

    private void Start()
    {
        _visual = Instantiate(visualPrefab, transform);
        InputDevices.deviceConnected += DeviceConnected;
        UpdateDevice();
    }

    private void DeviceConnected(InputDevice device)
    {
        UpdateDevice();
    }

    private void UpdateDevice()
    {
        _device = InputDevices.GetDeviceAtXRNode(controllerNode);
    }

    protected Vector3 GetPosition()
    {
        if (_device.isValid && _device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
            return position;
        return Vector3.zero;
    }

    protected Quaternion GetRotation()
    {
        if (_device.isValid && _device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
        {
            return rotation;
        }
        return Quaternion.identity;
    }

    protected bool GetGrab()
    {
        if (!_grabDown && _device.isValid && _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool value) &&
            value)
        {
            _grabDown = true;
            return true;
        }

        return false;
    }

    protected bool GetRelease()
    {
        if (_grabDown && !(_device.isValid && _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool value) &&
            value))
        {
            _grabDown = false;
            return true;
        }

        return false;
    }

    protected bool GetRecenter()
    {
        bool down = recenterButton && _device.isValid && _device.TryGetFeatureValue(CommonUsages.primaryButton, out bool value) && value;
        if (!_recenterDown && down)
        {
            _recenterDown = true;
            return true;
        }
        if (_recenterDown && !down)
        {
            _recenterDown = false;
        }
        return false;
    }
    
    protected void ProcessInput()
    {
        RealPosition = GetPosition();
        RealRotation = GetRotation();
        if (GetRecenter())
        {
            rig.cameraFloorOffsetObject.transform.position = -rig.cameraGameObject.transform.localPosition;
            rig.cameraFloorOffsetObject.transform.position -= Vector3.up * Vector3.Dot(rig.cameraFloorOffsetObject.transform.position, Vector3.up);
        }
        if (GetGrab())
        {
            Grab();
        }
        if (GetRelease())
        {
            Release();
        }
    }

    protected virtual void UpdateVirtual()
    {
        VirtualPosition = transform.TransformPoint(RealPosition);
        VirtualRotation = RealRotation;
        _visual.position = VirtualPosition;
        _visual.localRotation = VirtualRotation;
    }

    protected virtual void UpdateHolding()
    {
        if (_holding)
        {
            _holding.isKinematic = true;
            _holding.position = VirtualPosition;
            _holding.rotation = VirtualRotation;
        }
    }

    private void Update()
    {
        ProcessInput();
        UpdateVirtual();
        UpdateHolding();
    }

    private void Grab()
    {
        if (_holding) return;
        int size = Physics.OverlapSphereNonAlloc(VirtualPosition, grabRadius, _colliders, layerMask);
        float smallestDistance = Mathf.Infinity;
        Collider closestObject = null;
        for (int i = 0; i < size; i++)
        {
            float distance = (_colliders[i].ClosestPoint(VirtualPosition) - VirtualPosition).sqrMagnitude;
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestObject = _colliders[i];
            }
        }
        if (size > 0 && closestObject != null)
        {
            _holding = closestObject.attachedRigidbody;
            ControlDisplayRatio = 1f / _holding.mass;
            _holding.isKinematic = true;
        }
    }

    private void Release()
    {
        if (!_holding) return;
        _holding.isKinematic = false;
        _holding = null;
    }
}
