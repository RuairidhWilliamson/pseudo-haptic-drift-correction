using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PickupManager : MonoBehaviour
{
    
    [SerializeField] private float maxGrabDistance;
    [SerializeField] private float controlDisplayRatio = 1f;
    [SerializeField] private Transform virtualRepresentation;
    [SerializeField] private Transform realRepresentation;
    [SerializeField] private float holdingDistance = 10f;
    [SerializeField] private bool mouseControlRotation;
    [SerializeField] private bool offCenterHoldPoint;
    [SerializeField] private bool rotateAroundHoldPoint;

    private Camera _camera;
    private Rigidbody _holding;
    private Vector3 _lastHoldPoint;
    private Vector3 _virtualHoldPoint;
    private Vector3 _lastMousePosition;
    private Vector3 _mouseTranslation;
    private Quaternion _mouseRotation = Quaternion.identity;
    private Quaternion _virtualRotation = Quaternion.identity;
    private Quaternion _lastRotation = Quaternion.identity;
    private LayerMask _layerMask;
    private Vector3 _translationOffset;
    private Quaternion _rotationOffset;


    private void Start()
    {
        _camera = Camera.main;
        _layerMask = LayerMask.GetMask("Pickupable");
    }

    protected virtual bool GetGrab()
    {
        return Input.GetMouseButtonDown(0);
    }

    protected virtual bool GetRelease()
    {
        return Input.GetMouseButtonUp(0);
    }

    protected virtual bool GetToggleMode()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    protected virtual Vector3 GetRealHoldPoint()
    {
        Ray mouseRay = _camera.ScreenPointToRay(_mouseTranslation);
        return mouseRay.origin + mouseRay.direction * holdingDistance;
    }

    protected virtual Quaternion GetRealRotation()
    {
        return _mouseRotation;
    }

    private void Update()
    {

        if (mouseControlRotation)
        {
            // Calculate the rotation from the mouse
            _mouseRotation *= Quaternion.Euler(Input.mousePosition - _lastMousePosition);
        }
        else
        {
            // Calculate the position from the mouse
            _mouseTranslation += Input.mousePosition - _lastMousePosition;
        }
        _lastMousePosition = Input.mousePosition;

        if (GetGrab())
        {
            Vector3 cameraPosition = _camera.transform.position;
            Ray ray = new Ray(cameraPosition, _virtualHoldPoint - cameraPosition);
            Debug.DrawRay(ray.origin, ray.direction * maxGrabDistance);
            if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, _layerMask))
            {
                _holding = hit.rigidbody;
                _holding.isKinematic = true;
                controlDisplayRatio = 1f / _holding.mass;
                _translationOffset = _holding.position - _virtualHoldPoint;
                _rotationOffset = Quaternion.Inverse(_virtualRotation) * _holding.rotation ;
            }
        }

        if (GetRelease())
        {
            if (_holding)
            {
                _holding.isKinematic = false;
                controlDisplayRatio = 1f;
                _holding = null;
            }
        }

        if (GetToggleMode())
        {
            mouseControlRotation = !mouseControlRotation;
        }
    }

    private void FixedUpdate()
    {
        // Update translation
        Vector3 realHoldPoint = GetRealHoldPoint();
        Vector3 delta = realHoldPoint - _lastHoldPoint;
        _virtualHoldPoint += controlDisplayRatio * delta;
        _lastHoldPoint = realHoldPoint;
        
        // Update rotation
        Quaternion realRotation = GetRealRotation();
        Quaternion rotDelta = realRotation * Quaternion.Inverse(_lastRotation);
        if (rotateAroundHoldPoint)
        {
            _translationOffset = Quaternion.Slerp(Quaternion.identity,rotDelta, controlDisplayRatio) * _translationOffset;
        }
        _virtualRotation = Quaternion.Slerp(Quaternion.identity,rotDelta, controlDisplayRatio) * _virtualRotation;
        _lastRotation = realRotation;
        
        
        // Update visual cursors
        virtualRepresentation.position = _virtualHoldPoint;
        virtualRepresentation.rotation = _virtualRotation;
        realRepresentation.position = realHoldPoint;
        realRepresentation.rotation = realRotation;
        
        if (_holding)
        {
            _holding.position = _virtualHoldPoint + _translationOffset;
            _holding.rotation = _virtualRotation * _rotationOffset;
        }
    }
}
