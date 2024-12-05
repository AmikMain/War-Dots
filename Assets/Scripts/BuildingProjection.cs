using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingProjection : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other) {
        Debug.Log("staying in the trigga");
        BuildingManager.Instance.AllowToBuild(false);
    }

    private void OnTriggerExit2D(Collider2D other) {
        Debug.Log("allowing to build!");
        BuildingManager.Instance.AllowToBuild(true);
    }
}
