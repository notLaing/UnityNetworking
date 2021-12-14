using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DropServer : NetworkBehaviour
{
    [SerializeField] private NetworkObject dropPoint;
    [SerializeField] private NetworkObject dropSpeed;
    [SerializeField] private NetworkObject dropSteal;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CallSpawnDrop()
    {
        SpawnDropsServerRpc();
    }

    [ServerRpc]
    public void SpawnDropsServerRpc(ServerRpcParams rpcParams = default)
    {
        //only allow the server to spawn drops
        if (NetworkManager.Singleton.IsServer)
        {
            //spawn 20 drops every 3 seconds
            for(int i = 0; i < 20; ++i)
            {
                //set variables (position, type)
                float x = Random.Range(-55f, 55f);
                float z;
                if (x < 0)
                {
                    z = Random.Range(-55f - x, x + 55f);
                }
                else
                {
                    z = Random.Range(x - 55f, 55f - x);
                }
                Vector3 pos = new Vector3(x, 0.5f, z);

                switch(Mathf.Floor(Random.Range(0f, 9f)))
                {
                    case 0:
                        //spawn steal
                        dropSteal.GetComponent<Drop>().Position.Value = pos;
                        NetworkObject stealInstance = Instantiate(dropSteal, pos, Quaternion.identity);
                        stealInstance.Spawn();
                        break;
                    case 1:
                        //spawn speed
                        dropSpeed.GetComponent<Drop>().Position.Value = pos;
                        NetworkObject speedInstance = Instantiate(dropSpeed, pos, Quaternion.identity);
                        speedInstance.Spawn();
                        break;
                    default:
                        //spawn point
                        dropPoint.GetComponent<Drop>().Position.Value = pos;
                        NetworkObject pointInstance = Instantiate(dropPoint, pos, Quaternion.identity);
                        pointInstance.Spawn();//.GetComponent<NetworkObject>().Spawn();
                        break;
                }//switch
            }
            
        }
    }
}
