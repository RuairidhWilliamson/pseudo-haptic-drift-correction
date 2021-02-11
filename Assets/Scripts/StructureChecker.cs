using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class StructureChecker : MonoBehaviour
{
    [SerializeField] private Vector3 size;
    public static StructureChecker Instance;
    private readonly Collider[] _blocks = new Collider[50];
    
    [SerializeField] private float maxDistance;
    [SerializeField] private float maxAngle;
    [SerializeField] public DesiredStructure desired;
    [SerializeField] private Transform displayBlock;
    [SerializeField] private Transform structureDisplay;

    private List<Transform> _displayBlocks = new List<Transform>();
    private static readonly int Active = Shader.PropertyToID("_Active");

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InvokeRepeating(nameof(Check), 2f, 1f);
    }

    private void Check()
    {
        bool validStructure = CheckStructure();
        if (validStructure)
        {
            ExperimentManager.Instance.EndTest();
        }
    }

#if UNITY_EDITOR
    [MenuItem("Structure/Save")]
    private static void SaveStructureButton()
    {
        Instance.SaveStructure();
    }
    
    private void SaveStructure()
    {
        if (desired is null)
        {
            Debug.LogError("No structure");
            return;
        }
        int count = Physics.OverlapBoxNonAlloc(transform.position, size / 2, _blocks);
        var newBlocks = new List<DesiredStructure.Block>();
        var addedHashCodes = new List<int>();
        for (int j = 0; j < count; j++)
        {
            Collider col = _blocks[j];
            if (col.gameObject.layer != LayerMask.NameToLayer("Pickupable")) continue;
            if (addedHashCodes.Contains(col.gameObject.GetInstanceID())) continue;
            addedHashCodes.Add(col.gameObject.GetInstanceID());
            newBlocks.Add(new DesiredStructure.Block()
            {
                color = int.Parse(col.name),
                position = col.transform.position,
                rotation = col.transform.rotation,
            });
        }

        desired.desiredStructure = newBlocks.ToArray();
        DisplayStructure();
        EditorUtility.SetDirty(desired);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif

    private bool CheckStructure()
    {
        if (desired is null) return false;
        int count = Physics.OverlapBoxNonAlloc(transform.position, size / 2, _blocks);
        for (int i = 0; i < count; i++)
        {
            Collider col = _blocks[i];
            MeshRenderer mr = col.GetComponent<MeshRenderer>();
            mr?.material.SetFloat(Active, 0f);
        }

        bool result = true;
        foreach (DesiredStructure.Block t in desired.desiredStructure)
        {
            if (!IsThereBlock(t, count))
            {
                result = false;
            }
        }
        return result;
    }

    private bool IsThereBlock(DesiredStructure.Block block, int count)
    {
        bool result = false;
        Collider closestCol = null;
        float smallestError = Mathf.Infinity;
        for (int j = 0; j < count; j++)
        {
            Collider col = _blocks[j];
            if (col.name != block.color.ToString()) continue;
            float distance = (block.position - col.transform.position).magnitude;
            float angle = Quaternion.Angle(block.rotation, col.transform.rotation);
            if (distance < maxDistance && angle < maxAngle)
            {
                float error = distance / maxDistance + angle / maxAngle;
                if (smallestError > error)
                {
                    closestCol = col;
                    smallestError = error;
                }
                result = true;
            }
        }

        if (result)
        {
            MeshRenderer mr = closestCol.GetComponent<MeshRenderer>();
            mr.material.SetFloat(Active, 1f);
        }

        return result;
    }

    public void DisplayStructure()
    {
        foreach (Transform t in _displayBlocks)
        {
            Destroy(t.gameObject);
        }
        _displayBlocks.Clear();
        
        foreach (DesiredStructure.Block t in desired.desiredStructure)
        {
            Transform b = Instantiate(displayBlock, structureDisplay);
            b.localPosition = t.position;
            b.localRotation = t.rotation;
            b.GetComponent<MeshRenderer>().material = BlockSpawner.Instance.spawnPoints[t.color].material;
            _displayBlocks.Add(b);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, size);
        Gizmos.color = Color.white;
    }
}
