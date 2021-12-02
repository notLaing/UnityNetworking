using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public override void OnNetworkSpawn()
    {
        Position.OnValueChanged += OnPositionChange;
        if(IsOwner)//
        {
            Move();
        }
    }

    public void Move()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            //set position randomly
            var randomPos = GetRandomPositionOnPlane();
            transform.position = randomPos;
            Position.Value = randomPos;
        }
        else
        {
            //request a position
            SubmitPositionRequestServerRpc();
        }
    }

    public void OnPositionChange(Vector3 prev, Vector3 value)
    {
        transform.position = value;
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
    }

    [ServerRpc]//client -> server
    public void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = GetRandomPositionOnPlane();
    }
}
