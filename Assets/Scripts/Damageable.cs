using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Damageable : MonoBehaviour {

    [Header("HP")]

    public int maximumHP = 10;
    private int currentHP;

    public float hitMaxCooldown = 1.5f;
    private float hitCooldown = 0f;

    [Header("Listeners")]

    public MonoBehaviour deathListener;
    public MonoBehaviour damageListener;

    [Header("Printing Damage")]

    public Transform gameCanvas;
    public PopupController damagePopup;
    public Transform popupOrigin;

    private Rigidbody2D rigidBody;
    private Animator animator;

    void Awake ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHP = maximumHP;
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
        PopupController popup = Instantiate(damagePopup); //Pop damage inflicted
        popup.origin = popupOrigin;
        popup.transform.SetParent(gameCanvas, false);
        popup.Text = (-damage).ToString();
    }

    public void OnHit (int damage, Vector2 force)
    {
        if(Alive)
        {
            if (hitCooldown <= 0f) //Invincible or not
            {
                if (rigidBody) rigidBody.AddForce(force, ForceMode2D.Impulse);
                currentHP -= damage;
                if (currentHP > 0) //Still alive
                {
                    if (animator) animator.SetTrigger("Hurt");
                    hitCooldown = hitMaxCooldown;

                    PrintDamage(damage);

                    OnDamaged(damage);
                }
                else //Dead
                {
                    if (animator) animator.SetTrigger("Death");

                    OnDeath();
                }
            }
        }
    }

    void OnDamaged(int damage)
    {
        if (damageListener)
        {
            if (damageListener is IDamageListener)
            {
                IDamageListener listener = damageListener as IDamageListener;
                listener.OnDamaged(damage);
            }
            else
            {
                damageListener = null;
            }
        }
    }

    void OnDeath()
    {
        if (deathListener)
        {
            IDeathListener listener = deathListener as IDeathListener;
            if (listener != null)
            {
                
                listener.OnDeath();
            }
            else
            {
                deathListener = null;
            }
        }
    }

    public bool Stunned
    {
        get { return currentHP == 0 || hitCooldown > 0f; }
    }

    public bool Alive
    {
        get { return currentHP > 0; }
    }

    public int CurrentHP
    {
        get { return currentHP; }
    }

    public int MaxHP
    {
        get { return maximumHP; }
    }
}
