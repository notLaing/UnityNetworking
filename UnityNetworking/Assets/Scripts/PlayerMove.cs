using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public Animator anim;
    float horizontal = 0;
    float vertical = 0;
    float speed = 10f;
    float rotationSpeed = 700f;
    public bool isMoving = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        if(horizontal != 0f || vertical != 0f)
        {
            isMoving = true;
            Vector3 moveDirection = Vector3.Normalize(new Vector3(horizontal, 0f, vertical));
            transform.position += moveDirection * speed * Time.fixedDeltaTime;

            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            isMoving = false;
        }

        anim.SetBool("isMoving", isMoving);
    }
}
