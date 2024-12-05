using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkManagerCustom : MonoBehaviour
{
    

    public void ToolkitStartHost ()=> NetworkManager.Singleton.StartHost();
    public void ToolkitStartClient ()=> NetworkManager.Singleton.StartClient();
}
