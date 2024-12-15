using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;


    public event Action onShiftPerformed;
    public event Action onShiftCanceled;
    public event Action onCtrlPerformed;
    public event Action onCtrlCanceled;
    public event Action onAltPerformed;
    public event Action onAltCanceled;
    public event Action onRMBPerformed;
    public event Action onRMBCanceled;
    public event Action onLMBPerformed;
    public event Action onLMBCanceled;
    
    public Vector2 movementVector = new Vector2(0,0);

    private void Awake() {

        DontDestroyOnLoad(gameObject);

        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }

    }

    public void Movement(InputAction.CallbackContext context)
    {
        movementVector = context.ReadValue<Vector2>();
    }

    public void LMBClick(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            onLMBPerformed?.Invoke();
        }

        if(context.canceled)
        {
            onLMBCanceled?.Invoke();
        }
    }

    public void RMBClick(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            onRMBPerformed?.Invoke();
        }

        if(context.canceled)
        {
            onRMBCanceled?.Invoke();
        }
    }

    public void ShiftClick(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            onShiftPerformed?.Invoke();
        }

        if(context.canceled)
        {
            onShiftCanceled?.Invoke();
        }
    }

    public void CtrlClick(InputAction.CallbackContext context)
    {
        Debug.Log("CtrlClick");
        if(context.performed)
        {
            onCtrlPerformed?.Invoke();
        }

        if(context.canceled)
        {
            onCtrlCanceled?.Invoke();
        }
    }

    public void AltClick(InputAction.CallbackContext context)
    {
        Debug.Log("AltClick");
        if(context.performed)
        {
            onAltPerformed?.Invoke();
        }

        if(context.canceled)
        {
            onAltCanceled?.Invoke();
        }
    }

}
