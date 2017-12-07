using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MovingObject {

    public GameObject gunshot;

    public void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        inverseAttackSpeed = 1 / attackSpeed;
        health = 100;
    }

    public override IEnumerator Move()
    {
        float horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        float vertical = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        rb2d.velocity = new Vector2(horizontal, vertical);
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
        } else
        {
            return;
        }
        
        GameObject shot = Instantiate(gunshot, transform.position, Quaternion.identity, transform) as GameObject;
        Quaternion target = Quaternion.Euler(0, 0, angle);
        shot.transform.rotation = target;
        //Debug.Log(target+": "+angle.ToString());
        shot.GetComponent<GunshotController>().MoveB(facingDirection);

    }

    public IEnumerator ChangeDirection()
    {
        yield return null;
    }
	
	void Update ()
    {
        CheckIfDead();
        if (dead)
        {
            Debug.Log(name + " has died");
            enabled = false;
        }

        bool upArrow = Input.GetKey(KeyCode.UpArrow);
        bool downArrow = Input.GetKey(KeyCode.DownArrow);
        bool leftArrow = Input.GetKey(KeyCode.LeftArrow);
        bool rightArrow = Input.GetKey(KeyCode.RightArrow);
        
        //StartCoroutine(Move());
        float horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        float vertical = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        rb2d.velocity = new Vector2(horizontal, vertical);

        if ((upArrow || downArrow || rightArrow || leftArrow) && !attacking)
            StartCoroutine(Attack(upArrow, downArrow, rightArrow, leftArrow));
    }

    public IEnumerator Attack(bool upArrowPressed, bool downArrowPressed, bool rightArrowPressed, bool leftArrowPressed)
    {
        attacking = true;

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
            yield return null;
        }

        GameObject shot = Instantiate(gunshot, transform.position, Quaternion.identity, transform) as GameObject;
        Quaternion target = Quaternion.Euler(0, 0, angle);
        shot.transform.rotation = target;
        shot.GetComponent<GunshotController>().MoveB(facingDirection);

        yield return new WaitForSeconds(inverseAttackSpeed * speedConstant);
        attacking = false;
    }

    public override IEnumerator Attack()
    {
        throw new NotImplementedException();
    }

    public override void TakeDamage(FacingDirection directionOfDamage, float amountOfDamage, float amountOfKnockback)
    {
        throw new NotImplementedException();
    }
}
