﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunshotController : MovingObject {

    public float damage;
    public float knockback;

    private BoxCollider2D boxCollider;

	// Use this for initialization
	void Start ()
    {
        rb2d = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
	}
	
    public void MoveB(FacingDirection direction)
    {
        if (rb2d != GetComponent<Rigidbody2D>())
            rb2d = GetComponent<Rigidbody2D>();

        if (boxCollider != GetComponent<BoxCollider2D>())
            boxCollider = GetComponent<BoxCollider2D>();


        if (facingDirection != direction) {
            facingDirection = direction;
            //print(facingDirection);
        }

        Vector3 velocity = new Vector3(0, 0);

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

        transform.Translate(velocity);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.tag) {
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

    public override void TakeDamage(FacingDirection directionOfDamage, float amountOfDamage, float amountOfKnockback)
    {
        throw new NotImplementedException();
    }
}
