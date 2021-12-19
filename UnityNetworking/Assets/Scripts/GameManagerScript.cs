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
        
    }

    public void StartPlay()
    {
        //button for the host to start the game
        /*foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            client.PlayerObject.transform.GetComponent<Player>().Playing.Value = true;
            //client.PlayerObject.transform.GetComponent<GameManagerScript>().StartGame.Value = true;
            client.PlayerObject.transform.GetComponent<Player>().SetIsPlaying();
            client.PlayerObject.transform.GetComponent<Player>().DeactivateLobby();
            //client.PlayerObject.transform.GetComponent<NetcodeManager>().lobby = false;
            //client.PlayerObject.transform.GetComponent<NetcodeManager>().isPlaying = true;
            //client.PlayerObject.transform.GetComponent<NetcodeManager>().gui = false;
        }*/
        StartGame.Value = true;

        var p = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        p.GetComponent<Player>().Playing.Value = true;
        /*GameObject.Find("/Canvas/Panel - Lobby").SetActive(false);*/

        HashSet<NetworkObject> list = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList;
        foreach(NetworkObject n in list)
        {
            if(n.GetComponent<Player>() != null)
            {
                var client = n.GetComponent<Player>();
                client.Playing.Value = true;
                //client.PlayerObject.transform.GetComponent<GameManagerScript>().StartGame.Value = true;
                client.SetIsPlaying();
                client.DeactivateLobby();
            }
        }
    }
}
