using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Turret : NetworkBehaviour
{
    [SerializeField] private GameObject TurretPivot;
    [SerializeField] private int damage;
    [SerializeField] private float shootingInterval;
    private List<Unit> enemiesInRange = new List<Unit>();
    private Unit currentTarget;
    private ulong currentTargetId;
    private Vector3 targetPosition;
    private float shootingTimer;
    

    private void Update() {
        if(currentTarget != null)
        {   
            targetPosition = currentTarget.transform.position;
        }
        ShootCountdown();
        RotateTurret();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        other.TryGetComponent(out Unit unit);

        if(unit == null) return;

        if(unit.OwnerClientId != OwnerClientId) {
            enemiesInRange.Add(unit);
            FindClosestUnit();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        other.TryGetComponent(out Unit unit);

        if(unit == null) return;

        if(true) {
            enemiesInRange.Remove(unit);
            FindClosestUnit();
        }
    }

    private void RotateTurret()
    {
        Debug.Log("Trying to rotate turret");

        if(currentTarget == null) return;

        Debug.Log("Rotating turret");

        Vector3 direction = targetPosition - TurretPivot.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Adjust angle by 90 degrees
        TurretPivot.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void ShootCountdown()
    {
        shootingTimer += Time.deltaTime;

        if (shootingTimer >= shootingInterval)
        {
            shootingTimer = 0f;

            Shoot();
        }
    }

    private void Shoot()
    {
        if(currentTarget == null) return;

        if(IsHost)
        {
            DealDamage();
        }
    }

    private void DealDamage()
    {
        currentTarget.RemoveUnits(damage);
    }

    private void FindClosestUnit()
    {
        if(!IsHost) return;

        if (enemiesInRange.Count == 0)
        {
            currentTarget = null;
            SyncCurrentTargetClientRpc(0);
            return;
        }

        Unit closestUnit = enemiesInRange[0];
        float closestDistance = Vector2.Distance(transform.position, closestUnit.transform.position);

        foreach (Unit unit in enemiesInRange)
        {
            float distance = Vector2.Distance(transform.position, unit.transform.position);
            if (distance < closestDistance)
            {
                closestUnit = unit;
                closestDistance = distance;
            }
        }
    
        currentTarget = closestUnit;
        currentTargetId = currentTarget.NetworkObjectId;
        SyncCurrentTargetClientRpc(currentTargetId);
    }

    [ClientRpc]
    private void SyncCurrentTargetClientRpc(ulong targetId)
    {
        currentTargetId = targetId;
        if (currentTargetId == 0)
        {
            currentTarget = null;
        }
        else
        {
            currentTarget = NetworkManager.Singleton.SpawnManager.SpawnedObjects[currentTargetId].GetComponent<Unit>();
        }
    }

}
