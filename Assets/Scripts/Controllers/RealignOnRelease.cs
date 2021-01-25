using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealignOnRelease : PseudoHapticController
{
    protected override void Release()
    {
        base.Release();
        ResetDrift();
    }
}
