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

    public abstract void TakeDamage(FacingDirection directionOfDamage, float amountOfDamage, float amountOfKnockback);

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
