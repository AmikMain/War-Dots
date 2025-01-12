using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Silo : MonoBehaviour
{  
    public void LaunchMissile(Vector3 coordinates)
    {
        Debug.Log($"Missile launched towards {coordinates.ToString()}");
    }
}
