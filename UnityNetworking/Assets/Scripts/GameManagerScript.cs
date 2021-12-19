using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//ATTACH TO: something ACTIVE on the canvas

public class GameManagerScript : NetworkBehaviour
{
    public NetworkVariable<bool> StartGame = new NetworkVariable<bool>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        /*if(StartGame.Value)
        {
            if(NetworkManager.Singleton.IsServer)
            {
                //can kind of cheat; NetworkManager can run checks for if the player IsServer or IsClient. That means I can make every client and server do something so long as the server says it's okay
                //But I still need some way for the clients to know the server says it's okay
            }
            else
            {
                //request serverrpc
            }
        }*/

        if(NetworkManager.Singleton.IsServer)
        {

        }
    }

    public void StartPlay()
    {
        //button for the host to start the game
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            client.PlayerObject.transform.GetComponent<Player>().Playing.Value = true;
            client.PlayerObject.transform.GetComponent<GameManagerScript>().StartGame.Value = true;
            client.PlayerObject.transform.GetComponent<Player>().DeactivateLobby();
        }
        /*StartGame.Value = true;

        var p = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        p.GetComponent<Player>().Playing.Value = true;
        GameObject.Find("/Canvas/Panel - Lobby").SetActive(false);*/
    }
}
