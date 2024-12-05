using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkTestObject : NetworkBehaviour
{
    Vector3 pos;

    

    private void Start() {
        InputManager.Instance.onRMBPerformed += Move;
    }
    private void Move()
    {
        if(IsOwner)
        {
            pos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);  
        } 
    }
}
