using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBuildingUnitCountDisplay : MonoBehaviour
{
    public event Action<bool> mouseOver;
    private void OnMouseOver() {
        mouseOver?.Invoke(true);
    }

    void OnMouseExit()
    {
        mouseOver?.Invoke(false);
    }
}
