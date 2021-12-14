using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Drop : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    //public NetworkVariable<int> Type = new NetworkVariable<int>();
    float selfDestructTimer = 5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //hover up and down from [0.5, 1.5]. It doesn't matter if the server knows the Y value
        float theta = Time.frameCount / 40.0f;
        transform.position = new Vector3(transform.position.x, Mathf.Abs(Mathf.Sin(theta)) + 0.5f, transform.position.z);
    }

    void FixedUpdate()
    {
        selfDestructTimer -= Time.fixedDeltaTime;
        if (selfDestructTimer <= 0f) SelfDestructServerRpc();
    }

    public void Collected()
    {
        SelfDestructServerRpc();
    }

    [ServerRpc]
    public void SelfDestructServerRpc(ServerRpcParams rpcParams = default)
    {
        Destroy(gameObject);
    }
}