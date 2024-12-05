using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Castle : NetworkBehaviour
{
    [Header("Castle Components")]
    [SerializeField] private SpriteRenderer buildingRenderer;
    [SerializeField] private TMP_Text[] unitCountTexts;
    [SerializeField] private GameObject[] unitCountImages;

    private Dictionary<ulong, int> unitCounts = new Dictionary<ulong, int>();
    private string castleUniqueId;
    private Camera mainCamera;

    #region Unity Lifecycle Methods

    private void Start() {
        unitCountImages[0].GetComponent<MainBuildingUnitCountDisplay>().mouseOver += SetSecondaryUnitDisplayersState;
        SetCastleUniqueId();
        UpdateLocalUnitCountUI();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(!IsHost) return;
        
        Unit unit = other.GetComponent<Unit>();

        if(unit == null) return;
        
        if(unit.GetSpawnCastleUniqueId() == castleUniqueId) return;

        AddUnits(unit.GetComponent<NetworkObject>().OwnerClientId, unit.GetUnitCount());
        other.GetComponent<Unit>().SelfDestroy();
    }

    #endregion

    #region Castle Initialization

    private void SetCastleUniqueId()
    {
        castleUniqueId = Guid.NewGuid().ToString();
        SendUniqueIdToClientClientRpc(castleUniqueId);
    }

    [ClientRpc]
    private void SendUniqueIdToClientClientRpc(string id)
    {
        castleUniqueId = id;
    }

    public string GetCastleUniqueId()
    {
        return castleUniqueId;
    }
    
    #endregion

    #region Unit Management
    public int GetUnitCount(ulong clientId)
    {
        return unitCounts.ContainsKey(clientId) ? unitCounts[clientId] : 0;
    }

    public void AddUnits(ulong clientId, int count)
    {
        if (unitCounts.ContainsKey(clientId))
        {
            unitCounts[clientId] += count;
        }
        else
        {
            unitCounts[clientId] = count;
        }

        SendUnitCountsToClients();
    }

    public void RemoveUnits(ulong clientId, int count)
    {
        if (unitCounts.ContainsKey(clientId))
        {
            unitCounts[clientId] -= count;

            /* Deleting player if he doesnt have units there
            if (unitCounts[clientId] <= 0)
            {
                unitCounts.Remove(clientId);
            }*/
        }

        Debug.Log("Units Removed. Sending counts to clients");
        SendUnitCountsToClients();
    }

    private List<KeyValuePair<ulong, int>> GetTopThreePlayers()
    {
        // Проверяем, что словарь не пуст
        if (unitCounts == null || unitCounts.Count == 0)
        {
            Debug.LogWarning("unitCounts пуст или равен null");
            return new List<KeyValuePair<ulong, int>>();
        }

        // Сортируем словарь по значению (количество юнитов) в порядке убывания и берем первые три записи
        var topPlayers = unitCounts
            .OrderByDescending(pair => pair.Value) // Сортируем по убыванию значения
            .Take(3)                               // Берем первые 3 записи
            .ToList();                             // Преобразуем в список

        return topPlayers;
    }

    #endregion

    #region Networking

    [ClientRpc]
    private void UpdateUnitCountsClientRpc(ulong[] keys, int[] values)
    {
        unitCounts = Util.ConvertArraysToDictionary(keys, values);
        
        Debug.Log("Client RPC received. Updating UI");
        UpdateLocalUnitCountUI();   
    }

    public void SendUnitCountsToClients()
    {
        if (unitCounts == null || unitCounts.Count == 0)
        {
            Debug.LogWarning("unitCounts пуст, ничего не отправляется клиентам.");
            return;
        }

        ulong[] keys;
        int[] values;
        Util.ConvertDictionaryToArrays(unitCounts, out keys, out values);

        Debug.Log("Sent units to clients. Sending clientRpc");
        UpdateUnitCountsClientRpc(keys, values);
    }

    #endregion

    #region UI Management

    private void UpdateLocalUnitCountUI()
    {
        
        var topPlayers = GetTopThreePlayers();
        Debug.Log($"Trying to update UI {topPlayers.Count}");

        for (int i = 0; i < topPlayers.Count; i++)
        {
            ulong clientId = topPlayers[i].Key;
            int unitCount = topPlayers[i].Value;

            // Обновляем интерфейс для игрока
            unitCountTexts[i].text = unitCount.ToString();
            Debug.Log($"Обновляем {i} игрока на число {topPlayers[i].Value}");

            // Обновляем цвет
            unitCountImages[i].GetComponent<Image>().color = GameManager.Instance.playerColors[clientId];
        }

        // На всякий случай ставим цвет владельца цветом верхнего кружка
        unitCountImages[0].GetComponent<Image>().color = GameManager.Instance.playerColors[OwnerClientId];
    }

    public void SetSecondaryUnitDisplayersState(bool state)
    {
        unitCountImages[1].SetActive(state);
        unitCountImages[2].SetActive(state);
    }

    #endregion

    #region Visuals and Interaction

    public void SetCastleColor(Color newColor)
    {
        buildingRenderer.material.color = newColor;
        UpdateCastleColorClientRpc(newColor);
    }

    [ClientRpc]
    public void UpdateCastleColorClientRpc(Color newColor)
    {
        buildingRenderer.material.color = newColor;
    }

    public void SetCastlePosition(Vector3 position)
    {
        transform.position = position;
        UpdateCastlePositionClientRpc(position);
    }

    [ClientRpc]
    private void UpdateCastlePositionClientRpc(Vector3 position)
    {
        transform.position = position;
    }

    public void SetCastleVisualSelection(bool state)
    {
        buildingRenderer.material.color = state 
            ? new Color(buildingRenderer.material.color.r, buildingRenderer.material.color.g, buildingRenderer.material.color.b, 0.5f) 
            : new Color(buildingRenderer.material.color.r, buildingRenderer.material.color.g, buildingRenderer.material.color.b, 1f);
    }

    #endregion
}