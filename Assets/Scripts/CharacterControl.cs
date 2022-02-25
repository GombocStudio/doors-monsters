using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 3f;

    //For smoothing the rotation
    public float smoothRotTime = 0.1f;
    private float smoothSpeed;

    //Object of the interaction
    Interactable interactable = null;

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); //Between -1(A) and 1(D)
        float vertical = Input.GetAxisRaw("Vertical"); //Between -1(S) and 1(W)
        Vector3 direction = new Vector3(horizontal, 0.0f, vertical).normalized; //Movimiento en 2D

        if(direction.magnitude >= 0.1f) //If there is a direction (movement)
        {
            //Rotate the character toward the direction it is moving
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            //Smooth the rotation
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothSpeed, smoothRotTime);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

            //We move our character
            // controller.Move(direction * speed * Time.deltaTime);
        }

        /*
         We check whenever the player presses space.
         If it is in range of a monster: hunt monster
         If it is in range of player: short range attack
         If it is not in range of anything: long range attack on looking direction
        */
        if (Input.GetKeyDown("space"))
        {
            print("space key was pressed");
            if (interactable != null) //If there is any interactable object in range
            {
                if (interactable.tag == "Monster")
                {
                    //If the object is a monster
                    print("Hunt Monster");
                }
                else if (interactable.tag == "Enemy")
                {
                    print("Short Range Attack");
                }
            }
            else
            {
                print("Long Range Attack");
            }
            
        }

        //We remove the interactable object when we are far
        if (interactable != null && Vector3.Distance(transform.position, interactable.transform.position) > 1.0f)
        {
            endCollision();
        }

    }

    /*private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Guardamos el objecto en rango de interaccion
        if (hit.collider.GetComponent<Interactable>() != null)
        {
            interactable = hit.collider.GetComponent<Interactable>();

            //We check if the object is either an egg or a door
            if (interactable.tag == "Egg")
            {
                //If the object is a monster
                print("Grab egg");
                endCollision();
            }
            else if (interactable.tag == "Door")
            {
                print("Door stuff");
                endCollision();
            }
            else if (interactable.tag == "PowerUp")
            {
                print("Grab powerup");
                endCollision();
            }
        }
    }*/

    private void OnTriggerEnter(Collider other)
    {
        //Guardamos el objecto en rango de interaccion
        if (other.GetComponent<Interactable>() != null)
        {
            interactable = other.GetComponent<Interactable>();

            //We check if the object is either an egg or a door
            if (interactable.tag == "Egg")
            {
                //If the object is a monster
                print("Grab egg");
                endCollision();
            }
            else if (interactable.tag == "Door")
            {
                print("Door stuff");
                endCollision();
            }
            else if (interactable.tag == "PowerUp")
            {
                print("Grab powerup");
                endCollision();
            }
        }
    }

    private void endCollision()
    {
        print("End collision");
        interactable = null;
    }

}
