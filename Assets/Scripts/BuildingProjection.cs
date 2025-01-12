using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingProjection : MonoBehaviour
{
    private void Update() {
        if(CanPlaceBuilding())
        {
            BuildingManager.Instance.AllowToBuild(true);
        } else
        {
            BuildingManager.Instance.AllowToBuild(false);
        }
    }

    bool CanPlaceBuilding()
{
    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    mouseWorldPosition.z = 0; // Убираем ось Z для 2D

    // Проверяем "SpawnRange"
    Collider2D[] colliders = Physics2D.OverlapCircleAll(mouseWorldPosition, 0.5f);
    bool isSpawnRange = false;
    foreach (var collider in colliders)
    {
        if (collider.CompareTag("SpawnRange"))
        {
            isSpawnRange = true;
        }
        if (collider.CompareTag("Building"))
        {
            Debug.Log("Касается 'Untagged'. Строить нельзя.");
            return false;
        }
    }
    if (isSpawnRange)
    {
        return true;
    }
    return false;
}
}

