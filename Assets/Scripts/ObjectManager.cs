using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private List<(Rigidbody, Vector3, Quaternion)> _objects;
    [SerializeField] private static ObjectManager _instance;

    private void Awake()
    {
        if (_instance)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        _objects = gameObject.GetComponentsInChildren<Rigidbody>().Select(rb => (rb, rb.position, rb.rotation)).ToList();
    }

    public static void ResetObjects()
    {
        _instance._objects.ForEach(el =>
        {
            (Rigidbody rb, Vector3 position, Quaternion rotation) = el;
            rb.position = position;
            rb.rotation = rotation;
            rb.velocity = Vector3.zero;
        });
    }
}
