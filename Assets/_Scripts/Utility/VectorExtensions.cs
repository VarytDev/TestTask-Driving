using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 DirectionFromTo(this Vector3 from, Vector3 to)
    {
        return (to - from).normalized;
    }
}
