using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hatch : MonoBehaviour {

    private Animator animator;

    void Awake ()
    {
        animator = GetComponent<Animator>();
    }

    public void Unlock()
    {
        animator.SetTrigger("Open");
        gameObject.tag = "Untagged";
    }

    public void Lock()
    {
        animator.SetTrigger("Close");
        gameObject.tag = "Enemy";
    }

    public bool Open
    {
        get { return animator.GetCurrentAnimatorStateInfo(0).IsName("OpenTankHatch"); }
    }

    public bool Closed
    {
        get { return animator.GetCurrentAnimatorStateInfo(0).IsName("ClosedTankHatch"); }
    }
}
