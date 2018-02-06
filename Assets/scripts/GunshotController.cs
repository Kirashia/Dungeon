using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunshotController : MovingObject {

    public float damage;
    public float knockback;

    private BoxCollider boxCollider;

	// Use this for initialization
	void Start ()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
	}
	
    public void MoveB(FacingDirection direction)
    {
        if (rb != GetComponent<Rigidbody>())
            rb = GetComponent<Rigidbody>();

        if (boxCollider != GetComponent<BoxCollider>())
            boxCollider = GetComponent<BoxCollider>();


        if (facingDirection != direction) {
            facingDirection = direction;
            //print(facingDirection);
        }

        Vector3 velocity = new Vector3(0, 0, 0);

        switch (direction)
        {
            case FacingDirection.North:
                velocity = new Vector3(0, 0, moveSpeed);
                break;
            case FacingDirection.South:
                velocity = new Vector3(0,0, -moveSpeed);
                break;
            case FacingDirection.East:
                velocity = new Vector3(moveSpeed, 0, 0);
                break;
            case FacingDirection.West:
                velocity = new Vector3(-moveSpeed, 0, 0);
                break;
            case FacingDirection.NorthEast:
                velocity = new Vector3(moveSpeed / 2,0, moveSpeed / 2);
                break;
            case FacingDirection.NorthWest:
                velocity = new Vector3(-moveSpeed / 2,0, moveSpeed / 2);
                break;
            case FacingDirection.SouthEast:
                velocity = new Vector3(moveSpeed, 0,-moveSpeed / 2);
                break;
            case FacingDirection.SouthWest:
                velocity = new Vector3(-moveSpeed / 2, 0,-moveSpeed / 2);
                break;

        }

        rb.velocity = velocity;
        //Debug.Log(rb.velocity);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag+"potato");

        switch (other.tag)
        {
            case "Player":
                return;

            case "Wall":
                Destroy(gameObject);
                break;

            case "Enemy":
                Debug.Log(name + " has hit " + other.name);
                EnemyController e = other.GetComponentInParent<EnemyController>();
                e.TakeDamage(facingDirection, damage, knockback);
                Destroy(gameObject);
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;
        Debug.Log(other.tag);
        switch (other.tag)
        {
            case "Player":
                return;

            case "Wall":
                Destroy(gameObject);
                break;

            case "Enemy":
                Debug.Log(name + " has hit " + other.name);
                EnemyController e = other.GetComponent<EnemyController>();
                e.TakeDamage(facingDirection, damage, knockback);
                Destroy(gameObject);
                break;
        }
    }


    public override IEnumerator Move()
    {
        throw new NotImplementedException();
    }

    public override IEnumerator Attack()
    {
        throw new NotImplementedException();
    }
}
