using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MovingObject{

    public GameObject player;
    public Vector3 target;
    public bool reached = false;
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
        Debug.Log("test: "+target);

        target = player.transform.position;
        float sqrRemaining = Vector3.SqrMagnitude(transform.position - target);
        agent.SetDestination(target);

        while (sqrRemaining > float.Epsilon && target == player.transform.position)
        {
            sqrRemaining = Vector3.SqrMagnitude(transform.position - target);
            yield return null;
        }
    }

    // Update is called once per frame
    void Update () {
        CheckIfDead();

        if (!reached)
            StartCoroutine(Move());

        if (dead)
        {
            Debug.Log(name + " has died");
            enabled = false;
        }
	}
}
