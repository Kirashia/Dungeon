using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MovingObject {

	public float damage;
    public float knockback;

    private PolygonCollider2D collider;

	// Use this for initialization
	void Start ()
    {
        rb2d = GetComponent<Rigidbody2D>();
        collider = GetComponent<PolygonCollider2D>();
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
                break;
        }
    }

    public void Explode(FacingDirection d)
    {
        facingDirection = d;
        StartCoroutine(t());
    }

    public IEnumerator t()
    {
        yield return new WaitForSeconds(1);
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
