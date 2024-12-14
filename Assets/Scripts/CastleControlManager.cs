using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CastleControlManager : NetworkBehaviour
{
    [SerializeField] GameObject unitPrefab;
    [SerializeField] private int unitSpeed = 10;
    public static CastleControlManager Instance;
    public event Action<List<Castle>> OnSelectionChanged;
    private List<Castle> selectedCastles = new List<Castle>();
    private Camera mainCamera;
    public bool shiftPressed = false;
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start() {
        InputManager.Instance.onShiftPerformed += ()=> shiftPressed = true;
        InputManager.Instance.onShiftCanceled += ()=> shiftPressed = false;
        InputManager.Instance.onLMBPerformed += OnLMBSelectCastle;
        InputManager.Instance.onRMBPerformed += OnRMBSendUnits;
        mainCamera = Camera.main;
    }

    private void OnLMBSelectCastle()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition); // Создаём луч из позиции мыши
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray); // Проверяем пересечение с объектами

        if (hit.collider != null) // Если что-то было кликнуто
        {
            Castle castle = hit.collider.GetComponent<Castle>(); // Проверяем, есть ли компонент "Building"

            if (castle != null && castle.IsOwner)
            {
                Select(castle);
            }
        }
        else
        {
            ClearSelection();
        }
    }

    private void OnRMBSendUnits()
    {
        Vector3 destination;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition); // Создаём луч из позиции мыши
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray); // Проверяем пересечение с объектами

        if (hit.collider != null) // Если что-то было кликнуто
        {
            Castle castle = hit.collider.GetComponent<Castle>(); // Проверяем, есть ли компонент "Building"

            if (castle != null)
            {
                destination = castle.transform.position;
                foreach(Castle attackerCastle in selectedCastles)
                {
                    int unitCount = attackerCastle.GetUnitCount(NetworkManager.Singleton.LocalClientId);

                    if(unitCount > 0)
                    {
                        SendUnitsServerRpc(unitCount, attackerCastle.transform.position, destination, NetworkManager.Singleton.LocalClientId, attackerCastle.GetCastleUniqueId());
                        Debug.Log($"Step 1. Client. Asking castle to remove {unitCount} units.");
                        attackerCastle.RemoveUnits(NetworkManager.Singleton.LocalClientId, unitCount);
                    }     
                }
                ClearSelection();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendUnitsServerRpc(int unitCount, Vector3 spawnPos, Vector3 destination, ulong clientIdOfAttacker, string castleUniqueId)
    {
        GameObject newUnit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);

        newUnit.GetComponent<NetworkObject>().SpawnWithOwnership(clientIdOfAttacker);

        newUnit.GetComponent<Unit>().SetColor(GameManager.Instance.playerColors[clientIdOfAttacker]);

        newUnit.GetComponent<Unit>().SetDestination(destination);

        newUnit.GetComponent<Unit>().SetSpeed(unitSpeed);

        newUnit.GetComponent<Unit>().SetUnitsCount(unitCount);

        newUnit.GetComponent<Unit>().SetSpawnCastleUniqueId(castleUniqueId);

        newUnit.GetComponent<Unit>().SendUnitDataToClients();
    }

    public void Select(Castle castle)
    {
        if (selectedCastles.Contains(castle))
        {
            RemoveFromSelection(castle);
            return;
        }
        
        if(!shiftPressed)
        {
            ClearSelection();
        }
        if (!selectedCastles.Contains(castle))
        {
            selectedCastles.Add(castle);
        }

        foreach(Castle castle1 in selectedCastles)
        {
            castle1.SetCastleVisualSelection(true);
        }
    }

    public void RemoveFromSelection(Castle castle)
    {
        castle.SetCastleVisualSelection(false);
        selectedCastles.Remove(castle);
    }

    public void ClearSelection()
    {
        if (selectedCastles.Count > 0)
        {
            foreach(Castle castle in selectedCastles)
            {
                castle.SetCastleVisualSelection(false);
            }

            selectedCastles.Clear();
            OnSelectionChanged?.Invoke(selectedCastles); // Уведомляем об изменении выделения
        }
    }
}