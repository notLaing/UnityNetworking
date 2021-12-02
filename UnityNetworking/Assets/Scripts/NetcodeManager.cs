using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetcodeManager : MonoBehaviour
{
    public void OnGUI()
    {
        //runs at the start of the game and whenever UI has to do something
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));
        if(!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
            SubmitNewPosition();
        }

        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" :
            NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Mode: " + mode);
    }

    static void SubmitNewPosition()
    {
        string buttonLabel = NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change";
        if(GUILayout.Button(buttonLabel))
        {
            var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            var player = playerObj.GetComponent<Player>();
            player.Move();
        }
    }
}
