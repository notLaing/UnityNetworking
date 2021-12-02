using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransformTest : NetworkBehaviour
{
    // Update is called once per frame
    void Update()
    {
        /*if(IsServer)
        {
            float theta = Time.frameCount / 20.0f;
            transform.position = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta));
        }*/
    }
}
