using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // TODO produce oil in GameManager.

    [Header("Prefabs")]
    [SerializeField] GameObject CastlePrefab;

    [Header("Oil")]
    [SerializeField] TMP_Text oilText;
    [SerializeField] private int startingOilAmount;
    [SerializeField] private int oilProductionInterval;
    [SerializeField] private int oilAmountPerInterval;
    private int oilCount = 0;
    private float oilProductionTimer;

    public static GameManager Instance;
    private Color myPlayerColor;
    public event Action<Color> onColorChanged;
    public Dictionary<ulong, Color> playerColors = new Dictionary<ulong, Color>();
    private List<Castle> playerOwnedBuildings = new List<Castle>();
    

    #region Main Methods
    private void Awake() {

        DontDestroyOnLoad(gameObject);

        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }

    }

    private void Start() {
        AssignLocalColor();
    }

    private void Update() {
        ProduceOilCountdown();
    }

    public void TryStartGame()
    {
        if(!IsHost) return;

        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        //Adding color of the Host
        if(!playerColors.ContainsKey(NetworkManager.Singleton.LocalClientId))
        {
            playerColors.Add(NetworkManager.Singleton.LocalClientId, myPlayerColor);
        }

        //Adding colors of clients
        SendPlayerColorClientRpc(); 

        yield return new WaitForSeconds(1.0f);

        if(NetworkManager.Singleton.ConnectedClientsIds.Count == playerColors.Count)
        {
            ulong[] clientIds;
            Color[] colors;
            Util.ConvertDictionaryToArrays(playerColors, out clientIds, out colors);
            SendPlayerColorsClientRpc(clientIds, colors);

            yield return new WaitForSeconds(1.0f);

            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                SpawnCastleForPlayer(clientId);
            }

            SetOilAmountClientRpc();
        }
        else
        {
            //Adding color of the Host
            if(!playerColors.ContainsKey(NetworkManager.Singleton.LocalClientId))
            {
                playerColors.Add(NetworkManager.Singleton.LocalClientId, myPlayerColor);
            }
            //Adding colors of clients
            SendPlayerColorClientRpc(); 
        }
    }

    private void SpawnCastleForPlayer(ulong clientId)
    {

        if(!IsHost) return;

        Debug.Log("Spawning Castle for player.");

        Vector3 spawnPos = GetSpawnPosition();

        GameObject newCastle = Instantiate(CastlePrefab, spawnPos, Quaternion.identity);

        newCastle.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        newCastle.GetComponent<Castle>().SetCastleColor(playerColors[clientId]);

        newCastle.GetComponent<Castle>().SetCastlePosition(spawnPos);

        newCastle.GetComponent<Castle>().AddUnits(clientId, 10);

        MoveClientCameraClientRpc(spawnPos, clientId);
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(UnityEngine.Random.Range(-20, 20), UnityEngine.Random.Range(-20, 20), 0);
    }

    [ClientRpc]
    private void MoveClientCameraClientRpc(Vector3 pos, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            FindObjectOfType<PlayerMovement>().transform.position = new Vector3(pos.x, pos.y, 0);
        }
    }

    [ClientRpc]
    private void SendPlayerColorsClientRpc(ulong[] clientIds, Color[] colors)
    {
        Debug.Log("Client. Receiving player colors...");

        playerColors = Util.ConvertArraysToDictionary(clientIds, colors);

        foreach (var pair in playerColors)
        {
            Debug.Log($"Client. Player {pair.Key} has color {pair.Value}");
        }
    }

    [ClientRpc]
    private void SendPlayerColorClientRpc()
    {
        if(IsClient)
        {
            Debug.Log("Client. Trying to give server the color data");
            ReceivePlayerColorServerRpc(NetworkManager.Singleton.LocalClientId, myPlayerColor);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReceivePlayerColorServerRpc(ulong playerId, Color color)
    {
        Debug.Log("Host. Received client color");

        if(!playerColors.ContainsKey(playerId))
        {
            Debug.Log("Host. Client Color wasnt listed so it was added");
            playerColors.Add(playerId, color);
        }
    }

    public void AssignLocalColor()
    {
        int randomIndex = UnityEngine.Random.Range(0, PlayerColors.PlayerColorsList.Length);
        myPlayerColor = PlayerColors.PlayerColorsList[randomIndex];
        onColorChanged?.Invoke(myPlayerColor);
    }

    #endregion

    #region Buildings
    public void AddCastle(Castle castle)
    {
        if (playerOwnedBuildings.Contains(castle)) return;

        if (!castle.IsOwner) return;
        
        playerOwnedBuildings.Add(castle);
        Debug.Log($"Adding a building. Building count is now {playerOwnedBuildings.Count}");
    }

    public void RemoveCastle(Castle castle)
    {
        if (!playerOwnedBuildings.Contains(castle)) return;

        if (castle.IsOwner) return;
        
        playerOwnedBuildings.Remove(castle);
        Debug.Log($"Removed a building. Building count is now {playerOwnedBuildings.Count}");
    }
    #endregion

    #region Oil Rigs

    private void ProduceOilCountdown()
    {
        oilProductionTimer += Time.deltaTime;

        if (oilProductionTimer >= oilProductionInterval)
        {
            oilProductionTimer = 0f;

            ProduceOil();
        }
    }

    private void ProduceOil()
    {
        float oilFromOneOilRig = 0f;

        int oilRigs = 0;

        foreach (Castle building in playerOwnedBuildings)
        {
            if(building.IsOilRig()) oilRigs++;
        }

        if(oilRigs == 1)
        {
            oilFromOneOilRig = oilAmountPerInterval;
        }
        else if(oilRigs == 2)
        {
            oilFromOneOilRig = oilAmountPerInterval * 0.9f;
        }
        else if(oilRigs == 3)
        {
            oilFromOneOilRig = oilAmountPerInterval * 0.8f;
        }
        else if(oilRigs == 4)
        {
            oilFromOneOilRig = oilAmountPerInterval * 0.7f;
        }
        else if(oilRigs == 5)
        {
            oilFromOneOilRig = oilAmountPerInterval * 0.6f;
        }
        else if(oilRigs == 6)
        {
            oilFromOneOilRig = oilAmountPerInterval * 0.5f;
        }
        else if(oilRigs >= 7)
        {
            oilFromOneOilRig = oilAmountPerInterval * 0.45f;
        }

        Debug.Log($"Adding Oil: {(Mathf.RoundToInt(oilFromOneOilRig * oilRigs)).ToString()}");

        ChangeOilAmount(Mathf.RoundToInt(oilFromOneOilRig * oilRigs));
    }
    

    #endregion

    #region Oil
    public void ChangeOilAmount(int amount)
    {
        oilCount += amount;
        UpdateOilUi();
    }

    private void UpdateOilUi()
    {
        oilText.text = oilCount.ToString();
    }

    [ClientRpc]
    public void SetOilAmountClientRpc()
    {
        oilCount = 0;
        ChangeOilAmount(startingOilAmount);
    }
    
    public int GetOilCount()
    {
        return oilCount;
    }

    #endregion
}