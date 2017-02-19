using UnityEngine;
using System.Collections;

public class Countdown : MonoBehaviour {

    public float lifetime = 1f;

    void Awake()
    {
        Destroy(gameObject, lifetime);
    }
}
