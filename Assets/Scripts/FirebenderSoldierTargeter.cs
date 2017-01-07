using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebenderSoldierTargeter : FirebenderTargeter
{
    private Rigidbody2D rigidBody;

    protected override void Awake ()
    {
        base.Awake();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    public override void OnShoot()
    {
        base.OnShoot();
        rigidBody.velocity = Vector2.zero;
    }
}
