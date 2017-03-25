using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour {

    public void PlayBoatWhistle ()
    {
        AudioSource whistle = GetComponent<AudioSource>();
        if (whistle)
        {
            whistle.Play();
        }
    }

    public void Leave ()
    {
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.SetTrigger("Leave");
        }
    }
}
