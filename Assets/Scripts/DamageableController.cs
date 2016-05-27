using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageableController : MonoBehaviour {

    public int healthPoints = 10;
    protected int currentHP;

    public float hitMaxCooldown = 1.5f;
    protected float hitCooldown = 0f;

    public DamagePopupController damagePopup;
    private Transform popupOrigin;
    private Transform gameCanvas;

    private Rigidbody2D rigidBody;
    private Animator animator;

    void Start()
    {
        gameCanvas = GameObject.FindGameObjectWithTag("GameCanvas").transform;
    }

    void Awake ()
    {
        popupOrigin = transform.FindChild("PopupOrigin");
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHP = healthPoints;
    }

    void Update ()
    {
        if(hitCooldown > 0f)
        {
            hitCooldown -= Time.deltaTime;
        }
    }

    void PrintDamage(int damage)
    {
        DamagePopupController popup = Instantiate(damagePopup); //Pop damage inflicted
        popup.origin = popupOrigin;
        popup.transform.SetParent(gameCanvas);
        popup.GetComponentInChildren<Text>().text = (-damage).ToString();
    }

    public void OnHit (int damage, Vector2 force)
    {
        if (hitCooldown <= 0f) //Invincible or not
        {
            rigidBody.AddForce(force, ForceMode2D.Force);
            currentHP -= damage;
            if (currentHP > 0) //Still alive
            {
                animator.SetTrigger("Hurt");
                hitCooldown = hitMaxCooldown;

                PrintDamage(damage);
            }
            else //Dead
            {
                Destroy(gameObject);
            }
        }
    }

    public bool IsStunned ()
    {
        return hitCooldown > 0f;
    }
}
