using UnityEngine;
using System.Collections;

public class CountdownController : MonoBehaviour {

    public float lifetime = 1f;

    void Awake()
    {
        Destroy(gameObject, lifetime);
    }
}
