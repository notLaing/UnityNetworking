using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    /// <summary>
    /// This extends NetworkBehavior, meaning it's respobsible for connections, not the input and stuff you see in the game
    /// NetworkVariables should contain things pertinent to the player that the SERVER NEEDS TO KNOW, like a player's position, points, etc
    /// </summary>
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<Quaternion> Rotation = new NetworkVariable<Quaternion>();
    //public NetworkVariable<int> Points = new NetworkVariable<int>();

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

    public void MoveInput(Vector3 m, Quaternion r)
    {
        if (m != Vector3.zero)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                //set position
                transform.position += m;
                transform.rotation = r;
                Position.Value += m;
                Rotation.Value = r;
            }
            else
            {
                //request a position
                SubmitMoveRequestServerRpc(m, r);
            }
        }
    }

    public void OnPositionChange(Vector3 prev, Vector3 value)
    {
        transform.position = value;

        /*Vector3 change = value - prev;
        change = Vector3.Normalize(change);
        Quaternion toRotation = Quaternion.LookRotation(Quaternion.AngleAxis(-26f, Vector3.up) * change, Vector3.up);
        Quaternion finalRotation = Quaternion.RotateTowards(transform.rotation, toRotation, 1000f);// * Time.fixedDeltaTime);*/
        transform.rotation = Rotation.Value;
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
    }

    [ServerRpc]//client -> server
    public void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Called from button");
        Position.Value = GetRandomPositionOnPlane();
    }

    [ServerRpc]//client -> server
    public void SubmitMoveRequestServerRpc(Vector3 m, Quaternion r, ServerRpcParams rpcParams = default)
    {
        Debug.Log("Called from move");
        Position.Value += m;
        Rotation.Value = r;
    }
}
