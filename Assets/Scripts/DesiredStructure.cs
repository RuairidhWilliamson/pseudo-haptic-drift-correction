using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Desired Structure")]
public class DesiredStructure : ScriptableObject
{
    [Serializable]
    public class Block
    {
        public Vector3 position;
        public Quaternion rotation;
        public int color;
    }
    public Block[] desiredStructure;
    public Texture2D blueprint;
}
