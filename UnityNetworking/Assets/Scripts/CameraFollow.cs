using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Vector3 cameraDistance;
    // Start is called before the first frame update
    void Start()
    {
        cameraDistance = new Vector3(0f, 15f, -15f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveToPlayer(Vector3 playerPos)
    {
        transform.position = playerPos + cameraDistance;
    }
}
