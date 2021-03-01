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
    [SerializeField] private TMP_Text idTextBox;
    [SerializeField] private Logger logger;
    [SerializeField] private GameObject[] questionContainers;
    [SerializeField] private GameObject[] uiControllers;
    [SerializeField] private GameObject[] blockControllers;
    [SerializeField] private XRRig rig;
    [TextArea] [SerializeField] private string[] explanation;

    private List<Controller> _controllers;
    private bool _waitingUserContinue = false;
    private bool _userQuestions = false;
    private bool _experimentRunning = false;
    private int _explanationIndex = 0;
    private bool _started = false;
    private bool _primaryWasPressed = false;
    private float _lastPressTime = 0f;
    private int _questionIndex;
    private InputDevice _leftController;
    private InputDevice _rightController;
    public float[] masses;
    private Random _random = new Random();
    
    private Type[] _testingModes =
    {
        typeof(RealignOnRelease),
        typeof(RealignWhenOutOfView),
        typeof(SlowRealignContinuous),
        typeof(SlowRealignWhenMoving),
    };
    
    private readonly Type[] _allModes =
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
    private int _testIndex;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _experimentRunning = true;
        _leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        _rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        structures = structures.OrderBy(x => _random.Next()).ToArray();
        _testingModes = _testingModes.OrderBy(x => _random.Next()).ToArray();
        
        _controllers = new List<Controller>();
        foreach (GameObject obj in controllersObjects)
        {
            foreach (Type component in _allModes)
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
            go.SetActive(true);
        }

        foreach (var go in uiControllers)
        {
            go.SetActive(false);
        }
        Switch(typeof(NonPseudoHaptic));
        textBox.text = "Press A or X to start.";
        idTextBox.text = $"Participant ID:\n{logger.ParticipantID}";
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
        if (_testIndex >= Math.Min(structures.Length, _testingModes.Length))
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
        Switch(_testingModes[_testIndex]);
        textBox.text = "Build the structure on the grey plate based on the diagram.";
        logger.LogStartTest(_testIndex, _testingModes[_testIndex].Name, structures[_testIndex].name,string.Join(",", masses));
        debugTextBox.text = $"{GetAcronym(_testingModes[_testIndex].Name)}\n{_testingModes[_testIndex].Name}\n{string.Join(",", masses)}";
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

    private void ShowQuestion()
    {
        for (int i = 0; i < questionContainers.Length; i++)
        {
            questionContainers[i].SetActive(i == _questionIndex);
        }
    }

    public void HeaviestBlock(int x)
    {
        logger.LogHeaviestBlock(_testIndex, x);
        _questionIndex = 1;
        ShowQuestion();
    }

    public void DifficultOfTask(int x)
    {
        logger.LogDifficultyOfTask(_testIndex, x);
        _questionIndex = 2;
        ShowQuestion();
    }

    public void LightestBlock(int x)
    {
        logger.LogLightestBlock(_testIndex, x);
        _questionIndex = 3;
        ShowQuestion();
        LastQuestion();
    }

    public void RealisticOfMoving(int x)
    {
        logger.LogRealisticOfMoving(_testIndex, x);
        _questionIndex = 4;
        ShowQuestion();
    }

    private void LastQuestion()
    {
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
        
        #if UNITY_EDITOR
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            HeaviestBlock(0);
        }

        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            DifficultOfTask(0);
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            LightestBlock(0);
        }
        #endif
        if (GetSecondaryButton(_leftController) || GetSecondaryButton(_rightController))
        {
            Vector3 position = -rig.cameraGameObject.transform.localPosition;
            position -= Vector3.up * Vector3.Dot(position, Vector3.up);
            rig.cameraFloorOffsetObject.transform.position = position;
        }
        
        if (!_experimentRunning) return;
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            EndTest();
        }

        if (GetPrimaryButton(_leftController) || GetPrimaryButton(_rightController) || Keyboard.current.aKey.wasPressedThisFrame)
        {
            if (Time.time < _lastPressTime + 2f) return;
            if (_primaryWasPressed) return;
            _lastPressTime = Time.time;
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
                _questionIndex = 0;
                ShowQuestion();
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
        _experimentRunning = false;
        textBox.text = $"You have completed all the tasks. Please return to and complete the online form.";
        logger.LogEnd();
        logger.UploadLogs();
    }
}
