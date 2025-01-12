using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class ConnectionManager : NetworkBehaviour
{
    public static ConnectionManager Instance;

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

    private async void Start() {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void StartRelay()
    {
        string joinCode = await StartHostWithRelay();

        if (joinCode != null)
        {
            UIManager.Instance.UpdateJoinCode(joinCode);
            UIManager.Instance.ShowLobbyUI();
        }
        else
        {
            Debug.LogError("Failed to start host with relay");
        }
    }

    public async void JoinRelay()
    {
        if (await StartClientWithRelay())
        {
            Debug.Log("Asking for Server to Update Shit");
            
            UIManager.Instance.ShowLobbyUI();
        }
        else
        {
            Debug.LogError("Failed to join relay");
        }
    }

    private async Task<string> StartHostWithRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(10);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    private async Task<bool> StartClientWithRelay()
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(UIManager.Instance.GetJoinInputCode());

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        return !string.IsNullOrEmpty(UIManager.Instance.GetJoinInputCode()) && NetworkManager.Singleton.StartClient();
    }

    public void Disconnect(){
        GameManager.Instance.DeletePlayerServerRpc(NetworkManager.Singleton.LocalClientId);

        NetworkManager.Singleton.Shutdown();

        UIManager.Instance.ShowMainMenu();
    }

    private void AskForLobbyDataUpdate()
    {
        GameManager.Instance.AskForLobbyDataUpdateServerRpc();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        AskForLobbyDataUpdate();
    }
}
