using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CastleControlManager : NetworkBehaviour
{
    #region Main

    [SerializeField] GameObject unitPrefab;
    [SerializeField] private LayerMask castleLayer;
    public static CastleControlManager Instance;
    public event Action<List<Castle>> OnSelectionChanged;
    private List<Castle> selectedCastles = new List<Castle>();
    private Camera mainCamera;
    public bool shiftPressed = false;
    public bool ctrlPressed = false;
    public bool altPressed = false;

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

        InputManager.Instance.onCtrlPerformed += ()=> ctrlPressed = true;
        InputManager.Instance.onCtrlCanceled += ()=> ctrlPressed = false;

        InputManager.Instance.onAltPerformed += ()=> altPressed = true;
        InputManager.Instance.onAltCanceled += ()=> altPressed = false;

        InputManager.Instance.onLMBPerformed += OnLMBSelectCastle;
        InputManager.Instance.onRMBPerformed += OnRMBSendUnits;

        mainCamera = Camera.main;

        SetMissilePrice();
    }

    private void OnLMBSelectCastle()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition); // Создаём луч из позиции мыши
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray); // Получаем все пересечения

        Castle selectedCastle = null;

        foreach (var hit in hits)
        {
            Castle castle = hit.collider.GetComponent<Castle>();
            if (castle != null && castle.IsOwner)
            {
                selectedCastle = castle;
                break; // Приоритетно выбираем первый найденный замок
            }
        }

        if (selectedCastle != null)
        {
            Select(selectedCastle);
        }
        else
        {
            ClearSelection();
        }

        CheckForSilos();
    }

    

    private void OnRMBSendUnits()
    {
        Vector3 destination;

        // Создаём луч из позиции мыши
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Получаем все пересечения
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray);

        Castle targetCastle = null;

        // Ищем замок в пересечениях
        foreach (var hit in hits)
        {
            Castle castle = hit.collider.GetComponent<Castle>();
            if (castle != null)
            {
                targetCastle = castle;
                break; // Останавливаемся на первом найденном замке
            }
        }

        if (targetCastle != null)
        {
            destination = targetCastle.transform.position;

            foreach (Castle attackerCastle in selectedCastles)
            {
                int unitCount = attackerCastle.GetUnitCount(NetworkManager.Singleton.LocalClientId);

                int attackingUnitCount;

                if (ctrlPressed)
                {
                    attackingUnitCount = Mathf.RoundToInt(unitCount * 0.5f);
                }
                else if (altPressed)
                {
                    attackingUnitCount = Mathf.RoundToInt(unitCount * 0.1f);
                }
                else
                {
                    attackingUnitCount = unitCount;
                }

                // Убедимся, что отправляем войска только в другие замки
                if (attackingUnitCount > 0 && attackerCastle.GetCastleUniqueId() != targetCastle.GetCastleUniqueId())
                {
                    SendUnitsServerRpc(
                    attackingUnitCount,
                    attackerCastle.transform.position,
                    destination,
                    NetworkManager.Singleton.LocalClientId,
                    attackerCastle.GetCastleUniqueId()
                    );

                    attackerCastle.RemoveUnits(NetworkManager.Singleton.LocalClientId, attackingUnitCount);
                }
            }

            ClearSelection();
        }
    }
    

    [ServerRpc(RequireOwnership = false)]
    private void SendUnitsServerRpc(int unitCount, Vector3 spawnPos, Vector3 destination, ulong clientIdOfAttacker, string castleUniqueId)
    {
        GameObject newUnit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);

        newUnit.GetComponent<NetworkObject>().SpawnWithOwnership(clientIdOfAttacker);

        newUnit.GetComponent<Unit>().SetColor(GameManager.Instance.playerColors[clientIdOfAttacker]);

        newUnit.GetComponent<Unit>().SetDestination(destination);

        newUnit.GetComponent<Unit>().ReturnToNormalSpeed();

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

    #endregion

    #region Silo and Missiles

    [SerializeField] GameObject missleLaunchButton;
    [SerializeField] int missilePrice;

    private void SetMissilePrice() {
        missleLaunchButton.GetComponentInChildren<TMP_Text>().text = missilePrice.ToString();
    }

    // TODO: Make missiles launch when you press the button, going into launching mode, and then press LMB to launch them
    public void TryLaunchRockets(Vector3 targetCoordinates)
    {
        int oilNeeded = CheckForNeededOil();

        if(oilNeeded > GameManager.Instance.GetOilCount())
        {
            return;
        }

        GameManager.Instance.ChangeOilAmount(-oilNeeded);

        foreach(Castle castle in selectedCastles)
        {
            castle.TryGetComponent<Silo>(out Silo silo);
            if(silo != null)
            {
                silo.LaunchMissile(targetCoordinates);
            }
        }  
    }

    private int CheckForNeededOil()
    {
        int siloCount = 0;

        foreach(Castle castle in selectedCastles)
        {
            castle.TryGetComponent<Silo>(out Silo silo);
            if(silo != null)
            {
                siloCount++;
            }
        }

        return siloCount * missilePrice;
    }

    private void CheckForSilos()
    {
        int siloCount = 0;

        foreach(Castle castle in selectedCastles)
        {
            castle.TryGetComponent<Silo>(out Silo silo);
            if(silo != null)
            {
                siloCount++;
            }
        }

            if(siloCount > 0)
            {
                ShowMissileLaunchButton(true);
            }
            else
            {
                ShowMissileLaunchButton(false);
            }
    }

    public void ShowMissileLaunchButton(bool show)
    {
        missleLaunchButton.SetActive(show);
    }

    #endregion
}