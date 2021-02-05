using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class BlockSpawner : MonoBehaviour
{

    public static BlockSpawner Instance; 
    [Serializable]
    public class SpawnPoint
    {
        public Transform point;
        public Material material;
    }
    [SerializeField] public SpawnPoint[] spawnPoints;
    [SerializeField] private Rigidbody prefab;
    [SerializeField] private int blockLimit = 200;
    
    private readonly Collider[] _colliders = new Collider[1];
    private Bounds _bounds;
    private readonly List<GameObject> _spawnedBlocks = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Random rnd = new Random();
        
        InvokeRepeating(nameof(SlowUpdate), 1f, 1f);
    }

    private void SlowUpdate()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int count = Physics.OverlapBoxNonAlloc(spawnPoints[i].point.position, _bounds.extents, _colliders);
            if (count == 0)
            {
                SpawnBlock(i);
            }
        }
    }

    private void SpawnBlock(int index)
    {
        if (transform.childCount > blockLimit) return;
        Rigidbody block = Instantiate(prefab, transform);
        block.position = spawnPoints[index].point.position;
        block.GetComponent<MeshRenderer>().material = spawnPoints[index].material;
        block.mass = ExperimentManager.Instance.masses[index];
        block.name = index.ToString();
        BoxCollider[] prefabColliders = block.transform.GetComponentsInChildren<BoxCollider>();
        _bounds = prefabColliders[0].bounds;
        _spawnedBlocks.Add(block.gameObject);
        foreach (BoxCollider c in prefabColliders)
        {
            _bounds.Encapsulate(c.bounds);
        }
    }

    public void ClearBlocks()
    {
        foreach (GameObject block in _spawnedBlocks)
        {
            Destroy(block);
        }
        _spawnedBlocks.Clear();
    }
}
