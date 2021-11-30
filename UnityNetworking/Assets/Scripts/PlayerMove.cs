using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public Animator anim;
    public CharacterController controller;
    Vector3 moveDirection;
    float horizontal = 0;
    float vertical = 0;
    float speed = 10f;
    float rotationSpeed = 1000f;
    float rotateAngle = -26f;
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
            moveDirection = Vector3.Normalize(new Vector3(horizontal, 0f, vertical));
            //transform.position += moveDirection * speed * Time.fixedDeltaTime;
            controller.Move(moveDirection * speed * Time.fixedDeltaTime);
            //transform.Translate(moveDirection * speed * Time.fixedDeltaTime);
            //Rigidbody.Move

            Quaternion toRotation = Quaternion.LookRotation(Quaternion.AngleAxis(rotateAngle, Vector3.up) * moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.fixedDeltaTime);
            //roughly 30 degree clockwise
        }
        else
        {
            isMoving = false;
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed);// * Time.fixedDeltaTime);
        }

        anim.SetBool("isMoving", isMoving);
    }
}
