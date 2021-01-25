using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerData : MonoBehaviour
{
    [SerializeField] public XRNode controllerNode = XRNode.RightHand;
    [SerializeField] public Transform visualPrefab;
    [SerializeField] public Transform realPrefab;
    [SerializeField] public bool showReal;
    [SerializeField] public float grabRadius = 0.1f;
    [SerializeField] public LayerMask layerMask;
    [SerializeField] public XRRig rig;
    [SerializeField] public bool recenterButton;
    [SerializeField] public bool resetObjectsButton;
    [SerializeField] public bool resetDriftButton;
}
