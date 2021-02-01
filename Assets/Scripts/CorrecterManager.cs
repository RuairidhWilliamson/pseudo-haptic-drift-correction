using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class CorrecterManager : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    private readonly Type[] _modes =
    {
        typeof(NonPseudoHaptic), 
        typeof(RealignOnButton),
        typeof(RealignOnRelease),
        typeof(RealignWhenOutOfView),
        typeof(SlowRealignOnRelease), 
        typeof(SlowRealignContinuous),
        typeof(SlowRealignWhenMoving),
        typeof(SlowRealignWhenOutOfView),
        typeof(SlowRealignWhenMovingAndRealignWhenOutOfView)
    };
    private List<Controller> _controllers;
    [SerializeField] private GameObject[] controllersObjects;
    private int _mode;
    private InputDevice _leftController;
    private InputDevice _rightController;
    private float _lastInput;

    private void Start()
    {
        _leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        _rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        _controllers = new List<Controller>();
        foreach (GameObject obj in controllersObjects)
        {
            foreach (Type component in _modes)
            {
                Controller c = (Controller) obj.GetComponent(component);
                if (c != null)
                {
                    _controllers.Add(c);
                }
            }
        }
        Switch(_modes[_mode]);
    }

    private void Switch(Type clazz)
    {
        foreach (Controller controller in _controllers)
        {
            controller.enabled = clazz.IsInstanceOfType(controller);
            if (controller.enabled)
            {
                text.text = controller.Name;
            }
        }
    }

    private static float GetThumbStickInput(InputDevice inputDevice)
    {
        if (inputDevice.isValid &&
            inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbStick))
        {
            return thumbStick.x;
        }

        return 0f;
    }
    
    private void Update()
    {
        float input = GetThumbStickInput(_leftController) + GetThumbStickInput(_rightController);
        input = Mathf.Clamp(Mathf.Round(input), -1.0f, 1.0f);
        if (input > 0 && input - _lastInput > 0)
        {
            _mode++;
            _mode %= _modes.Length;
            Switch(_modes[_mode]);
        } else if (input < 0 && input - _lastInput < 0)
        {
            _mode--;
            _mode += _modes.Length;
            _mode %= _modes.Length;
            Switch(_modes[_mode]);
        }

        _lastInput = input;
    }
}
