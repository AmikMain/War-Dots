using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    private Color myPlayerColor;
    public event Action<Color> onColorChanged;
    public Dictionary<ulong, Color> playerColors = new Dictionary<ulong, Color>();
    private int oilCount = 0;
    private int startingOilAmount = 50;
    [SerializeField] TMP_Text oilText;
    [SerializeField] GameObject CastlePrefab;

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
        Debug.Log($"Был выбран цвет под индексом {randomIndex}");
        onColorChanged?.Invoke(myPlayerColor);
    }

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
}