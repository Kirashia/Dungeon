using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MovingObject{

    public GameObject player;
    public Vector3 target;

    private NavMeshAgent agent;

    public override IEnumerator Attack()
    {
        throw new NotImplementedException();
    }

    public void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb2d = GetComponent<Rigidbody2D>();
        inverseAttackSpeed = 1 / attackSpeed;
        health = 10;
    }

    public override IEnumerator Move()
    {
        target = player.transform.position;
        Debug.Log("mOVING TOWARDS PLAYER AT: " + target);
        agent.SetDestination(target);
        yield return null;
    }

    public override void TakeDamage(FacingDirection directionOfDamage, float amountOfDamage, float amountOfKnockback)
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

    // Update is called once per frame
    void Update () {
        CheckIfDead();
        StartCoroutine(Move());
        if (dead)
        {
            Debug.Log(name + " has died");
            enabled = false;
        }
	}
}
