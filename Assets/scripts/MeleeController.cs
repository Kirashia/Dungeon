using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeController : MovingObject
{

    public float damage;
    public float knockback;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void OnTriggerEnter(Collider other)
    {
        Debug.Log("collide");
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
                break;
        }
    }

    public void Attack(FacingDirection d)
    {
        facingDirection = d;
        StartCoroutine(t());
    }

    public IEnumerator t()
    {
        yield return new WaitForSeconds(.4f);
        Destroy(gameObject);
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
