﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MovingObject {

    public GameObject gunshot;
    public GameObject melee;
    public LayerMask blockingLayer;

    public int invFrames;
    public FacingDirection walkingDirection;

    private bool takingDamage = false;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inverseAttackSpeed = 1 / attackSpeed;
        health = 100;
    }

	void Update ()
    {
        CheckIfDead();
        if (dead)
        {
            Debug.Log("You have died");
            enabled = false;
        }

        // Interpreting KB inputs
        SortOutArrowInputs();
    }

    public override IEnumerator Move()
    {
        float horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * 1;
        float vertical = Input.GetAxis("Vertical") * Time.deltaTime * 1;

        rb.velocity = new Vector3(horizontal, 0f, vertical);
        yield return null;
    }
	
    public void NewAttack()
    {
        bool upArrowPressed = Input.GetKey(KeyCode.UpArrow);
        bool downArrowPressed = Input.GetKey(KeyCode.DownArrow);
        bool leftArrowPressed = Input.GetKey(KeyCode.LeftArrow);
        bool rightArrowPressed = Input.GetKey(KeyCode.RightArrow);

        float angle = 0;

        // Controlling the direction the player faces plus adding an attack
        if (upArrowPressed && leftArrowPressed)
        {
            facingDirection = FacingDirection.NorthWest;
            angle = 45;
        }
        else if (upArrowPressed && rightArrowPressed)
        {
            facingDirection = FacingDirection.NorthEast;
            angle = -45;
        }
        else if (downArrowPressed && leftArrowPressed)
        {
            facingDirection = FacingDirection.SouthWest;
            angle = 135;
        }
        else if (downArrowPressed && rightArrowPressed)
        {
            facingDirection = FacingDirection.SouthEast;
            angle = 225;
        }
        else if (upArrowPressed && !downArrowPressed)
        {
            facingDirection = FacingDirection.North;
            angle = 0;
        }
        else if (leftArrowPressed && !rightArrowPressed)
        {
            facingDirection = FacingDirection.West;
            angle = 90;
        }
        else if (rightArrowPressed && !leftArrowPressed)
        {
            facingDirection = FacingDirection.East;
            angle = -90;
        }
        else if (downArrowPressed && !upArrowPressed)
        {
            facingDirection = FacingDirection.South;
            angle = 180;
        }
        else
        {
            return;
        }
        
        GameObject shot = Instantiate(gunshot, transform.position, Quaternion.identity, transform) as GameObject;
        Quaternion target = Quaternion.Euler(90, angle, 0);
        shot.transform.rotation = target;
        //Debug.Log(target+": "+angle.ToString());
        shot.GetComponent<GunshotController>().MoveB(facingDirection);

    }

    public void SortOutArrowInputs()
    {
        bool upArrow = Input.GetKey(KeyCode.UpArrow);
        bool downArrow = Input.GetKey(KeyCode.DownArrow);
        bool leftArrow = Input.GetKey(KeyCode.LeftArrow);
        bool rightArrow = Input.GetKey(KeyCode.RightArrow);
        bool rightCTRL = Input.GetKeyDown(KeyCode.RightControl);

        //StartCoroutine(Move());
        float horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        float vertical = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        if (vertical >= 1 && horizontal <= -1)
        {
            walkingDirection = FacingDirection.NorthWest;

        }
        else if (vertical >= 1 && horizontal >= 1)
        {
            walkingDirection = FacingDirection.NorthEast;

        }
        else if (vertical <= -1 && horizontal <= -1)
        {
            walkingDirection = FacingDirection.SouthWest;

        }
        else if (vertical <= -1 && horizontal >= 1)
        {
            walkingDirection = FacingDirection.SouthEast;

        }
        else if (vertical >= 1 && !(vertical <= -1))
        {
            walkingDirection = FacingDirection.North;

        }
        else if (horizontal <= -1 && !(horizontal >= 1))
        {
            walkingDirection = FacingDirection.West;

        }
        else if (horizontal >= 1 && !(horizontal <= -1))
        {
            walkingDirection = FacingDirection.East;

        }
        else if (vertical <= -1 && !(vertical >= 1))
        {
            walkingDirection = FacingDirection.South;

        }

        // Moves the player, or stops them if no input
        rb.velocity = new Vector3(horizontal, 0f, vertical);

        // If the player is pressing an attack button
        if ((upArrow || downArrow || rightArrow || leftArrow || rightCTRL) && !attacking)
            StartCoroutine(Attack(upArrow, downArrow, rightArrow, leftArrow, rightCTRL));
    }

    public IEnumerator Attack(bool upArrowPressed, bool downArrowPressed, bool rightArrowPressed, bool leftArrowPressed, bool ctrl)
    {
        attacking = true;

        float angle = 0;
        Vector3 directionV = Vector3.forward;
        Vector3 comepnenstation = new Vector3(0, 0, 0);

        if (ctrl)
        {
            // Interpreting the facing direction to a Euler angle
            switch (walkingDirection)
            {
                case FacingDirection.East:
                    directionV = new Vector3(1, .5f, 0);
                    comepnenstation = new Vector3(1, 0, 0);
                    angle = -90;
                    break;

                case FacingDirection.West:
                    directionV = new Vector3(-1, .5f, 0);
                    comepnenstation = new Vector3(-1, 0, 0);
                    angle = 90;
                    break;

                case FacingDirection.North:
                    directionV = new Vector3(0, .5f, 1);
                    comepnenstation = new Vector3(0, 0, 1);
                    angle = 0;
                    break;

                case FacingDirection.South:
                    directionV = new Vector3(0, .5f, -1);
                    comepnenstation = new Vector3(0, 0, -1);
                    angle = 180;
                    break;

                case FacingDirection.NorthEast:
                    directionV = new Vector3(1, .5f, 1);
                    comepnenstation = new Vector3(.5f, 0, .5f);
                    angle = -45;
                    break;

                case FacingDirection.SouthEast:
                    directionV = new Vector3(1, .5f, -1);
                    comepnenstation = new Vector3(.5f, 0, -.5f);
                    angle = 225;
                    break;

                case FacingDirection.NorthWest:
                    directionV = new Vector3(-1, .5f, 1);
                    comepnenstation = new Vector3(-.5f, 0, .5f);
                    angle = 45;
                    break;

                case FacingDirection.SouthWest:
                    directionV = new Vector3(-1, .5f, -1);
                    comepnenstation = new Vector3(-.5f, 0, -.5f);
                    angle = 135;
                    break;
            }

            // Melee attack
            GameObject attack = Instantiate(melee, transform.position + comepnenstation, Quaternion.identity, transform) as GameObject;
            Quaternion target = Quaternion.Euler(90, 0, angle);
            attack.transform.rotation = target;
            attack.GetComponent<MeleeController>().Attack(walkingDirection);
        }
        else
        {
            // Interpreting the arrow key inputs as directions
            if (upArrowPressed && leftArrowPressed)
            {
                facingDirection = FacingDirection.NorthWest;
                directionV = new Vector3(-1, .5f, 1);

                angle = 45;
            }
            else if (upArrowPressed && rightArrowPressed)
            {
                facingDirection = FacingDirection.NorthEast;
                directionV = new Vector3(1, .5f, 1);

                angle = -45;
            }
            else if (downArrowPressed && leftArrowPressed)
            {
                facingDirection = FacingDirection.SouthWest;
                directionV = new Vector3(-1, .5f, -1);

                angle = 135;
            }
            else if (downArrowPressed && rightArrowPressed)
            {
                facingDirection = FacingDirection.SouthEast;
                directionV = new Vector3(1, .5f, -1);

                angle = 225;
            }
            else if (upArrowPressed && !downArrowPressed)
            {
                facingDirection = FacingDirection.North;
                directionV = new Vector3(0, .5f, 1);

                angle = 0;
            }
            else if (leftArrowPressed && !rightArrowPressed)
            {
                facingDirection = FacingDirection.West;
                directionV = new Vector3(-1, .5f, 0);

                angle = 90;
            }
            else if (rightArrowPressed && !leftArrowPressed)
            {
                facingDirection = FacingDirection.East;
                directionV = new Vector3(1, .5f, 0);

                angle = -90;
            }
            else if (downArrowPressed && !upArrowPressed)
            {
                facingDirection = FacingDirection.South;
                directionV = new Vector3(0, .5f, -1);

                angle = 180;
            }
            else
            {
                yield return null;
            }

            // Ranged attack
            GameObject shot = Instantiate(gunshot, transform.position + directionV, Quaternion.identity, transform) as GameObject;
            Quaternion target = Quaternion.Euler(90, 0, angle);
            shot.transform.rotation = target;
            shot.GetComponent<GunshotController>().MoveB(facingDirection);
        }

        

        yield return new WaitForSeconds(inverseAttackSpeed * speedConstant);
        attacking = false;
    }

    public override IEnumerator Attack()
    {
        throw new NotImplementedException();
    }

    public new void TakeDamage(float amountOfDamage)
    {
        StartCoroutine(TakeDamageC(amountOfDamage));
    }

    public IEnumerator TakeDamageC(float amount)
    {
        // Won't take damage if being hit already
        if (takingDamage)
            yield break;

        takingDamage = true;
        health -= amount;
        yield return new WaitForSeconds(invFrames);
        takingDamage = false;
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;
        //Debug.Log("test");

        switch (other.tag)
        {
            case "Enemy":
                TakeDamage(2);
                break;
        }
    }

    private void OnCollision(Collision collision)
    {
        Collider other = collision.collider;
        Debug.Log("test2");

        switch (other.tag)
        {
            case "Enemy":
                TakeDamage(2);
                break;
        }
    }
}
