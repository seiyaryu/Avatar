using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Damageable : MonoBehaviour {

    [Header("HP")]

    [SerializeField]
    private int maximumHP = 10;
    private int currentHP;

    [SerializeField]
    private float hitCooldown = 1.5f;
    private float hitTimer = 0f;

    [Header("Listeners")]

    [SerializeField]
    private MonoBehaviour deathListener;
    private IDeathListener deathController;
    [SerializeField]
    private MonoBehaviour damageListener;
    private IDamageListener damageController;

    [Header("Printing Damage")]

    [SerializeField]
    private Transform gameCanvas;
    [SerializeField]
    private Popup damagePopup;
    [SerializeField]
    private Transform popupOrigin;

    private Rigidbody2D rigidBody;
    private Animator animator;

    void Awake ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        deathController = deathListener as IDeathListener;
        damageController = damageListener as IDamageListener;

        currentHP = maximumHP;

        if (animator) animator.SetFloat("CooldownSpeed", 1f / hitCooldown);
    }

    void Update ()
    {
        if (hitTimer > 0f)
        {
            hitTimer -= Time.deltaTime;
        }
    }

    void PrintDamage(int damage)
    {
        Popup popup = Instantiate(damagePopup); //Pop damage inflicted
        popup.origin = popupOrigin;
        popup.transform.SetParent(gameCanvas, false);
        popup.Text = (-damage).ToString();
    }

    public void OnHit (int damage, Vector2 force)
    {
        if(Alive)
        {
            if (hitTimer <= 0f) //Invincible or not
            {
                if (rigidBody) rigidBody.AddForce(force, ForceMode2D.Impulse);

                currentHP -= damage;

                PrintDamage(damage);

                if (currentHP > 0) //Still alive
                {
                    if (animator) animator.SetTrigger("Hurt");

                    hitTimer = hitCooldown;

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
        if (damageController != null)
        {
            damageController.OnDamaged(damage);
        }
    }

    void OnDeath()
    {
        if (deathController != null)
        {
            deathController.OnDeath();
        }
    }

    public Transform GameCanvas
    {
        set { gameCanvas = value; }
    }

    public bool Stunned
    {
        get { return currentHP <= 0 || hitTimer > 0f; }
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
