using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;
using Random = System.Random;

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance;
    [SerializeField] private DesiredStructure[] structures;
    [SerializeField] private GameObject[] controllersObjects;
    [SerializeField] private TMP_Text textBox;
    [SerializeField] private TMP_Text debugTextBox;
    private List<Controller> _controllers;
    private bool _waitingUserContinue = false;
    private InputDevice _leftController;
    private InputDevice _rightController;
    public float[] masses;
    private Random _random = new Random();
    
    private Type[] _modes =
    {
        typeof(RealignOnRelease),
        typeof(RealignWhenOutOfView),
        typeof(SlowRealignContinuous),
        typeof(SlowRealignWhenMoving),
    };
    private int _testIndex;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        _rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        structures = structures.OrderBy(x => _random.Next()).ToArray();
        _modes = _modes.OrderBy(x => _random.Next()).ToArray();
        
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
        StartTest();
    }

    private void StartTest()
    {
        _waitingUserContinue = false;
        BlockSpawner.Instance.ClearBlocks();
        if (_testIndex >= Math.Min(structures.Length, _modes.Length))
        {
            EndExperiment();
            return;
        }
        masses = masses.OrderBy(x => _random.Next()).ToArray();
        StructureChecker.Instance.desired = structures[_testIndex];
        StructureChecker.Instance.DisplayStructure();
        Switch(_modes[_testIndex]);
        textBox.text = "Build the structure on the grey plate based on the diagram.";
        debugTextBox.text = $"{GetAcronym(_modes[_testIndex].Name)}\n{_modes[_testIndex].Name}\n{string.Join(",", masses)}";
    }

    private string GetAcronym(string input)
    {
        return Regex.Replace(input, "[^A-Z]", "");
    }
    
    private void Switch(Type clazz)
    {
        foreach (Controller controller in _controllers)
        {
            controller.enabled = clazz.IsInstanceOfType(controller);
        }
    }

    public void EndTest()
    {
        if (_waitingUserContinue) return;
        _testIndex++;
        textBox.text = "You have completed the structure press the primary button to continue.";
        _waitingUserContinue = true;
    }

    private static bool GetPrimaryButton(InputDevice inputDevice)
    {
        if (inputDevice.isValid &&
            inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primary))
        {
            return primary;
        }

        return false;
    }

    private void Update()
    {
        if (_waitingUserContinue && (GetPrimaryButton(_leftController) || GetPrimaryButton(_rightController)))
        {
            StartTest();
        }
    }

    private void EndExperiment()
    {
        textBox.text = "You have completed the experiment. Thank you for participating.";
    }
}
