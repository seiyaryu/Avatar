using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPilot : FirebenderBase, IDeathListener
{
    [Header("Tank")]

    [SerializeField]
    private Hatch hatch;
    [SerializeField]
    private Turret turret;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BoxCollider2D boxCollider;

    protected override void Awake ()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update ()
    {
        if (boxCollider) boxCollider.enabled = hatch.Open;
    }

    public void MoveIn ()
    {
        spriteRenderer.sortingOrder = -1;
    }

    public void MoveOut()
    {
        spriteRenderer.sortingOrder = 1;
    }

    void Orient (Vector2 target)
    {
        Vector3 toTarget = transform.InverseTransformVector((Vector3)(target) - transform.position);
        if (toTarget.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    public override bool GetTarget (out Vector2 target)
    {
        if (hatch.Open && base.GetTarget(out target))
        {
            Orient(target);
            return true;
        }
        else
        {
            target = Vector2.zero;
            return false;
        }
    }

    public override void OnShoot(Projectile projectile)
    {
        animator.SetTrigger("Firing");
    }

    public void OnDeath()
    {
        turret.OnPilotDeath();
        Destroy(boxCollider);
        Destroy(gameObject, 5f);
    }
}