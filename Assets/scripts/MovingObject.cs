using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {

    public abstract IEnumerator Move();

    public abstract IEnumerator Attack();

    public bool dead = false;
    public float speedConstant = 1; // To refine the attack speed numbers
    public bool attacking;

    [SerializeField] protected float health;
    protected Rigidbody2D rb2d;
    [SerializeField] protected FacingDirection facingDirection;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float meleeRange;
    [SerializeField] protected float gunRange;
    [SerializeField] protected float attackSpeed = 10f;
    protected float inverseAttackSpeed;

    public enum FacingDirection
    {
        North, 
        NorthEast,
        East, 
        SouthEast,
        South, 
        SouthWest, 
        West, 
        NorthWest,
    }

    public void TakeDamage(FacingDirection directionOfDamage, float amountOfDamage, float amountOfKnockback)
    {
        health -= amountOfDamage;
        Vector2 force = new Vector2(0, 0);

        Debug.Log(name + " is taking " + amountOfDamage + " damage");

        switch (directionOfDamage)
        {
            case FacingDirection.North:
                force = new Vector2(0, amountOfKnockback);
                break;
            case FacingDirection.South:
                force = new Vector2(0, -amountOfKnockback);
                break;
            case FacingDirection.East:
                force = new Vector2(amountOfKnockback, 0);
                break;
            case FacingDirection.West:
                force = new Vector2(-amountOfKnockback, 0);
                break;
            case FacingDirection.NorthEast:
                force = new Vector2(amountOfKnockback / 2, amountOfKnockback / 2);
                break;
            case FacingDirection.NorthWest:
                force = new Vector2(-amountOfKnockback / 2, amountOfKnockback / 2);
                break;
            case FacingDirection.SouthEast:
                force = new Vector2(amountOfKnockback, -amountOfKnockback / 2);
                break;
            case FacingDirection.SouthWest:
                force = new Vector2(-amountOfKnockback / 2, -amountOfKnockback / 2);
                break;
        }

        rb2d.AddForce(force);
        Debug.Log(name + " is taking " + amountOfKnockback + " knockback");
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void CheckIfDead()
    {
        if (health <= 0)
            dead = true;
        else
            dead = false;
    }
}
