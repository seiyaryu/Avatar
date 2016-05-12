using UnityEngine;
using System.Collections;

public class IceShatterController : MonoBehaviour {

    public float shatterLifetime = 1f;

    void Awake()
    {
        Destroy(gameObject, shatterLifetime);
    }
}
