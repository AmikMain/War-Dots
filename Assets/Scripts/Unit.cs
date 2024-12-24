using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Unit : NetworkBehaviour
{
    [SerializeField] TMP_Text unitCountText;
    [SerializeField] int normalSpeed;
    [SerializeField] int slowSpeed;
    private float speedMultiplyer = 0.1f;
    private int unitCount;
    int currentSpeed;
    Vector3 destination;
    private string spawnCastleUniqueId;

    private void Update() {
        unitCountText.text = unitCount.ToString();

        if(destination == null) return;
        transform.position = Vector3.MoveTowards(transform.position, destination, currentSpeed * Time.deltaTime * speedMultiplyer);
    }

    public void SendUnitDataToClients()
    {
        SendUnitDataClientRpc(GetComponent<SpriteRenderer>().color, unitCount, spawnCastleUniqueId);
    }

    [ClientRpc]
    private void SendUnitDataClientRpc(Color unitColor, int newUnitCount, string spawnId)
    {
        GetComponent<SpriteRenderer>().color = unitColor;
        unitCount = newUnitCount;
        spawnCastleUniqueId = spawnId;
    }

    public void SelfDestroy()
    {
        if(IsHost)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    public void RemoveUnits(int units)
    {
        unitCount -= units;
        if(unitCount <= 0)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
        else
        {
            SendUnitDataClientRpc(GetComponent<SpriteRenderer>().color, unitCount, spawnCastleUniqueId);
        }
    }

    public void SetSpawnCastleUniqueId(string id)
    {
        spawnCastleUniqueId = id;
    }

    public string GetSpawnCastleUniqueId()
    {
        return spawnCastleUniqueId;
    }

    public int GetUnitCount()
    {
        return unitCount;
    }

    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
    }

    public void SetUnitsCount(int count)
    {
        unitCount = count;
    }

    public void SetColor(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
    }

    public void SlowDown()
    {
        currentSpeed = slowSpeed;
    }

    public void ReturnToNormalSpeed()
    {
        currentSpeed = normalSpeed;
    }
}
