using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;

public class Controller : MonoBehaviour
{
    [SerializeField] private ControllerData _controllerData;
    private InputDevice _device;
    [SerializeField] private bool showReal;
    [SerializeField] private float grabRadius = 0.1f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private XRRig rig;
    [SerializeField] private bool recenterButton;
    [SerializeField] private bool resetObjectsButton;

    [SerializeField] private bool resetDriftButton;

    protected Vector3 RealPosition;
    protected Quaternion RealRotation;
    protected Vector3 VirtualPosition;
    protected Quaternion VirtualRotation = Quaternion.identity;
    
    protected float ControlDisplayRatio = 1f;

    private bool _grabDown;
    private bool _recenterDown;
    private bool _resetObjectsDown;
    private bool _resetDriftDown;
    protected Vector3 _grabPositionOffset;
    protected Quaternion _grabRotationOffset;

    private Transform _realRepresentation;
    private Transform _visual;
    private readonly Collider[] _colliders = new Collider[5];
    private Rigidbody _holding;

    private void Start()
    {
        _realRepresentation = Instantiate(_controllerData.realPrefab, transform);
        _visual = Instantiate(_controllerData.visualPrefab, transform);
        InputDevices.deviceConnected += DeviceConnected;
        UpdateDevice();
        // Reset drift once all setup so that we start with no drift
        Invoke(nameof(ResetDrift), 1f);
    }

    private void DeviceConnected(InputDevice device)
    {
        UpdateDevice();
    }

    private void UpdateDevice()
    {
        _device = InputDevices.GetDeviceAtXRNode(_controllerData.controllerNode);
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
        bool down = recenterButton && _device.isValid && _device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool value) && value;
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

    protected bool GetResetDrift()
    {
        bool down = resetDriftButton && _device.isValid && _device.TryGetFeatureValue(CommonUsages.primaryButton, out bool value) && value;
        if (!_resetDriftDown && down)
        {
            _resetDriftDown = true;
            return true;
        }
        if (_resetDriftDown && !down)
        {
            _resetDriftDown = false;
        }
        return false;
    }

    protected bool GetResetObjects()
    {
        bool down = resetObjectsButton && _device.isValid &&
                    _device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool value) && value;
        if (!_resetObjectsDown && down)
        {
            _resetObjectsDown = true;
            return true;
        }

        if (_resetObjectsDown && !down)
        {
            _resetObjectsDown = false;
        }
        return false;
    }
    
    protected void ProcessInput()
    {
        RealPosition = transform.TransformPoint(GetPosition());
        RealRotation = GetRotation();
        if (GetRecenter())
        {
            rig.cameraFloorOffsetObject.transform.position = -rig.cameraGameObject.transform.localPosition;
            rig.cameraFloorOffsetObject.transform.position -= Vector3.up * Vector3.Dot(rig.cameraFloorOffsetObject.transform.position, Vector3.up);
        }
        if (GetResetObjects())
        {
            ObjectManager.ResetObjects();
        }

        if (GetResetDrift())
        {
            ResetDrift();
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
        VirtualPosition = RealPosition;
        VirtualRotation = RealRotation;
    }

    protected virtual void UpdateRepresentation()
    {
        _visual.position = VirtualPosition;
        _visual.localRotation = VirtualRotation.normalized;
        if (showReal)
        {
            _realRepresentation.position = RealPosition;
            _realRepresentation.localRotation = RealRotation.normalized;
        }
        else
        {
            _realRepresentation.position = Vector3.one * -100f;
        }
    }

    protected virtual void UpdateHolding()
    {
        if (_holding)
        {
            _holding.isKinematic = true;
            _holding.position = VirtualPosition + _grabPositionOffset;
            _holding.rotation = VirtualRotation.normalized * _grabRotationOffset;
        }
    }

    private void Update()
    {
        ProcessInput();
        UpdateVirtual();
        UpdateRepresentation();
        UpdateHolding();
    }

    protected virtual void ResetDrift()
    {
        VirtualPosition = RealPosition;
        VirtualRotation = RealRotation;
    }

    protected virtual void Grab()
    {
        if (_holding) return;
        int size = Physics.OverlapSphereNonAlloc(VirtualPosition, grabRadius, _colliders, layerMask);
        // Find closest pickupable object within range
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
        // If we found an object to pickup, pick it up
        if (size > 0 && closestObject != null)
        {
            _holding = closestObject.attachedRigidbody;
            ControlDisplayRatio = 1f / _holding.mass;
            _holding.isKinematic = true;
            _grabPositionOffset = _holding.position - VirtualPosition;
            _grabRotationOffset = Quaternion.Inverse(VirtualRotation) * _holding.rotation;
        }
    }

    protected virtual void Release()
    {
        if (!_holding) return;
        _holding.isKinematic = false;
        _holding = null;
        ControlDisplayRatio = 1f;
    }
}
