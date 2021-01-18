using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
[CreateAssetMenu(fileName = "RightController", menuName = "ScriptableObjects/ControllerData", order = 1)]
public class ControllerData : ScriptableObject
{
    [SerializeField] public XRNode controllerNode = XRNode.RightHand;
    [SerializeField] public Transform visualPrefab;
    [SerializeField] public Transform realPrefab;
}
