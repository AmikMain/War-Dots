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

    [Header("Unit Production")]
    [SerializeField] private float unitProductionInterval;
    [SerializeField] private int unitsPerInterval; 
    [SerializeField] private bool producesUnits;

    private Dictionary<ulong, int> unitCounts = new Dictionary<ulong, int>();
    private string castleUniqueId;
    private Camera mainCamera;
    private float unitProductionTimer = 0f;

    #region Unity Lifecycle Methods

    private void Start() {
        unitCountImages[0].GetComponent<MainBuildingUnitCountDisplay>().mouseOver += SetSecondaryUnitDisplayersState;
        SetCastleUniqueId();
        UpdateLocalUnitCountUI();
    }

    private void Update() {
        ProduceUnits();
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
        if(!IsHost) return;
        
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
        if (string.IsNullOrEmpty(castleUniqueId))
        {
            Debug.LogWarning("CastleUniqueId is not yet initialized on the client!");
        }
        return castleUniqueId;
    }
    
    #endregion

    #region Ownership

    private void UpdateCastleOwner()
    {
        //checking if there are any units at all

        bool proceed = false;

        foreach(KeyValuePair<ulong, int> pair in unitCounts)
        {
            if (pair.Value > 0)
            {
                proceed = true;
            }
        }

        if (proceed != true) return;

        var topPlayers = GetTopThreePlayers();

        if(NetworkObject.OwnerClientId == topPlayers[0].Key) return;

        GetComponent<NetworkObject>().ChangeOwnership(topPlayers[0].Key);

        UpdateCastleColorClientRpc(GameManager.Instance.playerColors[topPlayers[0].Key]);
    }

    #endregion

    #region Unit Management

    private void ProduceUnits()
    {
        if (!IsHost || !producesUnits) return;

        unitProductionTimer += Time.deltaTime;

        if (unitProductionTimer >= unitProductionInterval)
        {
            unitProductionTimer = 0f;

            AddUnits(OwnerClientId, unitsPerInterval);
        }
    }

    public int GetUnitCount(ulong clientId)
    {
        return unitCounts.ContainsKey(clientId) ? unitCounts[clientId] : 0;
    }

    public void AddUnits(ulong clientId, int count)
    {
        if(!IsHost) return;

        if (unitCounts.ContainsKey(clientId))
        {
            unitCounts[clientId] += count;
        }
        else
        {
            unitCounts[clientId] = count;
        }

        UpdateCastleOwner();
        SendUnitCountsToClients();
    }

    public void RemoveUnits(ulong clientId, int count)
    {
        RemoveUnitsServerRpc(clientId, count);
    }

    [ServerRpc]
    private void RemoveUnitsServerRpc(ulong clientId, int count)
    {
        if(!IsHost) return;

        if (unitCounts.ContainsKey(clientId))
        {
            unitCounts[clientId] -= count;
        }

        UpdateCastleOwner();
        SendUnitCountsToClients();
    }

    private List<KeyValuePair<ulong, int>> GetTopThreePlayers()
    {
        // Проверяем, что словарь не пуст
        if (unitCounts == null || unitCounts.Count == 0)
        {
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
        UpdateLocalUnitCountUI();   
    }

    public void SendUnitCountsToClients()
    {
        if (unitCounts == null || unitCounts.Count == 0)
        {
            return;
        }

        ulong[] keys;
        int[] values;
        Util.ConvertDictionaryToArrays(unitCounts, out keys, out values);

        UpdateUnitCountsClientRpc(keys, values);
    }

    #endregion

    #region UI Management

    private void UpdateLocalUnitCountUI()
    {
        
        var topPlayers = GetTopThreePlayers();

        for (int i = 0; i < topPlayers.Count; i++)
        {
            ulong clientId = topPlayers[i].Key;
            int unitCount = topPlayers[i].Value;

            // Обновляем интерфейс для игрока
            unitCountTexts[i].text = unitCount.ToString();

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