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
    public NetworkVariable<float> Speed = new NetworkVariable<float>();
    public NetworkVariable<float> SpeedTime = new NetworkVariable<float>();
    public NetworkVariable<float> GameTime = new NetworkVariable<float>();//ONLY the server player's time matters
    public NetworkVariable<int> Points = new NetworkVariable<int>();
    public NetworkVariable<bool> Buffed = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        Position.OnValueChanged += OnPositionChange;
        if(IsOwner)//
        {
            Move();
            SetVariables();
        }
    }

    public void SetVariables()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            //set variables
            Speed.Value = 150f;
            SpeedTime.Value = 0f;
            GameTime.Value = 120f;
            Points.Value = 0;
            Buffed.Value = false;
        }
        else
        {
            //request values to be set
            StartVariableRequestServerRpc();
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
            //move camera only to this player's playerObj
            //if (IsOwner) FindObjectOfType<CameraFollow>().MoveToPlayer(transform.gameObject);
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
                CharacterController controller = transform.gameObject.GetComponent<CharacterController>();
                controller.Move(m * Speed.Value * Time.fixedDeltaTime);
                //using controller to move, so don't need to do transform.position += m. HOWEVER, should set transform.position = Position.Value?
                //transform.position += m;
                transform.rotation = r;
                //Position.Value += m;
                Position.Value = transform.position;
                Rotation.Value = r;
                //move camera only to this player's playerObj
                //if (IsOwner) FindObjectOfType<CameraFollow>().MoveToPlayer(transform.gameObject);
            }
            else
            {
                //request a position
                SubmitMoveRequestServerRpc(m, r);
            }
        }
        else
        {
            if (NetworkManager.Singleton.IsServer)
            {
                //set rotation on not moving
                transform.rotation = r;
                Rotation.Value = r;
                Animator a = transform.gameObject.GetComponent<Animator>();
                a.SetBool("isMoving", false);
            }
            else
            {
                //request a position
                SubmitMoveRequestServerRpc(m, r);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //check if the player hit a drop
        if (other.tag == "Drop_Point")
        {
            other.GetComponent<Drop>().Collected();
            //var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            //var player = playerObj.GetComponent<Player>();
            IncreasePoints();
            Debug.Log("Points: " + Points.Value);
        }
        else if (other.tag == "Drop_Speed")
        {
            other.GetComponent<Drop>().Collected();
            IncreaseSpeed();
            Debug.Log("Speed up");
        }
        else if (other.tag == "Drop_Steal")
        {
            other.GetComponent<Drop>().Collected();
            StealBuff();
            Debug.Log("Steal buff ready");
        }
        //check if this player hit another player while this player has the steal buff
        else if (other.tag == "Player")
        {
            Debug.Log("Player");
            if (Buffed.Value)
            {
                //steal up to 5 points from the other player
                var otherPlayer = other.gameObject.GetComponent<Player>();

                if (otherPlayer.Points.Value < 5)
                {
                    Steal(otherPlayer.Points.Value);
                    otherPlayer.Robbed(otherPlayer.Points.Value);
                }
                else
                {
                    Steal(5);
                    otherPlayer.Robbed(5);
                }
            }
        }
    }

    /*void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision");
        //check if this player hit another player while this player has the steal buff
        if (collision.gameObject.tag == "Player")
        {
            if (Buffed.Value)
            {
                //steal up to 5 points from the other player
                var otherPlayer = collision.gameObject.GetComponent<Player>();

                if (otherPlayer.Points.Value < 5)
                {
                    Steal(otherPlayer.Points.Value);
                    otherPlayer.Robbed(otherPlayer.Points.Value);
                }
                else
                {
                    Steal(5);
                    otherPlayer.Robbed(5);
                }
            }
        }
    }*/

    /*public void TimePass()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GameTime.Value -= Time.fixedDeltaTime;
        }
        else
        {
            //PointsRequestServerRpc();
        }
    }*/

    /*public void EndGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            FindObjectOfType<NetcodeManager>().Restart();
            Destroy(gameObject);
        }
        else
        {
            NetcodeManager n = FindObjectOfType<NetcodeManager>();
            RestartServerRpc(n);
        }
    }*/

    public void IncreasePoints()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            ++Points.Value;
        }
        else
        {
            PointsRequestServerRpc();
        }
    }

    public void IncreaseSpeed()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if(Speed.Value == 150f) Speed.Value *= 1.5f;
            SpeedTime.Value = 5f;
        }
        else
        {
            SpeedIncreaseRequestServerRpc();
        }
    }

    public void DecreaseSpeed()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Speed.Value = 150f;
        }
        else
        {
            SpeedDecreaseRequestServerRpc();
        }
    }

    public void DecreaseTime()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SpeedTime.Value -= Time.fixedDeltaTime;
        }
        else
        {
            TimeDecreaseRequestServerRpc();
        }
    }

    public void StealBuff()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Buffed.Value = true;
        }
        else
        {
            BuffRequestServerRpc();
        }
    }

    public void Steal(int stolen)
    {
        //take up to 5 points away from the other player
        if (NetworkManager.Singleton.IsServer)
        {
            Buffed.Value = false;
            Points.Value += stolen;
        }
        else
        {
            BuffOffServerRpc(stolen);
        }
    }

    public void Robbed(int lost)
    {
        //lose up to 5 points
        if (NetworkManager.Singleton.IsServer)
        {
            Points.Value -= lost;
        }
        else
        {
            LosePointsServerRpc(lost);
        }
    }

    public void OnPositionChange(Vector3 prev, Vector3 value)
    {
        transform.position = value;
        transform.rotation = Rotation.Value;
        if (prev != value)
        {
            Animator a = transform.gameObject.GetComponent<Animator>();
            a.SetBool("isMoving", true);
        }
        else
        {
            Animator a = transform.gameObject.GetComponent<Animator>();
            a.SetBool("isMoving", false);
        }
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        //random start position in one of the pentagon's corners
        Vector3 start = Vector3.zero;
        switch(Mathf.Floor(Random.Range(0f, 4f)))
        {
            case 0:
                start = new Vector3(-36f, 0f, 56f);
                break;
            case 1:
                start = new Vector3(36f, 0f, 56f);
                break;
            case 2:
                start = new Vector3(58f, 0f, -13f);
                break;
            case 3:
                start = new Vector3(0, 0f, -55f);
                break;
            case 4:
                start = new Vector3(-58f, 0f, -13f);
                break;
        }
        return start;
    }

    [ServerRpc]//client -> server
    public void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Called from button");
        Position.Value = GetRandomPositionOnPlane();
        //move camera only to this player's playerObj
        //if(IsOwner) FindObjectOfType<CameraFollow>().MoveToPlayer(transform.gameObject);
    }

    [ServerRpc]//client -> server
    public void SubmitMoveRequestServerRpc(Vector3 m, Quaternion r, ServerRpcParams rpcParams = default)
    {
        Debug.Log("Called from move");
        //not sure if the next two lines can be here, since they might take away from Server authority
        //however, since I'm doing this in a ServerRpc, I'm really moving an object from the server, aren't I? What's the real difference between this and {Position.Value = GetRandomPositionOnPlane()} from class?
        CharacterController controller = transform.gameObject.GetComponent<CharacterController>();
        controller.Move(m * Speed.Value * Time.fixedDeltaTime);

        //Position.Value += m;
        Position.Value = transform.position;
        Rotation.Value = r;
        //move camera only to this player's playerObj
        //if(IsOwner) FindObjectOfType<CameraFollow>().MoveToPlayer(transform.gameObject);

        if (m == Vector3.zero)
        {
            Animator a = transform.gameObject.GetComponent<Animator>();
            a.SetBool("isMoving", false);
        }
    }

    [ServerRpc]
    public void StartVariableRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Speed.Value = 150f;
        SpeedTime.Value = 0f;
        Points.Value = 0;
        Buffed.Value = false;
    }

    [ServerRpc]
    public void PointsRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        ++Points.Value;
    }

    [ServerRpc]
    public void SpeedIncreaseRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        if(Speed.Value != 150f) Speed.Value *= 1.5f;
        SpeedTime.Value = 5f;
    }

    [ServerRpc]
    public void SpeedDecreaseRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Speed.Value = 150f;
        SpeedTime.Value = 0f;
    }

    [ServerRpc]
    public void TimeDecreaseRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        SpeedTime.Value -= Time.fixedDeltaTime;
    }

    [ServerRpc]
    public void BuffRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Buffed.Value = true;
    }

    [ServerRpc]
    public void BuffOffServerRpc(int s, ServerRpcParams rpcParams = default)
    {
        Buffed.Value = false;
        Points.Value += s;
    }

    [ServerRpc]
    public void LosePointsServerRpc(int l, ServerRpcParams rpcParams = default)
    {
        Points.Value -= l;
    }

    /*[ServerRpc]
    public void RestartServerRpc(NetcodeManager n, ServerRpcParams rpcParams = default)
    {
        n.Restart();
    }*/
}
