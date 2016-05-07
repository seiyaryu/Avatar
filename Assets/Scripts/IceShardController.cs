﻿using UnityEngine;
using System.Collections;

public class IceShardController : MonoBehaviour {

    private Rigidbody2D rigidBody;

    public float speed = 10.0f;
    public float repel = 500.0f;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 direction)
    {
        rigidBody.velocity = direction.normalized * speed;
        Vector3 angles = transform.eulerAngles;
        angles.z -= Vector2.Angle(direction, Vector2.left) * Mathf.Sign(direction.y);
        transform.eulerAngles = angles;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<Animator>().SetTrigger("Hurt");

            Vector2 toOther = other.gameObject.transform.position - transform.position;
            other.gameObject.GetComponent<Rigidbody2D>().AddForce(toOther.normalized * repel, ForceMode2D.Force);

            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Scene"))
        {
            Destroy(gameObject);
        }
    }
}