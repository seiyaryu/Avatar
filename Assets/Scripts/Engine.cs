using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{

    public Transform Player
    {
        set { water = value.GetComponentInChildren<WaterFlask>(); }
    }
    private WaterFlask water;

    [Header("Tank Parts")]

    [SerializeField]
    private Pipe pipe;
    private bool pipeBlocked;

    [SerializeField]
    private Hatch hatch;

    [Header("Overheat")]

    [SerializeField]
    private int overheat;
    private int heat;

    [SerializeField]
    private float heatingDelay;
    private float heatingTimer;

    [SerializeField]
    private AudioSource overheatingSound;

    private SpriteRenderer spriteRenderer;
    private Damageable damageable;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageable = GetComponent<Damageable>();

        heatingTimer = heatingDelay;
    }

    void HeatUpEngine ()
    {
        if (heat > 0)
        {
            float ratio = (float)(heat) / overheat;
            spriteRenderer.material.SetFloat("_Flash", ratio);
            overheatingSound.volume = ratio;
            if (!overheatingSound.isPlaying)
            {
                overheatingSound.Play();
            }
            if (heat == overheat)
            {
                damageable.OnHit(1, Vector2.zero);
            }
        }
        else if (overheatingSound.isPlaying)
        {
            overheatingSound.Stop();
        }
    }

    void Update ()
    {
        HeatUpEngine();

        if (heatingTimer > 0f)
        {
            heatingTimer -= Time.deltaTime;
        }
    }

    public void EmitFumes ()
    {
        if (!pipeBlocked)
        {
            if (heat > 0)
            {
                --heat;
            }

            pipe.StartEmitFumes();
        }
        else
        {
            if (heat < overheat && heatingTimer <= 0f)
            {
                ++heat;
                heatingTimer = heatingDelay;
            }

            pipe.StopEmitFumes();

            if (hatch.Closed)
            {
                hatch.Unlock();
            }
        }
    }

    public void Restart ()
    {
        pipe.StopEmitFumes();
        if (hatch.Open)
        {
            hatch.Lock();
        }
    }

    void FixedUpdate()
    {
        pipeBlocked = CheckPipe();
    }

    bool CheckPipe()
    {
        if (water.Frozen)
        {
            Collider2D[] colliders = water.waterDrop.GetComponents<Collider2D>();
            return pipe.IsBlocked(colliders);
        }
        else
        {
            return false;
        }
    }

    public bool Overheating
    {
        get { return heat > 0; }
    }
}