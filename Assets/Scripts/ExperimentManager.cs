using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
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
    [SerializeField] private Logger logger;
    [SerializeField] private GameObject taskDifficulty;
    [SerializeField] private GameObject heavyObject;
    [SerializeField] private GameObject lightObject;
    [SerializeField] private GameObject[] uiControllers;
    [SerializeField] private GameObject[] blockControllers;
    [SerializeField] private XRRig rig;
    [TextArea] [SerializeField] private string[] explanation;

    private List<Controller> _controllers;
    private bool _waitingUserContinue = false;
    private bool _userQuestions = false;
    private int _explanationIndex = 0;
    private bool _started = false;
    private bool _primaryWasPressed = false;
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
        foreach (var go in blockControllers)
        {
            go.SetActive(false);
        }

        foreach (var go in uiControllers)
        {
            go.SetActive(true);
        }

        textBox.text = "Press A or X to start.";
    }

    private void StartExperiment()
    {
        logger.LogStart();
        _started = true;
        StartTest();
    }

    private void StartTest()
    {
        _waitingUserContinue = false;
        _userQuestions = false;
        BlockSpawner.Instance.ClearBlocks();
        if (_testIndex >= Math.Min(structures.Length, _modes.Length))
        {
            EndExperiment();
            return;
        }
        foreach (var go in blockControllers)
        {
            go.SetActive(true);
        }

        foreach (var go in uiControllers)
        {
            go.SetActive(false);
        }
        masses = masses.OrderBy(x => _random.Next()).ToArray();
        StructureChecker.Instance.desired = structures[_testIndex];
        StructureChecker.Instance.DisplayStructure();
        Switch(_modes[_testIndex]);
        textBox.text = "Build the structure on the grey plate based on the diagram.";
        logger.LogStartTest(_testIndex, _modes[_testIndex].Name, structures[_testIndex].name,string.Join(",", masses));
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
        if (_waitingUserContinue || _userQuestions) return;
        logger.LogEndTest(_testIndex);
        textBox.text = "You have completed the structure press A or X to continue.";
        _waitingUserContinue = true;
    }

    public void DifficultOfTask(int x)
    {
        
        logger.LogDifficultyOfTask(_testIndex, x);
        heavyObject.SetActive(true);
        taskDifficulty.SetActive(false);
    }

    public void HeaviestBlock(int x)
    {
        logger.LogHeaviestBlock(_testIndex, x);
        heavyObject.SetActive(false);
        lightObject.SetActive(true);
    }

    public void LightestBlock(int x)
    {
        logger.LogLightestBlock(_testIndex, x);
        lightObject.SetActive(false);
        _testIndex++;
        StartTest();
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

    private static bool GetSecondaryButton(InputDevice inputDevice)
    {
        if (inputDevice.isValid &&
            inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondary))
        {
            return secondary;
        }

        return false;
    }

    private void Update()
    {
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            EndTest();
        }

        if (GetSecondaryButton(_leftController) || GetSecondaryButton(_rightController))
        {
            Vector3 position = -rig.cameraGameObject.transform.localPosition;
            position -= Vector3.up * Vector3.Dot(position, Vector3.up);
            rig.cameraFloorOffsetObject.transform.position = position;
        }

        if (GetPrimaryButton(_leftController) || GetPrimaryButton(_rightController))
        {
            if (_primaryWasPressed) return;
            if (!_started)
            {
                if (_explanationIndex < explanation.Length)
                {
                    textBox.text = explanation[_explanationIndex];
                    _explanationIndex++;
                }
                else
                {
                    StartExperiment();
                }
            }

            if (_waitingUserContinue)
            {
                _waitingUserContinue = false;
                _userQuestions = true;
                taskDifficulty.SetActive(true);
                foreach (var go in blockControllers)
                {
                    go.SetActive(false);
                }

                foreach (var go in uiControllers)
                {
                    go.SetActive(true);
                }
            }

            _primaryWasPressed = true;
        }
        else
        {
            _primaryWasPressed = false;
        }
    }


    private void EndExperiment()
    {
        textBox.text = "You have completed the experiment. Thank you for participating.";
        logger.LogEnd();
        logger.UploadLogs();
    }
}
