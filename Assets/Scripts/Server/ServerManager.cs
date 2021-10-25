using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;

public class ServerManager : MonoBehaviour
{
    public NetworkManager targetManager;


    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager가 아직 초기화되지 않았습니다.");
            return;
        }

#if UNITY_SERVER
        var trans = NetworkManager.Singleton.GetComponent<UNetTransport>();
        trans.ConnectAddress = "127.0.0.1";
        NetworkManager.Singleton.StartServer();
#endif

        if (targetManager == null)
            targetManager = NetworkManager.Singleton;

        targetManager.OnClientConnectedCallback += OnClientConnected;
        targetManager.OnClientDisconnectCallback += OnClientDisconnected;
        targetManager.OnServerStarted += OnServerStated;
    }

    private void OnServerStated()
    {
        Debug.Log("Server on");
    }

    private void OnClientConnected(ulong id)
    {
        Debug.Log($"Connected {id}");
    }
    private void OnClientDisconnected(ulong id)
    {
        Debug.Log($"Disconnected {id}");
    }
}
