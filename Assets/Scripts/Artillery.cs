using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Artillery : NetworkBehaviour
{
    private List<Unit> enemiesInRange = new List<Unit>();
    private Unit currentTarget;

    private void OnTriggerEnter2D(Collider2D other) {
        if(!IsHost) return;

        other.TryGetComponent(out Unit unit);

        if(unit == null) return;

        if(!unit.IsOwner) {
            enemiesInRange.Add(unit);
            FindClosestUnit();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(!IsHost) return;

        other.TryGetComponent(out Unit unit);

        if(unit == null) return;

        if(true) {
            enemiesInRange.Remove(unit);
            unit.ReturnToNormalSpeed();
            FindClosestUnit();
        }
    }

    private void FindClosestUnit()
    {
        if (enemiesInRange.Count == 0)
        {
            currentTarget = null;
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

        if(currentTarget != null)
        {
            SlowUnitDown();
        }
    }

    private void SlowUnitDown()
    {
        currentTarget.SlowDown();
    }

    private void ReturnUnitToNormalSpeed()
    {
        currentTarget.GetComponent<Unit>().ReturnToNormalSpeed();
    }
}
